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
	public partial class ReadUnreadFarmSettingsEditor : OperationsPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			FarmLicense.ExpireCachedObject();
			FarmSettings.ExpireCachedObject();
			this.btnAdvanced.Enabled = false;
			this.btnMaintenance.Enabled = FarmSettings.Settings.IsOk;
			if (FarmLicense.License.LicenseMode >= LicenseModeType.Professional)
			{
				this.btnAdvanced.Enabled = true;
			}
			this.FarmSettingsTitleLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsTitleLiteral");
			this.FarmSettingsDescriptionLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsDescriptionLiteral");
			this.FarmSettingsMaintenanceDescriptionLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsMaintenanceDescription");


			this.btnAbout.AlternateText = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnAboutLabel") ;
			this.btnConnection.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnConnectionLabel") ;

			this.btnAdvanced.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnAdvancedLabel") ;
			this.btnMaintenance.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnMaintenanceLabel") ;
			

			this.FarmSettingsMaintenanceTitleLiteral.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsMaintenanceTitle") ;
			
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			ScriptLink.RegisterCore(this.Page, false);
		}

		protected string CultureManagerLabel
		{
			get { return Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnLanguageLabel"); }
		}

		protected string MaintenanceLabel
		{
			get { return Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnMaintenanceLabel"); }
		}

		protected string AdvancedSettingsLabel
		{
			get { return Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnAdvancedLabel"); }
		}

		protected string ConnectionSettingsLabel
		{
			get { return Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnConnectionLabel"); }
		}

		protected string LicensingLabel
		{
			get { return Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnLicensingLabel"); }
		}

		protected string AboutLabel
		{
			get { return Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"FarmSettingsBtnAboutLabel"); }
		}
	}
}