// -----------------------------------------------------------------------------
//This is free and unencumbered software released into the public domain.
//Anyone is free to copy, modify, publish, use, compile, sell, or
//distribute this software, either in source code form or as a compiled
//binary, for any purpose, commercial or non-commercial, and by any
//means.
//In jurisdictions that recognize copyright laws, the author or authors
//of this software dedicate any and all copyright interest in the
//software to the public domain.We make this dedication for the benefit
//of the public at large and to the detriment of our heirs and
//successors.We intend this dedication to be an overt act of
//relinquishment in perpetuity of all present and future rights to this
//software under copyright law.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
//OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
//OTHER DEALINGS IN THE SOFTWARE.
//For more information, please refer to<http://unlicense.org>
// ----------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Office.Server.Utilities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// this job runs every minute and if there is any data in the queue
	/// it will kick of a list initialization job
	/// </summary>
	[Guid("8433B17A-12EC-4617-8B1A-FBA705A46DC7")]
	public class InitializationQueueMonitorJob : SPJobDefinition
	{
		public const string JOB_NAME = "RemarQInitializationQueueMonitorJob";

		/// <summary>
		/// use this CTOR
		/// </summary>
		/// <param name="jobName"></param>
		public InitializationQueueMonitorJob(string jobName)
			: base(jobName, SPAdministrationWebApplication.Local, null, SPJobLockType.Job)
		{
			//if for some reason our title comes back missing we have to provide one
			//otherwise the job status page will throw an error
			string jobTitle = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "BalsamicSolutionsQueueMonitorJobTitle");
			if (string.IsNullOrWhiteSpace(jobTitle) || jobTitle.IndexOf("ERROR", 0, jobTitle.Length, StringComparison.OrdinalIgnoreCase) > -1)
			{
				jobTitle = "RemarQ Queue Monitor";
			}
			this.Title = jobTitle;
		}

		/// <summary>
		/// required for serilization
		/// </summary>
		public InitializationQueueMonitorJob()
			: base()
		{
		}

		/// <summary>
		/// execute the job
		/// </summary>
		/// <param name="targetInstanceId"></param>
		public override void Execute(Guid targetInstanceId)
		{
			RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.InitializationQueueMonitorJob.Execute");
			FarmLicense.ExpireCachedObject();
			FarmSettings.ExpireCachedObject();
		 
			using (SPMonitoredScope spScope = new SPMonitoredScope(JOB_NAME))
			{
				this.UpdateProgress(0);
				FarmSettings farmSettings = FarmSettings.Settings;
				if (null != farmSettings && farmSettings.IsOk && farmSettings.Activated && !farmSettings.ExternalJobManager && FarmLicense.License.IsLicensed())
				{
					if (!ListInitializationJob.IsRunning)
					{
						if (SqlRemarQ.QueueCount(Constants.ReadUnreadQueueTableName) > 0)
						{
							if (!ListInitializationJob.RunJobNow())
							{
								if (ListInitializationJob.IsInstalled())
								{
									throw new RemarQException("Could not execute List Initialization Job");
								}
								else
								{
									throw new RemarQException("Could not find List Initialization Job");
								}
							}
						}
					}
					this.UpdateProgress(50);
					if (!LargeListInitializationJob.IsRunning)
					{
						if (SqlRemarQ.QueueCount(Constants.ReadUnreadLargeQueueTableName) > 0)
						{
							if (!LargeListInitializationJob.RunJobNow())
							{
								if (LargeListInitializationJob.IsInstalled())
								{
									throw new RemarQException("Could not execute Large List Initialization Job");
								}
								else
								{
									throw new RemarQException("Could not find Large List Initialization Job");
								}
							}
						}
					}
				}
				this.UpdateProgress(100);
			}

			RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.InitializationQueueMonitorJob.Execute");
		}

		/// <summary>
		/// install this job in the admin site
		/// </summary>
		public static void Install()
		{
			UnInstall();
			InitializationQueueMonitorJob spJob = new InitializationQueueMonitorJob(JOB_NAME);
			SPMinuteSchedule jobSchedule = new SPMinuteSchedule();
			jobSchedule.BeginSecond = 0;
			jobSchedule.EndSecond = 59;
			jobSchedule.Interval = 1;
			spJob.Schedule = jobSchedule;
			spJob.Update();
		}

		/// <summary>
		/// remove this job from the farm
		/// </summary>
		public static void UnInstall()
		{
			List<SPJobDefinition> jobsToDelete = new List<SPJobDefinition>();
			foreach (SPJobDefinition spJob in SPAdministrationWebApplication.Local.JobDefinitions)
			{
				if (spJob.Name == JOB_NAME)
				{
					jobsToDelete.Add(spJob);
				}
			}
			 
			foreach (SPJobDefinition spJob in jobsToDelete)
			{
				spJob.Delete();
			}
		}

		public static bool IsRunning
		{
			get
			{
				foreach (SPRunningJob spJob in SPAdministrationWebApplication.Local.RunningJobs)
				{
					if (null != spJob.JobDefinition && null != spJob.JobDefinition.Name)
					{
						if (spJob.JobDefinition.Name == JOB_NAME)
						{
							return true;
						}
					}
				}

				return false;
			}
		}
	}
}