// -----------------------------------------------------------------------------
//  Copyright 5/30/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace BalsamicSolutions.ReadUnreadSiteColumn.Layouts.BalsamicSolutions.ReadUnreadSiteColumn
{
	public partial class ReadUnreadReportQuery : LayoutsPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			//if(!System.Diagnostics.Debugger.IsAttached)
			//{
			//	System.Diagnostics.Debugger.Break();
			//}
			string jsonResult = "[]";
			if (FarmLicense.License.LicenseMode >= LicenseModeType.Enterprise)
			{
				string itemIdsQS = this.Request.QueryString["itemId"];
				string listIdQS = this.Request.QueryString["listId"];
				string takeQS = this.Request["take"];
				string skipQS = this.Request["skip"];
				string pageQS = this.Request["page"];
				string pageSizeQS = this.Request["pageSize"];
				string hIdQs = this.Request["hId"];
				int hId =0;
				int takeSize = 10;
				int skipSize = 0;
				int pageSize = 10;
				int pageNum = 1;
				string sortField = "ReadOn";
				string sortDirection = "desc";
				string sortFieldQs = this.Request.Form["filter[filters][0][sort]"];
				string sortDirectionQs = this.Request.Form["filter[filters][0][dir]"];
				//we only support a CONTAINS against the ReadBy field so lets collect that 
				//and ignore the other filter commands properties
				string containsValue =  this.Request.Form["filter[filters][0][value]"];
				if (!string.IsNullOrEmpty(sortFieldQs))
				{
					sortField = sortFieldQs.ToUpperInvariant();
				}
				if (!string.IsNullOrEmpty(sortDirectionQs))
				{
					sortDirection = sortDirectionQs.ToUpperInvariant();
				}
				if (!string.IsNullOrEmpty(hIdQs))
				{
					int.TryParse(hIdQs, out hId);
				}
				if (!string.IsNullOrEmpty(takeQS))
				{
					int.TryParse(takeQS, out takeSize);
				}
				if (!string.IsNullOrEmpty(skipQS))
				{
					int.TryParse(skipQS, out skipSize);
				}
				if (!string.IsNullOrEmpty(pageQS))
				{
					int.TryParse(pageQS, out pageNum);
				}
				if (!string.IsNullOrEmpty(pageSizeQS))
				{
					int.TryParse(pageSizeQS, out pageSize);
				}

				int itemId = 0;
				Guid listId = Guid.Empty;
				if (int.TryParse(itemIdsQS, out itemId) && Guid.TryParse(listIdQS, out listId))
				{
					ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(listId);
					if (null != listConfig)
					{
						using (SPSite spSite = new SPSite(listConfig.SiteId))
						{
							using (SPWeb spWeb = spSite.OpenWeb(listConfig.WebId))
							{
								SPList spList = spWeb.Lists[listConfig.ListId];
								SPListItem spItem = spList.GetItemById(itemId);
								if (SharePointRemarQ.HasPermissionToRunReport(spWeb.CurrentUser, spItem, spList, spWeb))
								{
									RemarQuery ruQuery = new RemarQuery(spItem);
									List<ReportData> versionIds = null;
									List<ReportData> readHistory = ruQuery.AllReaders(hId,out versionIds);
									int totalCount = readHistory.Count;
									//we have to update the display names before we apply the contains match
 									foreach (ReportData reportItem in readHistory)
									{
										  this.UpdateDisplayNames(reportItem,reportItem.UserId, spWeb);
									}
									if(!string.IsNullOrWhiteSpace(containsValue))
									{
										List<ReportData> subSet = new List<ReportData>();
										foreach (ReportData checkMe in readHistory)
										{
											if (null != checkMe && null != checkMe.ReadBy)
											{
												if (checkMe.ReadBy.CaseInsensitiveContains(containsValue))
												{
													subSet.Add(checkMe);
												}
											}
										}
										readHistory = subSet;
										totalCount = readHistory.Count;
									}
									readHistory = this.SortAndpageList(readHistory, takeSize, skipSize, pageSize,  sortField, sortDirection);
									
									JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
									AjaxResult returnValue = new AjaxResult(readHistory,totalCount);
									jsonResult = jsonSerializer.Serialize(returnValue);
								}
							}
						}
					}
				}
			}
			
			this.Response.Clear();
			this.Response.BufferOutput = false;
			this.Response.ContentType = "application/json;";
			this.Response.Write(jsonResult);
			this.Context.ApplicationInstance.CompleteRequest();
		}

		List<ReportData> SortAndpageList(List<ReportData> rowData, int takeSize, int skipSize, int pageSize,   string sortField, string sortDirection)
		{
			 
			List<ReportData> sortedList = new List<ReportData>();
			bool sortDesc = sortDirection.ToUpperInvariant() == "DESC";
			switch(sortField.ToUpperInvariant())
			{
 
				case "READON":
					if (sortDesc)
					{
						sortedList = rowData.OrderByDescending(o => o.ReadOn).ToList();
					}
					else
					{
						sortedList = rowData.OrderBy(o => o.ReadOn).ToList();
					}
					 
					break;
				case "READBY":
					if (sortDesc)
					{
						sortedList = rowData.OrderByDescending(o => o.ReadBy).ToList();
					}
					else
					{
						sortedList = rowData.OrderBy(o => o.ReadBy).ToList();
					}
					 
					break;
			}

			List<ReportData> returnValue = new List<ReportData>();
			if (skipSize < sortedList.Count)
			{
				int idxNum = skipSize;
				int maxNum = skipSize + takeSize;
				if (maxNum > sortedList.Count)
				{
					maxNum = sortedList.Count;
				}
				while (returnValue.Count < pageSize && idxNum < maxNum)
				{
					returnValue.Add(sortedList[idxNum]);
					idxNum ++;	
				}
			}
			return returnValue;
		}
	
		void UpdateDisplayNames(ReportData reportItem,int userId, SPWeb spWeb)
		{
			string displayName = userId.ToString(CultureInfo.InvariantCulture);
			string emailName = string.Empty;
			try
			{
				SPUser webUser = spWeb.AllUsers.GetByID(userId);
				if (null == webUser)
				{
					displayName = userId.ToString() + "->" + Framework.ResourceManager.GetString(spWeb.Locale, "UNKNOWN");
				}
				else
				{
					displayName = webUser.Name;
					if(!string.IsNullOrEmpty(webUser.Email))
					{
						emailName = webUser.Email;
					}
				}
			}
			catch (SPException)
			{
				displayName = userId.ToString() + "->" + Framework.ResourceManager.GetString(spWeb.Locale, "UNKNOWN");
			}
			catch (System.ArgumentOutOfRangeException)
			{
				displayName = userId.ToString() + "->" + Framework.ResourceManager.GetString(spWeb.Locale, "INVALID");
			}
			catch (IndexOutOfRangeException)
			{
				displayName = userId.ToString() + "->" + Framework.ResourceManager.GetString(spWeb.Locale, "INVALID");
			}
 
			reportItem.ReadBy= displayName;
			reportItem.ReadByEmail=emailName;
		}
	}
}