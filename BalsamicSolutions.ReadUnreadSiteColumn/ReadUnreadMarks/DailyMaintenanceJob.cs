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
using System.Globalization;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// this job runs daily and makes sure that the configuration table
	/// accurately reflects all of the existing ReadUnreadFields and
	/// that no duplicate field entries exist on configured lists
	/// It also handles processing any feature upgrades that are due
	/// to a change in licensing
	/// </summary>
	[Guid("68623F85-09CC-47F5-96F9-5D4A4BAB1C65")]
 
	public class DailyMaintenanceJob : SPJobDefinition
	{
		const string JOB_NAME = "RemarQDailyMaintenanceJob";
		 
		Decimal _PercentCompletePerList = 0;
		Decimal _PercentComplete = 0;

		/// <summary>
		/// use this CTOR
		/// </summary>
		/// <param name="jobName"></param>
		public DailyMaintenanceJob(string jobName)
			: base(jobName, SPAdministrationWebApplication.Local, null, SPJobLockType.Job)
		{
			//if for some reason our title comes back missing we have to provide one
			//otherwise the job status page will throw an error
			string jobTitle = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"DailyMaintenanceJobTitle");
			if (string.IsNullOrWhiteSpace(jobTitle) || jobTitle.IndexOf("ERROR", 0, jobTitle.Length, StringComparison.OrdinalIgnoreCase) > -1)
			{
				jobTitle = "RemarQ Daily Maintenance";
			}
			this.Title = jobTitle;
		}

		/// <summary>
		/// required for serialization
		/// </summary>
		public DailyMaintenanceJob()
			: base()
		{
		}

		/// <summary>
		/// verify all our objects are correctly configured for a specific ReadUnreadField
		/// </summary>
		/// <param name="listConfig"></param>
		/// <param name="checkMe"></param>
		/// <param name="parentList"></param>
		static void ValidateReadUnreadFieldAndConfiguration(ListConfiguration listConfig, ReadUnreadField checkMe, SPList parentList)
		{
			//WE have a field and a config record so check and make sure our supporting objects
			//are correctly configured 
			bool reprocessList = false;
			if (!ReadUnreadField.AreAllEventHandlersInstalled(checkMe.ParentList))
			{
				ReadUnreadField.UninstallAllEventHandlersInternal(checkMe.ParentList);
				ReadUnreadField.InstallEventHandlersInternal(checkMe.ParentList);
				RemarQLog.LogWarning("BalsamicSolutionsDailyMaintenanceJob  found an misconfigured event handler for " + parentList.Title);
				reprocessList = true;
			}
								 
			if (!reprocessList)
			{
				//list reprocessing may occasionally be queued for
				//heavily reused lists but for now this is a workaround
				//for servers that crash while events are being processed
				int itemCount = checkMe.ParentList.ItemCount;
				int hierarchyItemcount = SqlRemarQ.HierarchyItemcount(listConfig.ListId);
				//we are just going to swag it, assuming we are ok if close, 
				//if not they can trip the rebuild on the list toolbar
				reprocessList = Math.Abs(itemCount - hierarchyItemcount) > 10;
			}

			if (reprocessList)
			{
				RemarQLog.LogWarning("BalsamicSolutionsDailyMaintenanceJob  found a list needing a rescan for " + parentList.Title);
				SqlRemarQ.QueueListCommand(checkMe.ParentList, ListCommand.ReInitialize);
			}
			else
			{
				if (!ReadUnreadField.AreAllContextMenusInstalled(checkMe.ParentList, listConfig.ContextMenu))
				{
					RemarQLog.LogWarning("BalsamicSolutionsDailyMaintenanceJob  found an misconfigured context menu for " + parentList.Title);
					SqlRemarQ.QueueListCommand(checkMe.ParentList, ListCommand.Verify);
				}
			}
		}

		/// <summary>
		/// flags an orphaned field for reinitialization
		/// </summary>
		/// <param name="initalizeMe"></param>
		static void ReInitializeField(ReadUnreadField initalizeMe)
		{
			SqlRemarQ.ProvisionReadUnreadTable(initalizeMe.ParentList.ID);
			SqlRemarQ.CreateListConfiguration(initalizeMe.ParentList.ID,
				initalizeMe.ParentList.ParentWeb.ID,
				initalizeMe.ParentList.ParentWeb.Site.ID,
				initalizeMe.Id,
				ColumnRenderMode.BoldDisplay,
				Constants.DefaultReadImageUrl,
				Constants.DefaultUnreadImageUrl,
				Constants.DefaultUnreadColor,
				Constants.DefaultUnreadColor,
				ListConfiguration.ContextMenuType.All,
				ListConfiguration.VersionUpdateType.All,
				initalizeMe.InternalName,
				initalizeMe.ParentList.ParentWeb.Language,
				initalizeMe.ParentList.ParentWebUrl);
			SqlRemarQ.QueueListCommand(initalizeMe.ParentList, ListCommand.ReInitialize);
		}

		/// <summary>
		/// forces the EnableMinimalDownload to be false
		/// TODO we want to fix this but the support for the Javascript 
		/// version (aka RegisterModuleInit) is not working adaquately
		/// in base or SPq 
		/// </summary>
		/// <param name="siteWeb"></param>
		void DisableMinimalDownload(SPWeb siteWeb)
		{
			if (siteWeb.EnableMinimalDownload)
			{
				siteWeb.EnableMinimalDownload = false;
				siteWeb.Update();
				RemarQLog.LogWarning("Minimal Download disabled for " + siteWeb.Title);
			}
		}

		/// <summary>
		/// process all lists that have a read unread field
		/// and verify there are no duplicates
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

					List<ReadUnreadField> validateThese = Utilities.FindFieldsOnList(targetList);
					ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(listId);
					if (validateThese.Count > 0 || null != listConfig)
					{ 
						//what is the correct FieldId
						Guid existingFieldId = (null == listConfig) ? validateThese[0].Id : listConfig.FieldId;
						
						this.DisableMinimalDownload(listWeb);

						ReadUnreadField actualField = null;
						//If more than one field delete the stragglers, should never happen
						//but programmers could force it from a custom project
						for (int fieldNum = 0; fieldNum < validateThese.Count; fieldNum++)
						{ 
							if (validateThese[fieldNum].Id != existingFieldId)
							{
								RemarQLog.LogWarning("BalsamicSolutionsDailyMaintenanceJob  found a list with more than one RemarQ field" + targetList.Title);
								validateThese[fieldNum].DeleteWithoutDeprovisioning();
							}
							else
							{
								actualField = validateThese[fieldNum];
							}
						}

						if (null == listConfig)
						{
							//we have a field with no configuration record, so we will upate it if its a supported type
							if (null != actualField)
							{
								RemarQLog.LogWarning("BalsamicSolutionsDailyMaintenanceJob  found an orphaned RemarQ field in " + targetList.Title);
								if (actualField.ParentListIsSupportedListType)
								{
									ReInitializeField(actualField);
								}
								else
								{
									actualField.Delete();
								}
							}
						}
						else
						{
							if (null == actualField)
							{
								//No field but we had a configureation record so we will remove the configuration record
								RemarQLog.LogWarning("BalsamicSolutionsDailyMaintenanceJob  found an orphaned configuration record for " + listConfig.ListId.ToString() + listConfig.LayoutsPath);
								SqlRemarQ.DeProvisionReadUnreadTableAndRemoveConfigurationEntry(listConfig.ListId);
							}
							else
							{
								//Good record, good field  so check all our objects
								ValidateReadUnreadFieldAndConfiguration(listConfig, actualField, targetList);
							}
						}
					}
				}
				catch (ArgumentException)
				{
					System.Diagnostics.Trace.Write("ArgumentException");
				}
				catch (SPException)
				{
					System.Diagnostics.Trace.Write("SPException");
				}
				this._PercentComplete += this._PercentCompletePerList;
				int reportingPercentage = (int)Math.Round(this._PercentComplete, 0);
				this.UpdateProgress(reportingPercentage);
			}
		}

		/// <summary>
		/// process all the sub webs in a particular site collection
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
					using (SPMonitoredScope spScope = new SPMonitoredScope(JOB_NAME + " " + listWeb.Title))
					{
						this.ProcessAllListsInWeb(listWeb, allListsInFarm.ListIds(listSite.ID, webId));
					}
				}
				catch (ArgumentException)
				{
					System.Diagnostics.Trace.Write("Bad Web Id");
				}
				catch (SPException)
				{
					System.Diagnostics.Trace.Write("SPException");
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
		
		/// <summary>
		/// removes any list configuration record
		/// with bad web or list id's we leave site
		/// </summary>
		void RemoveBadListConfigurationRecords()
		{
			foreach (ListConfiguration listConfig in SqlRemarQ.GetListConfigurations())
			{
				try
				{
					using (SPSite rootSite = new SPSite(listConfig.SiteId))
					{
						try
						{
							using (SPWeb listWeb = rootSite.AllWebs[listConfig.WebId])
							{
								try
								{
									SPList remarQList = listWeb.Lists[listConfig.ListId];
									System.Diagnostics.Trace.WriteLine(remarQList.Title);
								}
								catch (SPException missingList)
								{
									System.Diagnostics.Trace.WriteLine(missingList.Message);
									RemarQLog.LogWarning("BalsamicSolutionsDailyMaintenanceJob  found an orphaned configuration record for " + listConfig.ListId.ToString() + listConfig.LayoutsPath);
									SqlRemarQ.DeProvisionReadUnreadTableAndRemoveConfigurationEntry(listConfig.ListId);
								}
							}
						}
						catch (ArgumentException missingWeb)
						{
							System.Diagnostics.Trace.WriteLine(missingWeb.Message);
							RemarQLog.LogWarning("BalsamicSolutionsDailyMaintenanceJob  found an orphaned configuration record for " + listConfig.ListId.ToString() + listConfig.LayoutsPath);
							SqlRemarQ.DeProvisionReadUnreadTableAndRemoveConfigurationEntry(listConfig.ListId);
						}
					}
				}
				catch (FileNotFoundException missingSite)
				{
					System.Diagnostics.Trace.WriteLine(missingSite.Message);
					RemarQLog.LogWarning("BalsamicSolutionsDailyMaintenanceJob  found an orphaned configuration record for " + listConfig.ListId.ToString() + listConfig.LayoutsPath);
					SqlRemarQ.DeProvisionReadUnreadTableAndRemoveConfigurationEntry(listConfig.ListId);
				}
			}
		}

		/// <summary>
		/// execute implementation
		/// </summary>
		/// <param name="targetInstanceId"></param>
		public override void Execute(Guid targetInstanceId)
		{
			RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.DailyMaintenanceJob.Execute");
			FarmLicense.ExpireCachedObject();
			FarmSettings.ExpireCachedObject();
		 
			using (SPMonitoredScope spScope = new SPMonitoredScope(JOB_NAME))
			{
				this.UpdateProgress(0);
				FarmSettings farmSettings = FarmSettings.Settings;
				if (null != farmSettings && farmSettings.IsOk && farmSettings.Activated && FarmLicense.License.IsLicensed())
				{
					if (farmSettings.TrackDocuments)
					{
						if (!ReadUnreadHttpModule.IsRegistered())
						{
							ReadUnreadHttpModule.RegisterModule(false);
						}
					}
					else
					{
						if (ReadUnreadHttpModule.IsRegistered())
						{
							ReadUnreadHttpModule.UnregisterModule(false);
						}
					}
				
					SqlRemarQ.CleanUpVersionsDb();
					
					SharePointListDictionary allListsInFarm = new SharePointListDictionary();
					SPWebService webSvc = SPFarm.Local.Services.GetValue<SPWebService>(string.Empty);
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
					
					int listCount = allListsInFarm.Count;
					if (listCount > 0)
					{
						this._PercentCompletePerList = Decimal.Divide(90, listCount);

						foreach (Guid siteId in allListsInFarm.SiteIds())
						{
							SPSite listSite = null;
							try
							{
								listSite = new SPSite(siteId);
								this.ProcessAllWebsInSite(listSite, allListsInFarm);
							}
							catch (ArgumentException)
							{
								System.Diagnostics.Trace.Write("ArgError");
							}
							catch (SPException)
							{
								//bad site Id
								System.Diagnostics.Trace.Write("SiteError");
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
					this.UpdateProgress(90);
					this.RemoveBadListConfigurationRecords();
				}
				this.UpdateProgress(100);
			}
			RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.DailyMaintenanceJob.Execute");
		}
	
		/// <summary>
		/// install this job to run at 1 AM daily
		/// </summary>
		public static void Install()
		{
			UnInstall();
			DailyMaintenanceJob spJob = new DailyMaintenanceJob(JOB_NAME);
			//using a custom schedule will cause warnings in MSOCAF but we
			//cant set the job to 1440 minutes so we will do this
			SPSchedule onceADay = SPSchedule.FromString("daily at 01:00");
			spJob.Schedule = onceADay;
			spJob.Update();
		}

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

		/// <summary>
		/// remove this job
		/// </summary>
		public static void UnInstall()
		{
			//first wait on exit
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
	}
}