// -----------------------------------------------------------------------------
//  Copyright 8/11/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// the administrator can kick offf this job
	/// to remove all instances of the ReadUnreadField
	/// on all registered lists
	/// </summary>
	[Guid("76D2AC2A-F2DE-445B-8645-EFFACF151246")]
	public class RemoveRemarQJob : SPJobDefinition
	{
		const string JOB_NAME = "RemarQRemoveEverythingJob";
	 
		Decimal _PercentCompletePerList = 0;
		Decimal _PercentComplete = 0;

		/// <summary>
		/// use this CTOR
		/// </summary>
		/// <param name="jobName"></param>
		protected RemoveRemarQJob(string jobName)
			: base(jobName, SPAdministrationWebApplication.Local, null, SPJobLockType.Job)
		{
			//if for some reason our title comes back missing we have to provide one
			//otherwise the job status page will throw an error
			string jobTitle = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "BalsamicSolutionsRemoveAllFieldsJobTitle");
			if (string.IsNullOrWhiteSpace(jobTitle) || jobTitle.IndexOf("ERROR", 0, jobTitle.Length, StringComparison.OrdinalIgnoreCase) > -1)
			{
				jobTitle = "RemarQ Remove Everything";
			}
			this.Title = jobTitle;
		}

		/// <summary>
		/// required for serialization
		/// </summary>
		public RemoveRemarQJob()
			: base()
		{
		}

		/// <summary>
		/// anallyze all the lists in a particulare web
		/// </summary>
		/// <param name="listWeb"></param>
		/// <param name="allLists"></param>
		void ProcessAllListsInWeb(SPWeb listWeb, ICollection<Guid> allLists)
		{
			foreach (Guid listId in allLists)
			{
				try
				{
					SPList targetList = listWeb.Lists[listId];
					List<ReadUnreadField> deleteThese = Utilities.FindFieldsOnList(targetList);
					foreach (ReadUnreadField deleteMe in deleteThese)
					{
						deleteMe.Delete();
						targetList.Update();
					}
				}
				catch (ArgumentException)
				{
					//bad list id
				}
				catch (SPException)
				{
					//bad list id
				}
				try
				{
					if (FarmSettings.Settings.IsOk)
					{
						SqlRemarQ.DeProvisionReadUnreadTableAndRemoveConfigurationEntry(listId);
					}
				}
				catch (SqlException)
				{
					//bad table format
				}
				this._PercentComplete += this._PercentCompletePerList;
				int reportingPercentage = (int)Math.Round(this._PercentComplete, 0);
				if (reportingPercentage > 99)
				{
					reportingPercentage = 99;
				}
				this.UpdateProgress(reportingPercentage);
			}
		}

		/// <summary>
		/// analyze all the webs in a site collection
		/// </summary>
		/// <param name="listSite"></param>
		/// <param name="allListsInFarm"></param>
		void ProcessAllWebsInSite(SPSite listSite, SharePointListDictionary allListsInFarm)
		{
			foreach (Guid webId in allListsInFarm.WebIds(listSite.ID))
			{
				SPWeb listWeb = null;
				try
				{
					listWeb = listSite.AllWebs[webId];
					this.ProcessAllListsInWeb(listWeb, allListsInFarm.ListIds(listSite.ID, webId));
				}
				catch (ArgumentException)
				{
					//bad web Id
				}
				catch (SPException)
				{
					//bad web Id
				}
				finally
				{
					if (null != listWeb)
					{
						listWeb.Dispose();
					}
				}
			}
		}
		
		static bool RemarQIsActive(SPSite testMe)
		{
			bool returnValue = true;
			try
			{
				SPField remarQField = testMe.RootWeb.AvailableFields["RemarQ"];
				returnValue = (null != remarQField);
			}
			catch (ArgumentException)
			{
				returnValue = false;
			}
			return returnValue;
		}

		/// <summary>
		/// execute this job
		/// </summary>
		/// <param name="targetInstanceId"></param>
		public override void Execute(Guid targetInstanceId)
		{
			RemarQLog.LogMessage("Entering  BalsamicSolutions.ReadUnreadSiteColumn.RemoveRemarQJob.Execute");
	 
			using (SPMonitoredScope spScope = new SPMonitoredScope(JOB_NAME))
			{
				this.UpdateProgress(0);
				SharePointListDictionary allListsInFarm = new SharePointListDictionary();
				SPWebService webSvc = this.Farm.Services.GetValue<SPWebService>(string.Empty);
				foreach (SPWebApplication webApp in webSvc.WebApplications)
				{
					foreach (SPSite webSite in webApp.Sites)
					{
						foreach (SPWeb siteWeb in webSite.AllWebs)
						{
							foreach (SPList webList in siteWeb.Lists)
							{
								allListsInFarm.Add(webSite.ID, siteWeb.ID, webList.ID);
							}
							siteWeb.Dispose();
						}
						webSite.Dispose();
					}
				}
				int listCount = allListsInFarm.Count + 5; //5 for other functions
				if (listCount > 0)
				{
					this._PercentCompletePerList = Decimal.Divide(100, listCount);
					Guid featureGuid = new Guid("830c7e22-415b-4efb-bee3-407fa24faeb1");
					foreach (Guid siteId in allListsInFarm.SiteIds())
					{
						SPSite listSite = null;
						try
						{
							listSite = new SPSite(siteId);
							this.ProcessAllWebsInSite(listSite, allListsInFarm);
							if (RemarQIsActive(listSite))
							{
								listSite.RootWeb.Features.Remove(featureGuid);
								listSite.RootWeb.Update();
							}
						}
						catch (ArgumentException argError)
						{
							RemarQLog.TraceError(argError);
						}
						catch (SPException spError)
						{
							RemarQLog.TraceError(spError);
						}
						finally
						{
							if (null != listSite)
							{
								listSite.Dispose();
							}
						}
					}
				}
				this.UpdateProgress(95);
				FarmSettings farmSettings = FarmSettings.Settings;
				if (null != farmSettings && farmSettings.IsOk)
				{
					//now remove all tables and entries in configuration that might be
					//stragglers
					List<ListConfiguration> allKnownLists = SqlRemarQ.GetListConfigurations();
					foreach (ListConfiguration listConfig in allKnownLists)
					{
						SqlRemarQ.DeProvisionReadUnreadTableAndRemoveConfigurationEntry(listConfig.ListId);
					}
				}
				this.UpdateProgress(96);
				SPPersistedObject settingsObj = SPFarm.Local.GetObject(FarmSettings.SettingsId);
				if (null != settingsObj)
				{
					settingsObj.Delete();
				}
				this.UpdateProgress(97);
				
				FarmSettings.Remove();
				this.UpdateProgress(98);
			}
			this.UpdateProgress(100);
			RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.RemoveRemarQJob.Execute");
		}

		/// <summary>
		/// install this job to run now
		/// </summary>
		public static void Install()
		{
			UnInstall();
			RemoveRemarQJob spJob = new RemoveRemarQJob(JOB_NAME);
			spJob.RunNow();
		}

		/// <summary>
		/// remove any existing jobs
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