// -----------------------------------------------------------------------------
//  Copyright 8/3/2014 (c) Balsamic Solutions, Inc. All rights reserved.
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
using System.Web;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.ApplicationPages;
using Microsoft.SharePoint.ApplicationPages.WebControls;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace BalsamicSolutions.ReadUnreadSiteColumn.Layouts.BalsamicSolutions.ReadUnreadSiteColumn
{
	public partial class ReadUnreadAdvancedSettings : LayoutsPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			this.txtReadImageUrl.Enabled = false;
			this.txtUnreadHTMLColor.Enabled = false;
			this.txtUnreadImageUrl.Enabled = false;
			this.chkShowInlineEditingTools.Enabled = false;
			this.chkShowReportingMenu.Enabled = false;
			this.txtRefreshInterval.Enabled = false;
			this.btnApply.Enabled = false;
			this.chkVersionFlags.Enabled = false;
			this.RefreshIntervalLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditRefreshIntervalDescription");
			this.ColumnDisplayColorTextLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditColumnDisplayColorText");
			this.ChkShowInlineEditingToolsTextLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditChkShowInlineEditingToolsText");
			this.chkShowReportingMenuLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditChkShowReportingMenuText");
			this.UnreadImageUrlLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditUnreadImageUrl");
			this.ReadImageUrlLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditReadImageUrl");
			this.PropEditInstructionsLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditAdvancedEditReadUnreadPropertySectionDescription");
			this.chkVersionFlagsLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditVersionFlags");

			FarmLicense currentLicense = FarmLicense.License;
			FarmSettings.ExpireCachedObject();
			if (FarmSettings.Settings.IsOk)
			{
				if (currentLicense.LicenseMode >= LicenseModeType.Professional)
				{
					string listIdQs = this.Request.QueryString["listId"];
					bool isDiscussionBoard = false;
					string isDicussionBoardQs = this.Request.QueryString["isDb"];
					if (!string.IsNullOrEmpty(isDicussionBoardQs))
					{
						if (!bool.TryParse(isDicussionBoardQs, out isDiscussionBoard))
						{
							isDiscussionBoard = false;
						}
					}
					 
					if (!listIdQs.IsNullOrWhiteSpace())
					{
						Guid listId = Guid.Empty;
						if (Guid.TryParse(listIdQs, out listId))
						{
							try
							{
								ListConfiguration listConfig = new ListConfiguration(listId);
								this.kendoStuff.Text = FarmSettings.Settings.KendoHeader(listConfig.Locale);
								this.RefreshIntervalLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, listConfig.Locale, "PropEditRefreshIntervalDescription");
								this.ColumnDisplayColorTextLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, listConfig.Locale, "PropEditColumnDisplayColorText");
								this.ChkShowInlineEditingToolsTextLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, listConfig.Locale, "PropEditChkShowInlineEditingToolsText");
								this.chkShowReportingMenuLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, listConfig.Locale, "PropEditChkShowReportingMenuText");
								this.UnreadImageUrlLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, listConfig.Locale, "PropEditUnreadImageUrl");
								this.ReadImageUrlLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, listConfig.Locale, "PropEditReadImageUrl");
								this.PropEditInstructionsLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, listConfig.Locale, "PropEditAdvancedEditReadUnreadPropertySectionDescription");
								this.chkVersionFlagsLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, listConfig.Locale, "PropEditVersionFlags");

								if (!this.Page.IsPostBack)
								{
									this.txtRefreshInterval.Text = listConfig.RefreshInterval.ToString(CultureInfo.InvariantCulture);
									this.txtReadImageUrl.Text = listConfig.ReadImagePath;
									this.txtUnreadImageUrl.Text = listConfig.UnreadImagePath;
									this.txtUnreadHTMLColor.Text = listConfig.UnreadHtmlColor;
									this.chkShowInlineEditingTools.Checked = (listConfig.ContextMenu == ListConfiguration.ContextMenuType.All || listConfig.ContextMenu == ListConfiguration.ContextMenuType.ReadTool);
									this.chkShowReportingMenu.Checked = false;
									if (currentLicense.LicenseMode >= LicenseModeType.Enterprise)
									{
										this.chkVersionFlags.Enabled = true;
										this.chkVersionFlags.Checked = (listConfig.VersionUpdate == ListConfiguration.VersionUpdateType.VersionChange);
										this.chkShowReportingMenu.Enabled = true;
										this.chkShowReportingMenu.Checked = (listConfig.ContextMenu == ListConfiguration.ContextMenuType.All || listConfig.ContextMenu == ListConfiguration.ContextMenuType.ReportTool);
									}
								}
								this.txtRefreshInterval.Enabled = true;
								this.txtReadImageUrl.Enabled = !isDiscussionBoard;
								this.txtUnreadHTMLColor.Enabled = true;
								this.txtUnreadImageUrl.Enabled = !isDiscussionBoard;
								this.chkShowInlineEditingTools.Enabled = true;
								this.btnApply.Enabled = true;
								this.btnApply.Click += this.Apply_Click;
							}
							catch (ArgumentOutOfRangeException)
							{
								this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditListNotInitialized");
							}
						}
					}
				}
				else
				{
					this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditNoPermission");
				}
			}
		}

		void Apply_Click(object sender, EventArgs e)
		{
			string listIdQs = this.Request.QueryString["listId"];
			if (!listIdQs.IsNullOrWhiteSpace())
			{
				Guid listId = Guid.Empty;
				if (Guid.TryParse(listIdQs, out listId))
				{
					ListConfiguration listConfig = new ListConfiguration(listId);
					try
					{
						int refreshInterval = 0;
						if (!int.TryParse(this.txtRefreshInterval.Text, out refreshInterval))
						{
							refreshInterval = 0;
						}
						ListConfiguration.VersionUpdateType updateFlags = ListConfiguration.VersionUpdateType.All;
						if (this.chkVersionFlags.Checked)
						{
							updateFlags = ListConfiguration.VersionUpdateType.VersionChange;
						}
 
						ListConfiguration.ContextMenuType menuType = ListConfiguration.ContextMenuType.None;
						if (this.chkShowInlineEditingTools.Checked)
						{
							menuType = ListConfiguration.ContextMenuType.ReadTool;
						}
						if (this.chkShowReportingMenu.Checked)
						{
							menuType = ListConfiguration.ContextMenuType.ReportTool;
						}
						if (this.chkShowReportingMenu.Checked && this.chkShowInlineEditingTools.Checked)
						{
							menuType = ListConfiguration.ContextMenuType.All;
						}
						string unreadColor = this.txtUnreadHTMLColor.Text;
						if (string.IsNullOrWhiteSpace(unreadColor))
						{
							unreadColor = Constants.DefaultUnreadColor;
						}
						if (!unreadColor.StartsWith("#"))
						{
							unreadColor = "#" + unreadColor;
						}
						//CultureInfo ci =CultureInfo.CreateSpecificCulture(listConfig.CultureName);
						SqlRemarQ.UpdateListConfiguration(listConfig.ListId,
							listConfig.WebId,
							listConfig.SiteId,
							listConfig.FieldId,
							listConfig.ColumnRenderMode,
							this.txtReadImageUrl.Text,
							this.txtUnreadImageUrl.Text,
							unreadColor,
							unreadColor,
							menuType,
							updateFlags,
							listConfig.PublicName,
							(uint)listConfig.Locale.LCID,
							listConfig.LayoutsPath,
							refreshInterval);
						 
						if (listConfig.ContextMenu != menuType)
						{
							//All manual repairs go to the "slow processor"
							SqlRemarQ.QueueListCommand(listConfig.SiteId, listConfig.WebId, listConfig.ListId, ListCommand.Verify, Constants.ReadUnreadLargeQueueTableName);
						}
						this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,listConfig.Locale, "PropEditOkDokey");
						this.Page.Response.Clear();
						this.Page.Response.Write(Constants.CloseSharePointDialogScript);
						this.Page.Response.End();
					}
					catch (ArgumentOutOfRangeException)
					{
						this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,listConfig.Locale, "PropEditListNotInitialized");
					}
				}
			}
			else
			{
				this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "PropEditNoPermission");
			}
		}
	}
}