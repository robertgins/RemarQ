// -----------------------------------------------------------------------------
//  Copyright 8/29/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
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
	/// this is a variation of the list initialization job
	/// that handles large lists (over 5000 items) or
	/// user requested updates from the list UI
	/// </summary>
	[Guid("D6179ABC-1283-47B1-8D23-93880F91B967")]
	public class LargeListInitializationJob : SPJobDefinition
	{
		public const string JOB_NAME = "RemarQLargeListInitializationJob";
	 
		/// <summary>
		/// use this CTOR
		/// </summary>
		/// <param name="jobName"></param>
		public LargeListInitializationJob(string jobName)
			: base(jobName, SPAdministrationWebApplication.Local, null, SPJobLockType.Job)
		{
			//if for some reason our title comes back missing we have to provide one
			//otherwise the job status page will throw an error
			string jobTitle = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "BalsamicSolutionsLargeListInitializationJobTitle");
			if (string.IsNullOrWhiteSpace(jobTitle) || jobTitle.IndexOf("ERROR", 0, jobTitle.Length, StringComparison.OrdinalIgnoreCase) > -1)
			{
				jobTitle = "RemarQ Large List Initialization";
			}
	 
			this.Title = jobTitle;
		}

		/// <summary>
		/// required for serilization
		/// </summary>
		public LargeListInitializationJob()
			: base()
		{
		}

		/// <summary>
		/// execute the job
		/// </summary>
		/// <param name="targetInstanceId"></param>
		public override void Execute(Guid targetInstanceId)
		{
			RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.LargeListInitializationJob.Execute");  
			FarmLicense.ExpireCachedObject();
			FarmSettings.ExpireCachedObject();
		 
			using (SPMonitoredScope spScope = new SPMonitoredScope(JOB_NAME))
			{
				this.UpdateProgress(0);
				FarmSettings farmSettings = FarmSettings.Settings;
				if (null != farmSettings && farmSettings.IsOk && farmSettings.Activated && !farmSettings.ExternalJobManager && FarmLicense.License.IsLicensed())
				{
					ListInitializationProcessor batchProcessor = new ListInitializationProcessor(Constants.ReadUnreadLargeQueueTableName,JOB_NAME,this);
					batchProcessor.Execute(); 
				}
				this.UpdateProgress(100);
			}
			RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.LargeListInitializationJob.Execute");  
		}
			
		/// <summary>
	 
		public static bool RunJobNow()
		{
			bool returnValue = false;
			foreach (SPJobDefinition spJob in SPAdministrationWebApplication.Local.JobDefinitions)
			{
				if (spJob.Name == JOB_NAME)
				{
					spJob.RunNow();
					returnValue = true;
					break;
				}
			}
			return returnValue;
		}

		/// <summary>
		/// install this job in the admin site
		/// </summary>
		internal static void Install()
		{
			UnInstall();
			LargeListInitializationJob spJob = new LargeListInitializationJob(JOB_NAME);
			DateTime nextMonth = DateTime.Now.AddMonths(2); 
			SPYearlySchedule jobSchedule = new SPYearlySchedule();
			jobSchedule.BeginDay = 1;
			jobSchedule.BeginMonth = nextMonth.Month;
			jobSchedule.BeginSecond = 1;
			jobSchedule.BeginHour = 1;
			jobSchedule.BeginMinute = 1;
			jobSchedule.EndDay = 10;
			jobSchedule.EndHour = 5;
			jobSchedule.EndMinute = 59;
			jobSchedule.EndMonth = 1;
			jobSchedule.EndSecond = 59;
		 
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
	 
		public static bool IsInstalled()
		{
			foreach (SPJobDefinition spJob in SPAdministrationWebApplication.Local.JobDefinitions)
			{
				if (spJob.Name == JOB_NAME)
				{
					return true;
				}
			}
			return false;
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