// -----------------------------------------------------------------------------
//  Copyright 8/3/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Globalization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace BalsamicSolutions.ReadUnreadSiteColumn.Layouts.BalsamicSolutions.ReadUnreadSiteColumn
{
	public partial class ReadUnreadListUtilities : LayoutsPageBase
	{
		string _UserName = string.Empty;
		string _ListPath = string.Empty;

		protected void Page_Load(object sender, EventArgs e)
		{
			this.ListUtilitiesDescriptionLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditListUtilitiesDescriptionLiteral");
			this.btnCheckList.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditCheckList");
			this.btnResetList.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditInitList");
			this.btnCheckList.Enabled = false;
			this.btnResetList.Enabled = false;
			if (FarmSettings.Settings.IsOk)
			{
				string listIdQs = this.Request.QueryString["listId"];
				if (!listIdQs.IsNullOrWhiteSpace())
				{
					Guid listId = Guid.Empty;
					if (Guid.TryParse(listIdQs, out listId))
					{
						ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(listId);
						this.btnCheckList.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, listConfig.Locale, "PropEditCheckList");
						this.btnResetList.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, listConfig.Locale, "PropEditInitList");
						if (null != listConfig)
						{
							using (SPSite rootSite = new SPSite(listConfig.SiteId))
							{
								using (SPWeb rootWeb = rootSite.OpenWeb(listConfig.WebId))
								{
									bool hasPermission = false;
									if (null != rootWeb.CurrentUser)
									{
										this._UserName = rootWeb.CurrentUser.Name;
										SPList thisList = rootWeb.Lists[listConfig.ListId];
										this._ListPath = thisList.Title + "::" + rootWeb.Title + "::" + rootSite.Url;

										SPBasePermissions permMask = thisList.EffectiveBasePermissions;
										hasPermission = (permMask & SPBasePermissions.FullMask) != 0;
									}

									if (hasPermission)
									{
										this.btnCheckList.Enabled = true;
										this.btnResetList.Enabled = true;
										this.btnResetList.Click += this.BtnResetList_Click;
										this.btnCheckList.Click += this.BtnCheckList_Click;
									}
									else
									{
										this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, listConfig.Locale, "PropEditNoPermission");
									}
								}
							}
						}
						else
						{
							this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditListNotInitialized");
						}
					}
				}
			}
		}

		void BtnResetList_Click(object sender, EventArgs e)
		{
			this.btnCheckList.Enabled = false;
			this.btnResetList.Enabled = false;
			string listIdQs = this.Request.QueryString["listId"];
			if (!listIdQs.IsNullOrWhiteSpace())
			{
				Guid listId = Guid.Empty;
				if (Guid.TryParse(listIdQs, out listId))
				{
					ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(listId);
					if (null != listConfig)
					{
						//all manual repairs queue to the large queue processor
						string logMessage = string.Format("A re-initialize request was submitted for List {0}  ({1})", listConfig.ListId, this._ListPath);
						logMessage += " by " + this._UserName;
						
						RemarQLog.LogMessage(logMessage);
						SqlRemarQ.QueueListCommand(listConfig.SiteId, listConfig.WebId, listConfig.ListId, ListCommand.ReInitialize, Constants.ReadUnreadLargeQueueTableName);
						this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,listConfig.Locale, "PropEditListMaintStarted");
					}
				}
			}
			this.Page.Response.Clear();
			this.Page.Response.Write(Constants.CloseSharePointDialogScript);
			this.Page.Response.End();
		}

		void BtnCheckList_Click(object sender, EventArgs e)
		{
			this.btnCheckList.Enabled = false;
			this.btnResetList.Enabled = false;
			string listIdQs = this.Request.QueryString["listId"];
			if (!listIdQs.IsNullOrWhiteSpace())
			{
				Guid listId = Guid.Empty;
				if (Guid.TryParse(listIdQs, out listId))
				{
					ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(listId);
					if (null != listConfig)
					{
						//all manual repairs queue to the large queue processor
						string logMessage = string.Format("A verify request was submitted for List {0}  ({1})", listConfig.ListId, this._ListPath);
						logMessage += " by " + this._UserName;
						RemarQLog.LogMessage(logMessage);
						SqlRemarQ.QueueListCommand(listConfig.SiteId, listConfig.WebId, listConfig.ListId, ListCommand.Verify, Constants.ReadUnreadLargeQueueTableName);
						this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,listConfig.Locale, "PropEditListMaintStarted");
					}
				}
			}
			this.Page.Response.Clear();
			this.Page.Response.Write(Constants.CloseSharePointDialogScript);
			this.Page.Response.End();
		}
	}
}