// -----------------------------------------------------------------------------
//  Copyright 4/29/2014 (c) Balsamic Solutions, Inc. All rights reserved.
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