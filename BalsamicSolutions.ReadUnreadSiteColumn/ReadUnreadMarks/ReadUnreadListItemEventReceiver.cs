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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	//In an ideal world we would do all of this in ItemUpdateing and ItemAdding, however
	//the same events fire for document libraries as for items, and the order of firing
	//and avaiability of property collections is not guaranteed until the ItemAdded events
	//so we will do our updates here
	public class ReadUnreadListItemEventReceiver : SPItemEventReceiver
	{
		/// <summary>
		/// initialize the field value with our pointers
		/// and add the hierarchy item to the read/unread db
		/// </summary>
		/// <param name="properties"></param>
		public override void ItemAdded(SPItemEventProperties properties)
		{
			RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemAdded");
			base.ItemAdded(properties);
			 
			//System.Diagnostics.Trace.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId);
			using (SPMonitoredScope spScope = new SPMonitoredScope("ReadUnreadListItemEventReceiver.ItemAdded " + properties.WebUrl + "/" + properties.List.Title))
			{
				ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(properties.ListId);
				if (null != listConfig)
				{
					try
					{
						//Store the data we need for future operations
						ReadUnreadFieldValue fieldValue = new ReadUnreadFieldValue(properties);
						if (fieldValue.IsValid)
						{
							//every now and then we see this with a false
							//so honor the tls value
							bool eventFireEnabled = this.EventFiringEnabled;
							this.EventFiringEnabled = false;
							properties.ListItem[listConfig.FieldId] = fieldValue.ToString();
							properties.ListItem.SystemUpdate(false);
							this.EventFiringEnabled = eventFireEnabled;
							try
							{ 
								SharePointRemarQ.UpdateItemPath(properties.ListItem);
								//Mark this item as having been read by the author
								if (listConfig.VersionUpdate != ListConfiguration.VersionUpdateType.None)
								{
									SharePointRemarQ.ResetReadMarks(properties.ListItem);
								}
								SharePointRemarQ.MarkRead(properties);
							}
							catch (SqlException sqlError)
							{
								//This can happen if the list is out of sync during deprovisioning
								RemarQLog.LogError("Table is missing in BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemAdded;", sqlError);
							}
						}
					}
					catch (ArgumentException)
					{
						RemarQLog.LogWarning("Field is missing in BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemAdded; ListConfiguration is invalid queuing a deprovision");
						SqlRemarQ.QueueListCommand(listConfig.SiteId, listConfig.WebId, listConfig.ListId, ListCommand.Deprovision, Constants.ReadUnreadLargeQueueTableName);
					}
					catch (SPException sharePointError)
					{
						if (sharePointError.ErrorCode == -2130575305)
						{
							RemarQLog.LogError("Item could not be updated in  BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemAdded queuing a validate", sharePointError);
							SqlRemarQ.QueueListCommand(listConfig.SiteId, listConfig.WebId, listConfig.ListId, ListCommand.Verify, Constants.ReadUnreadLargeQueueTableName);
						}
						else
						{
							throw;
						}
					}
					catch (InvalidOperationException badThing)
					{
						//cant fail the reciever
						RemarQLog.LogError("Item could not be updated in  BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemAdded queuing a re-provisioning", badThing);
						SqlRemarQ.QueueListCommand(listConfig.SiteId, listConfig.WebId, listConfig.ListId, ListCommand.ReInitialize, Constants.ReadUnreadLargeQueueTableName);
					}
				}
			}
		
			RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemAdded");
		}

		/// <param name="properties"></param>
 
		/// <summary>
		/// update the path settings for a moved item
		/// </summary>
		/// <param name="properties"></param>
		public override void ItemFileMoved(SPItemEventProperties properties)
		{
			RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemFileMoved");
			base.ItemFileMoved(properties);
				
			using (SPMonitoredScope spScope = new SPMonitoredScope("ReadUnreadListItemEventReceiver.ItemFileMoved" + properties.WebUrl + "/" + properties.List.Title))
			{
				ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(properties.ListId);
				if (null != listConfig && null != properties.ListItem)
				{
					try
					{
						SharePointRemarQ.UpdateItemPath(properties.ListItem);
					}
					catch (SqlException sqlError)
					{
						//This can happen if the list is out of sync during deprovisioning
						RemarQLog.LogError("Table is missing in BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemFileMoved;", sqlError);
					}
				}
			}
			 
			RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemFileMoved");
		}

		/// <summary>
		/// if someone monkeyed with the item value, put it back correctly
		/// </summary>
		/// <param name="properties"></param>
		public override void ItemUpdated(SPItemEventProperties properties)
		{
			RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemUpdated");
			base.ItemUpdated(properties);
			//if (!SharePointRemarQ.ProvisiongFlagIsSet(properties.List))
			//{
			base.ItemUpdated(properties);
			using (SPMonitoredScope spScope = new SPMonitoredScope("ReadUnreadListItemEventReceiver.ItemUpdated" + properties.WebUrl + "/" + properties.List.Title))
			{
				ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(properties.ListId);
				if (null != listConfig && null != properties.ListItem)
				{
					#if AGGRESSIVEUPDATE
							//Store the data we need for future operations
							ReadUnreadFieldValue fieldValue = new ReadUnreadFieldValue(properties);
							if (fieldValue.IsValid)
							{
								try
								{
									object existingValue = properties.ListItem[listConfig.FieldId];
									if (null != existingValue)
									{
										if (!fieldValue.Equals(existingValue))
										{
											//Someone edited out pointer directly (with Excel or Access or something like that)
											//so lets put it back, the way it should be
											this.EventFiringEnabled = false;
											properties.ListItem[listConfig.FieldId] = fieldValue.ToString();
											try
											{
												properties.ListItem.SystemUpdate(false);
											}
											finally
											{
												this.EventFiringEnabled = true;
											}
										}
									}
								}
								catch (ArgumentException)
								{
									RemarQLog.LogWarning("Field is missing in BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemAdded; ListConfiguration is wrong, Daily Maintenance will fix it");
								}
							}
					#endif			
					try
					{ 
						if (listConfig.VersionUpdate == ListConfiguration.VersionUpdateType.All)
						{
							SharePointRemarQ.ResetReadMarks(properties.ListItem);
						}
						SharePointRemarQ.MarkRead(properties);
					}
					catch (SqlException sqlError)
					{
						//This can happen if the list is out of sync during deprovisioning
						RemarQLog.LogError("Table is missing in BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemUpdated;", sqlError);
					}
				}
			}
			//}
			RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemUpdated");
		}

		/// <summary>
		/// for version check ins
		/// </summary>
		/// <param name="properties"></param>
		public override void ItemCheckedIn(SPItemEventProperties properties)
		{
			RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemCheckedIn");
			base.ItemCheckedIn(properties);
			using (SPMonitoredScope spScope = new SPMonitoredScope("ReadUnreadListItemEventReceiver.ItemCheckedIn" + properties.WebUrl + "/" + properties.List.Title))
			{
				ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(properties.ListId);
				if (null != listConfig && null != properties.ListItem)
				{
					try
					{ 
						if (listConfig.VersionUpdate == ListConfiguration.VersionUpdateType.VersionChange)
						{
							SharePointRemarQ.ResetReadMarks(properties.ListItem);
						}
						SharePointRemarQ.MarkRead(properties);
					}
					catch (SqlException sqlError)
					{
						//This can happen if the list is out of sync during deprovisioning
						RemarQLog.LogError("Table is missing in BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemCheckedIn;", sqlError);
					}
				}
			}
			 
			RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemCheckedIn");
		}

		/// <summary>
		/// remove all references to the item from the read unread db
		/// </summary>
		/// <param name="properties"></param>
		public override void ItemDeleted(SPItemEventProperties properties)
		{
			RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemDeleted");
			base.ItemDeleted(properties);
			using (SPMonitoredScope spScope = new SPMonitoredScope("ReadUnreadListItemEventReceiver.ItemDeleted" + properties.WebUrl + "/" + properties.List.Title))
			{
				base.ItemDeleted(properties);
				try
				{ 
					SharePointRemarQ.RemoveAllItemReferences(properties.ListItemId, properties.ListId);
				}
				catch (SqlException sqlError)
				{
					//This can happen if the list is out of sync during deprovisioning
					RemarQLog.LogError("Table is missing in BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemDeleted;", sqlError);
				}
			}
			 
			RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadListItemEventReceiver.ItemDeleted");
		}
	}
}