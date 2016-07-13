// -----------------------------------------------------------------------------
//  Copyright 6/28/2014 (c) Balsamic Solutions, Inc. All rights reserved.
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
using System.Web;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.ApplicationPages;
using Microsoft.SharePoint.ApplicationPages.WebControls;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
 
namespace BalsamicSolutions.ReadUnreadSiteColumn.Administration
{
	public partial class ReadUnreadFarmSettingsEditorConnection : OperationsPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			this.FarmSettingsSqlConnectionStringInstructionsLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsSqlConnectionStringInstructions");
			this.FarmSettingsAdvancedLabelConfirmSqlPasswordLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsAdvancedLabelConfirmSqlPassword");

			this.FarmSettingsAdvancedLabelSqlServernameLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsAdvancedLabelSqlServername");
			this.FarmSettingsAdvancedLabelSqlDatabaseNameLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsAdvancedLabelSqlDatabaseName");
			this.FarmSettingsAdvancedLabelSqlUserIdLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsAdvancedLabelSqlUserId");
			this.FarmSettingsAdvancedLabelSqlPasswordLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsAdvancedLabelSqlPassword");
			this.FarmSettingsAdvancedLabelConfirmSqlPasswordLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsAdvancedLabelConfirmSqlPassword");
			this.FarmSettingsAdvancedLabelSqlIntegratedSecurityLabel.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsAdvancedLabelSqlIntegratedSecurity");
 
			this.btnTest.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsAdvancedBtnTestSqlConnection");  

			if (SPFarm.Local.CurrentUserIsAdministrator())
			{
				this.btnApply.Enabled = false;
				this.btnApply.Click += this.BtnApply_Click;
				this.btnTest.Click += this.BtnTest_Click;
				if (!this.Page.IsPostBack)
				{
					FarmSettings globalSettings = null;
					try
					{
						globalSettings = (FarmSettings)SPFarm.Local.GetObject(FarmSettings.SettingsId);
					}
					catch (SPException)
					{
						RemarQLog.LogMessage("Invalid or missing configuration object");
						globalSettings = null;
					}
					if (null != globalSettings)
					{
						try
						{
							if (!string.IsNullOrEmpty(globalSettings.SqlConnectionString))
							{
								SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder(globalSettings.SqlConnectionString);
								this.txtSqlServer.Text = connBuilder.DataSource;
								this.txtDataBaseName.Text = connBuilder.InitialCatalog;
								if (connBuilder.IntegratedSecurity)
								{
									this.chkIntegrated.Checked = true;
								}
								else
								{
									this.txtUserId.Text = connBuilder.UserID;
									this.txtPassword.Text = string.Empty;
									this.txtPasswordConfirm.Text = string.Empty;
								}
							}
						}
						catch (SqlException)
						{
							RemarQLog.LogMessage("Invalid or connection string attempt");
							this.lblErrorMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsInvalidSqlConnection");
						}
					}
				}
			}
		}

		void BtnTest_Click(object sender, EventArgs e)
		{
			SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder();
			 
			connBuilder.DataSource = this.txtSqlServer.Text;
			connBuilder.InitialCatalog = this.txtDataBaseName.Text;
			if (this.chkIntegrated.Checked)
			{
				connBuilder.IntegratedSecurity = true;
			}
			else
			{
				if (this.txtPassword.Text != this.txtPasswordConfirm.Text)
				{
					this.lblErrorMessage.Text = System.Web.Security.AntiXss.AntiXssEncoder.HtmlEncode(Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsPasswordsDoNotMatch"), false);
					return;
				}
				connBuilder.UserID = this.txtUserId.Text;
				connBuilder.Password = this.txtPassword.Text;
			}
			string connectionString = connBuilder.ToString();
			if (SqlRemarQ.TestConnectionString(connectionString))
			{
				this.btnApply.Enabled = true;
				if (!connBuilder.IntegratedSecurity)
				{
					this.checkedPw.Value = System.Web.Security.AntiXss.AntiXssEncoder.HtmlEncode(this.txtPassword.Text, false);
				}
			}
			else
			{
				this.lblErrorMessage.Text = System.Web.Security.AntiXss.AntiXssEncoder.HtmlEncode(Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsInvalidSqlConnection"), false);
			}
		}

		void BtnApply_Click(object sender, EventArgs e)
		{
			SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder();
			string postSafePassword = System.Web.Security.AntiXss.AntiXssEncoder.HtmlEncode(this.txtPassword.Text, false);
			connBuilder.DataSource = this.txtSqlServer.Text;
			connBuilder.InitialCatalog = this.txtDataBaseName.Text;
			if (this.chkIntegrated.Checked)
			{
				connBuilder.IntegratedSecurity = true;
			}
			else
			{
				if (this.txtPassword.Text != this.txtPasswordConfirm.Text)
				{
					this.lblErrorMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsPasswordsDoNotMatch");
					return;
				}
				connBuilder.UserID = this.txtUserId.Text;
				connBuilder.Password = postSafePassword;
			}
			string connectionString = connBuilder.ToString();
			if (SqlRemarQ.TestConnectionString(connectionString))
			{
				FarmSettings globalSettings = null;
				try
				{
					globalSettings = (FarmSettings)SPFarm.Local.GetObject(FarmSettings.SettingsId);
				}
				catch (SPException)
				{
					RemarQLog.LogMessage("Corrupt or missing ReadUnreadFarmSettings object");
					SPPersistedObject corruptJunk = SPFarm.Local.GetObject(FarmSettings.SettingsId);
					if (null != corruptJunk)
					{
						corruptJunk.Delete();
					}
					globalSettings = null;
				}
				if (null == globalSettings)
				{
					globalSettings = new FarmSettings();
				}
				globalSettings.SqlConnectionString = connectionString;
				globalSettings.TestedOk = true;
				try
				{
					globalSettings.Update();
					FarmSettings.ExpireCachedObject();
					SqlRemarQ.ProvisionConfigurationTables();
					Utilities.InstallJobsIfValid();
					this.Page.Response.Clear();
					this.Page.Response.Write(Constants.CloseSharePointDialogScript);
					this.Page.Response.End();
				}
				catch (SPException saveError)
				{
					Guid exceptionid = new Guid();
					RemarQLog.LogError(exceptionid.ToString("B"), saveError);
					this.lblErrorMessage.Text = string.Format(CultureInfo.InvariantCulture, Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingSqlSaveErrorTemplate"), exceptionid);
				}
				catch (SqlException sqlError)
				{
					Guid exceptionid = new Guid();
					RemarQLog.LogError(exceptionid.ToString("B"), sqlError);
					this.lblErrorMessage.Text = string.Format(CultureInfo.InvariantCulture, Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingSqlSaveErrorTemplate"), exceptionid);
				}
			}
			else
			{
				this.lblErrorMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsInvalidSqlConnection");
			}
		}
	}
}