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