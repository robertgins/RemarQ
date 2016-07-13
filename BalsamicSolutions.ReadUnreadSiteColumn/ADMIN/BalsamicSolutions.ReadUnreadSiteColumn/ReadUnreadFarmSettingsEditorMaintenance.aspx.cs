// -----------------------------------------------------------------------------
//  Copyright 9/14/2014 (c) Balsamic Solutions, Inc. All rights reserved.
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

namespace BalsamicSolutions.ReadUnreadSiteColumn.Administration
{
	public partial class ReadUnreadFarmSettingsEditorMaintenance : LayoutsPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			this.FarmSettingsMaintenanceDescriptionLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "FarmSettingsMaintenanceDescription");
			this.btnResetJobs.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "FarmSettingsAdvancedResetJobs"); 
			this.btnRunDailyNow.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "FarmSettingsAdvancedRunMaintenanceNowDescription"); 
			this.btnRemoveAllFields.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "FarmSettingsMaintenanceRemoveFieldsDescription"); 
			this.btnResetLanguage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "FarmSettingsMaintenanceResetLanguageDescription"); 
			if (SPFarm.Local.CurrentUserIsAdministrator())
			{
				this.btnRemoveAllFields.Click += this.RemoveAllFields_Click;
				this.btnRunDailyNow.Click += this.RunDailyNow_Click;
				this.btnResetJobs.Click += this.ResetJobs_Click;
				this.btnResetLanguage.Click += this.ResetLanguage_Click;
				this.btnRemoveAllFields.OnClientClick = "return confirm('" + Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "FarmSettingsRemoveFieldsLabelConfirmText") + "')";
			}
		}


		void ResetLanguage_Click(object sender, EventArgs e)
		{
			Framework.ResourceManager.Initialize(true);
			this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "FarmSettingsLanguageReset");
			Framework.ResourceManager.ResetCache();
		}

		void ResetJobs_Click(object sender, EventArgs e)
		{
			FarmLicense.ExpireCachedObject();
			FarmLicense.ExpireCachedObject();
			if (FarmLicense.License.IsLicensed() && FarmSettings.Settings.TestedOk && FarmSettings.Settings.Activated)
			{
				Utilities.InstallJobsIfValid();
				this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "FarmSettingsResetJobs");
			}
			else
			{
				string errorMessage = string.Format(Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "FarmSettingSqlSaveErrorTemplate"), "");
				this.lblMessage.Text = errorMessage;
			}
		}
        
		void RunDailyNow_Click(object sender, EventArgs e)
		{
			if (DailyMaintenanceJob.RunJobNow())
			{
				this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "FarmSettingsbtnRunDailyNowStatus");
			}
			else
			{
				this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "FarmSettingsbtnRunDailyNowError");
			}
		}

		void RemoveAllFields_Click(object sender, EventArgs e)
		{
			ReadUnreadHttpModule.UnregisterModule(false);
			Utilities.UninstallAllJobs();
			RemoveRemarQJob.Install();
			this.lblMessage.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture, "FarmSettingsbtnRemoveAllFieldsStatus");
		}
	}
}