// -----------------------------------------------------------------------------
//  Copyright 9/4/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Office.Server.Utilities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// handles processing of a list initialization 
	/// command from one of the queues
	/// </summary>
	internal class ListInitializationProcessor
	{
		readonly SPJobDefinition _HostSPJob = null;
		readonly string _TableName = string.Empty;
		readonly string _JobName = string.Empty;
		int _ThisBatchListCount = 0;
		int _LastReportingPercent = 0;
		Decimal _PercentComplete = 0;
		readonly Dictionary<Guid, decimal> _PercentCompleteAmount = new Dictionary<Guid, decimal>();
		readonly Dictionary<Guid, decimal> _PercentCompleteAmountPerItem = new Dictionary<Guid, decimal>();
		readonly Dictionary<Guid, decimal> _ListPercentComplete = new Dictionary<Guid, decimal>();
		bool _Canceled = false;
		Dictionary<Guid, int> _JobMap = new Dictionary<Guid, int>();
		readonly object _LockProxy = new object();
		readonly object _PauseProxy = new object();
		int _JobCount = 0;
		ContentIterator _ContentIterator = null;
		bool _Paused = false;

		internal ListInitializationProcessor(string tableName, string jobName, SPJobDefinition hostJob)
			: this(tableName,jobName)
		{
			this._HostSPJob = hostJob;
		}

		internal ListInitializationProcessor(string tableName, string jobName)
		{
			this._TableName = tableName;
			this._JobName = jobName;
			this._HostSPJob = null;
		}

		internal bool Canceled
		{
			get
			{
				lock (this._LockProxy)
				{
					return this._Canceled;
				}
			}
			set
			{
				lock (this._LockProxy)
				{
					this._Canceled = value;
					 
					if (null != this._ContentIterator)
					{
						this._ContentIterator.Cancel = value;
					}
				}
			}
		}

		internal bool Paused
		{
			get
			{
				lock (this._LockProxy)
				{
					return this._Paused;
				}
			}
			set
			{
				lock (this._LockProxy)
				{
					this._Paused = value;
				}
				Monitor.PulseAll(this._PauseProxy);
			}
		}

		void PauseWait()
		{
			while (this.Paused)
			{
				lock (this._PauseProxy)
				{
					if (!Monitor.Wait(this._PauseProxy, 5000))
					{
						throw new InvalidProgramException("Could not wait on Pause");
					}
				}
			}
		}

		internal void Execute()
		{
			this.UpdateProgressInternal(0);
			FarmSettings farmSettings = FarmSettings.Settings;
			if (null != farmSettings && farmSettings.IsOk && farmSettings.Activated && FarmLicense.License.IsLicensed())
			{
				Framework.ResourceManager.ResetCache();
				this._LastReportingPercent = 0;
				this._PercentComplete = 0;
				List<Guid> provisionList = null;
				List<Guid> verifyList = null; 
				List<Guid> deprovisionList = null; 
				List<Guid>reinitializeList = null;
				//Only one of these jobs runs each time, but we need
				//to know which ones to delete at the end of our process
				//so we will create a batch
				string batchId = SqlRemarQ.CreateBatch(this._TableName, out provisionList, out verifyList, out deprovisionList, out reinitializeList, out this._JobMap);
				this._ThisBatchListCount = provisionList.Count + verifyList.Count + deprovisionList.Count + reinitializeList.Count;
				if (!batchId.IsNullOrWhiteSpace() && this._ThisBatchListCount > 0)
				{
					//provision list will update progress item by item
					foreach (Guid listId in provisionList)
					{
						this._ListPercentComplete.Add(listId, 0);
					}
					foreach (Guid listId in reinitializeList)
					{
						this._ListPercentComplete.Add(listId, 0);
					}
					decimal percentCompletePerList = Decimal.Divide(90, this._ThisBatchListCount);
		 
					if (!this.Canceled)
					{
						this.ProvisionLists(provisionList, batchId, true);
					}
					if (!this.Canceled)
					{
						this.ProvisionLists(reinitializeList, batchId, false);
					}
					//no update the global value for percent complete
					this._PercentComplete = percentCompletePerList * provisionList.Count;
					this._LastReportingPercent = (int)Math.Round(this._PercentComplete, 0);
					this.UpdateProgressInternal(this._LastReportingPercent);

					//per list count for these, not per item
					if (!this.Canceled)
					{
						this.VerifyLists(verifyList);
					}
					this._PercentComplete += percentCompletePerList * verifyList.Count;
					this._LastReportingPercent = (int)Math.Round(this._PercentComplete, 0);
					this.UpdateProgressInternal(this._LastReportingPercent);

					//per list count for these, not per item
					if (!this.Canceled)
					{
						this.Deprovision(deprovisionList);
					}
					this._PercentComplete += percentCompletePerList * deprovisionList.Count;
					this._LastReportingPercent = (int)Math.Round(this._PercentComplete, 0);
					this.UpdateProgressInternal(this._LastReportingPercent);
					if (!this.Canceled)
					{
						SqlRemarQ.DeleteBatch(this._TableName, batchId);
					}
				}
			}
			this.UpdateProgressInternal(100);
		}

		void UpdateProgressInternal(int updateValue)
		{
			if (null != this._HostSPJob)
			{
				this._HostSPJob.UpdateProgress(updateValue);
			}
		}

		/// <summary>
		/// verify the remarq settings for a list of list Ids
		/// </summary>
		/// <param name="validateList"></param>
		void VerifyLists(List<Guid> validateList)
		{
			foreach (Guid listId in validateList)
			{
				this.PauseWait();
				if (!this.Canceled)
				{
					//Parallel.ForEach(validateList, listId =>
					//{
					try
					{
						ListConfiguration listConfig = SqlRemarQ.GetListConfiguration(listId);
						if (null != listConfig)
						{
							this.VerifyRemarQConfigurationSettings(listConfig);
						}
					}
					catch (SPException sharePointError)
					{
						RemarQLog.LogError("Error updating list " + listId.ToString("B"), sharePointError);
					}
					//}); 
				}
			}
		}

		/// <summary>
		/// deprovision remarq settings for a lsit of list Ids
		/// </summary>
		/// <param name="deProvisionList"></param>
		void Deprovision(List<Guid> deProvisionList)
		{
			foreach (Guid listId in deProvisionList)
			{
				this.PauseWait();
				if (!this.Canceled)
				{
					//Parallel.ForEach(deProvisionList, listId =>
					//{
					try
					{
						ListConfiguration listConfig = SqlRemarQ.GetListConfiguration(listId);
						if (null != listConfig)
						{
							this.DeleteRemarQConfigurationSettings(listConfig);
						}
					}
					catch (SPException sharePointError)
					{
						RemarQLog.LogError("Error deleting list " + listId.ToString("B"), sharePointError);
						SqlRemarQ.DeProvisionReadUnreadTableAndRemoveConfigurationEntry(listId);
					}
					//}); 
				}
			}
		}
		
		/// <summary>
		/// provision a list of listIds
		/// </summary>
		/// <param name="provisionList"></param>
		void ProvisionLists(List<Guid> provisionList, string batchId, bool removeAllItemReferences)
		{
			foreach (Guid listId in provisionList)
			{
				//Parallel.ForEach(provisionList, listId =>
				//{
				try
				{
					this.PauseWait();
					if (!this.Canceled)
					{
						ListConfiguration listConfig = SqlRemarQ.GetListConfiguration(listId);
						if (null != listConfig)
						{
							if (removeAllItemReferences)
							{
								SqlRemarQ.RemoveAllItemReferences(listConfig.ListId);
							}
							this.InitializeRemarQSettings(listConfig);
							this.UpdateAllExistingItemReadUnreadValuesOneByOne(listConfig.ListId, listConfig.WebId, listConfig.SiteId, batchId);
						}
					}
				}
				catch (SPException sharePointError)
				{
					RemarQLog.LogError("Error initializing list " + listId.ToString("B"), sharePointError);
					SqlRemarQ.DeProvisionReadUnreadTableAndRemoveConfigurationEntry(listId);
				}
				//}); 
			}
		}

		/// <summary>
		/// delete all the remaq seettings for a list
		///and deprovision the configuration for it
		/// </summary>
		/// <param name="listConfig"></param>
		void DeleteRemarQConfigurationSettings(ListConfiguration listConfig)
		{
			if (this._JobMap.ContainsKey(listConfig.ListId))
			{
				SqlRemarQ.TickleQueueRecord(this._JobMap[listConfig.ListId], 5, this._TableName);
			}
			try
			{
				using (SPSite spSite = new SPSite(listConfig.SiteId))
				{
					using (SPWeb spWeb = spSite.OpenWeb(listConfig.WebId))
					{
						SPList parentList = spWeb.Lists[listConfig.ListId];
						//The field is already gone so we need to simply delete the handlers
						ReadUnreadField.DeProvisionSpecialViews(parentList);
						ReadUnreadField.UninstallAllEventHandlersInternal(parentList);
						ReadUnreadField.UninstallContextMenuInternal(parentList);
					}
				}
			}
			catch (ArgumentException badListOrWebError)
			{
				RemarQLog.LogError("Error deleting list configuration web/list is unavailable " + listConfig.ListId.ToString("B"), badListOrWebError);
			}
			catch (FileNotFoundException badSiteError)
			{
				RemarQLog.LogError("Error deleting  list configuration site is unavailable " + listConfig.ListId.ToString("B"), badSiteError);
			}
			catch (SPException sharePointError)
			{
				RemarQLog.LogError("Error deleting  list configuration " + listConfig.ListId.ToString("B"), sharePointError);
			}
			SqlRemarQ.DeProvisionReadUnreadTableAndRemoveConfigurationEntry(listConfig.ListId);
			if (this._JobMap.ContainsKey(listConfig.ListId))
			{
				SqlRemarQ.TickleQueueRecord(this._JobMap[listConfig.ListId], 100, this._TableName);
			}
		}

		/// <summary>
		/// verify all the remarq settings for a list updating as necessary
		/// </summary>
		/// <param name="listConfig"></param>
		void VerifyRemarQConfigurationSettings(ListConfiguration listConfig)
		{
			if (this._JobMap.ContainsKey(listConfig.ListId))
			{
				SqlRemarQ.TickleQueueRecord(this._JobMap[listConfig.ListId], 5, this._TableName);
			}
			try
			{
				using (SPSite spSite = new SPSite(listConfig.SiteId))
				{
					using (SPWeb spWeb = spSite.OpenWeb(listConfig.WebId))
					{
						if (spWeb.EnableMinimalDownload)
						{
							spWeb.EnableMinimalDownload = false;
							spWeb.Update();
						}
						SPList parentList = spWeb.Lists[listConfig.ListId];
						if (parentList.Fields.Contains(listConfig.FieldId))
						{
							if (!ReadUnreadField.AreAllEventHandlersInstalled(parentList))
							{
								ReadUnreadField.UninstallAllEventHandlersInternal(parentList);
								ReadUnreadField.InstallEventHandlersInternal(parentList);
							}
							//always reset the context menus so we can update languages files
							ReadUnreadField.UninstallContextMenuInternal(parentList);
							if (listConfig.ContextMenu != ListConfiguration.ContextMenuType.None)
							{
								if (!ReadUnreadField.AreAllContextMenusInstalled(parentList, listConfig.ContextMenu))
								{
									ReadUnreadField.UninstallContextMenuInternal(parentList);
									ReadUnreadField.InstallContextMenuInternal(listConfig, parentList);
								}
							}
							ReadUnreadField.VerifyViews(parentList);
						}
					}
				}
			}
			catch (ArgumentException badListOrWebError)
			{
				RemarQLog.LogError("Error verifying list configuration web/list is unavailable " + listConfig.ListId.ToString("B"), badListOrWebError);
			}
			catch (FileNotFoundException badSiteError)
			{
				RemarQLog.LogError("Error verifying  list configuration site is unavailable " + listConfig.ListId.ToString("B"), badSiteError);
			}
			catch (SPException sharePointError)
			{
				RemarQLog.LogError("Error verifying  list configuration " + listConfig.ListId.ToString("B"), sharePointError);
			}
			if (this._JobMap.ContainsKey(listConfig.ListId))
			{
				SqlRemarQ.TickleQueueRecord(this._JobMap[listConfig.ListId], 100, this._TableName);
			}
		}

		/// <summary>
		/// initialize the remarq settings for a list
		/// </summary>
		/// <param name="listConfig"></param>
		void InitializeRemarQSettings(ListConfiguration listConfig)
		{
			try
			{
				using (SPSite spSite = new SPSite(listConfig.SiteId))
				{
					using (SPWeb spWeb = spSite.OpenWeb(listConfig.WebId))
					{
						if (spWeb.EnableMinimalDownload)
						{
							spWeb.EnableMinimalDownload = false;
							spWeb.Update();
						}
						SPList parentList = spWeb.Lists[listConfig.ListId];
						if (parentList.Fields.Contains(listConfig.FieldId))
						{
							ReadUnreadField.UninstallContextMenuInternal(parentList);
							ReadUnreadField.UninstallAllEventHandlersInternal(parentList);
							ReadUnreadField.InstallEventHandlersInternal(parentList);
							ReadUnreadField.InstallContextMenuInternal(listConfig, parentList);
							ReadUnreadField.VerifyViews(parentList);
						}
					}
				}
			}
			catch (ArgumentException badListOrWebError)
			{
				RemarQLog.LogError("Error initializing list configuration web/list is unavailable " + listConfig.ListId.ToString("B"), badListOrWebError);
			}
			catch (FileNotFoundException badSiteError)
			{
				RemarQLog.LogError("Error initializing  list configuration site is unavailable " + listConfig.ListId.ToString("B"), badSiteError);
			}
			catch (SPException sharePointError)
			{
				RemarQLog.LogError("Error initializing  list configuration " + listConfig.ListId.ToString("B"), sharePointError);
			}
		}

		/// <summary>
		/// use the ContentIterator on this list because it might be too big to 
		/// </summary>
		void UpdateAllExistingItemReadUnreadValuesOneByOne(Guid listId, Guid webId, Guid siteId, string batchId)
		{
			if (this._JobMap.ContainsKey(listId))
			{
				SqlRemarQ.TickleQueueRecord(this._JobMap[listId], 1, this._TableName);
			}
			try
			{
				using (SPSite spSite = new SPSite(siteId))
				{
					using (SPWeb spWeb = spSite.OpenWeb(webId))
					{
						SPList updateMe = null;
						try
						{
							updateMe = spWeb.Lists[listId];
						}
						catch (ArgumentException)
						{
							updateMe = null;
						}
						if (null != updateMe)
						{
							ReadUnreadField ruField = Utilities.FindFirstFieldOnList(updateMe);

							if (null != ruField && (updateMe.ItemCount > 0))
							{
								SharePointRemarQ.SetProvisioningFlag(siteId, webId, listId);
								try
								{
									using (SPMonitoredScope spScope = new SPMonitoredScope(this._JobName + " " + spWeb.Url + "/" + updateMe.Title))
									{
										lock (this._LockProxy)
										{
											decimal percentCompletePerItemThisList = Decimal.Divide(100, updateMe.ItemCount);
											this._PercentCompleteAmountPerItem[listId] = percentCompletePerItemThisList;
											if (this._ThisBatchListCount > 1)
											{
												percentCompletePerItemThisList = percentCompletePerItemThisList / (decimal)this._ThisBatchListCount;
											}
											this._PercentCompleteAmount[listId] = percentCompletePerItemThisList;
										}
										try
										{
											this._ContentIterator = new ContentIterator();
											this._ContentIterator.ProcessItemsInFolder(updateMe, updateMe.RootFolder, true, true, this.UpdateOneItemsReadUnReadValue, this.HandleOneItemsError);
										}
										finally
										{
											this._ContentIterator = null;
										}
									}
								}
								finally
								{
									SharePointRemarQ.ClearProvisioningFlag(siteId, webId, listId);
								}
							}
							if (null != spWeb.CurrentUser)
							{
								SqlRemarQ.MarkUnRead(listId, spWeb.CurrentUser);
							}
						}
					}
				}
			}
			catch (ArgumentException badListOrWebError)
			{
				RemarQLog.LogError("Error updating list items web/list is unavailable " + listId.ToString("B"), badListOrWebError);
			}
			catch (FileNotFoundException badSiteError)
			{
				RemarQLog.LogError("Error updating list items site is unavailable " + listId.ToString("B"), badSiteError);
			}
			catch (SPException sharePointError)
			{
				RemarQLog.LogError("Error updating list items " + listId.ToString("B"), sharePointError);
			}
			if (this._JobMap.ContainsKey(listId))
			{
				SqlRemarQ.TickleQueueRecord(this._JobMap[listId], 100, this._TableName);
			}
		}

		public int JobCount
		{
			get { return this._JobCount; }
		}

		/// <summary>
		/// callback from the content ContentIterator
		/// </summary>
		/// <param name="listItem"></param>
		void UpdateOneItemsReadUnReadValue(SPListItem listItem)
		{
			this.PauseWait();
			if (!this.Canceled && null != listItem && null != listItem.ParentList)
			{
				int itemId = listItem.ID;
				Guid listId = listItem.ParentList.ID;
				try
				{
					decimal perJobPercentCompletePerListItem = 0;
					decimal perListPercentCompletePerListItem = 0;

					lock (this._LockProxy)
					{
						this._PercentCompleteAmount.TryGetValue(listItem.ParentList.ID, out perJobPercentCompletePerListItem);
						this._PercentCompleteAmountPerItem.TryGetValue(listItem.ParentList.ID, out perListPercentCompletePerListItem);
					}

					ReadUnreadField ruField = Utilities.FindFirstFieldOnList(listItem.ParentList);
					if (null != ruField)
					{
						ReadUnreadFieldValue fieldValue = new ReadUnreadFieldValue(listItem);
						SqlRemarQ.UpdateItemPath(listItem);
						object existingValue = listItem[ruField.InternalName];
						string textValue = null == existingValue ? null : existingValue.ToString();
						if (null == textValue || !textValue.Equals(fieldValue.ToString()))
						{
							listItem[ruField.InternalName] = fieldValue.ToString();

							try
							{
								listItem.SystemUpdate(false);
							}
							catch (SPException)
							{
								SPListItem oneMoreTime = listItem.ParentList.GetItemById(listItem.ID);
								oneMoreTime[ruField.InternalName] = fieldValue.ToString();
								oneMoreTime.SystemUpdate(false);
							}
							// how to wait on the list item event handler which may fire ?
							//SharePointRemarQ.MarkUnRead(listItem,jobUser);
							SqlRemarQ.ResetReadMarks(new int[] { listItem.ID }, listId);
						}
						this._JobCount++;
												
						lock (this._LockProxy)
						{
							this._ListPercentComplete[listId] += perListPercentCompletePerListItem;
							this._PercentComplete += perJobPercentCompletePerListItem;
							int reportingPercentage = (int)Math.Round(this._PercentComplete, 0);
							if (reportingPercentage > this._LastReportingPercent)
							{
								this._LastReportingPercent = reportingPercentage;
								this.UpdateProgressInternal(reportingPercentage);
								//get the local list percentage
								reportingPercentage = (int)Math.Round(this._ListPercentComplete[listId], 0);
								if (this._JobMap.ContainsKey(listId))
								{
									SqlRemarQ.TickleQueueRecord(this._JobMap[listId], reportingPercentage, this._TableName);
								}
							}
						}
					}
				}
				catch (SPException sharePointError)
				{
					RemarQLog.LogError("Error initializing  list listitem (" + itemId.ToString() + ") of " + listId.ToString("B"), sharePointError);
				}
			}
		}
        
		/// <summary>
		/// handle error for one item from the content iterator
		/// </summary>
		/// <param name="listItem"></param>
		/// <param name="itemError"></param>
		/// <returns></returns>
		bool HandleOneItemsError(SPListItem listItem, Exception itemError)
		{
			string debugText = "Error during list initialization";
			if (null != listItem && null != listItem.ParentList && null != listItem.ParentList.ParentWeb)
			{
				debugText += listItem.ParentList.ParentWeb.ServerRelativeUrl + "/" + listItem.ParentList.Title + "/" + listItem.Title;
			}
			RemarQLog.LogError(debugText, itemError);
			return false;
		}
	}
}