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
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace BalsamicSolutions.ReadUnreadSiteColumn.Features.Installation
{
	/// <summary>
	/// This class handles events raised during feature  installation, uninstallation, and upgrade.
	/// We need to clean up our HTTPModule if it is installed, and also take care of resource files for the 
	/// central admin site 
	/// </summary>
	[Guid("898d5e26-874c-4e61-801e-181793099f00")]
	public class InstallationEventReceiver : SPFeatureReceiver
	{
		/// <summary>
		/// during installation we need to update any language files
		/// install an evaluation license 
		/// Configure the initialization job and the maintenance job
		/// </summary>
		public override void FeatureActivated(SPFeatureReceiverProperties xproperties)
		{
			try
			{
				RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.Features.Installation.FeatureActivated");
				this.ProcessActivation();
				RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.Features.Installation.FeatureActivated");
			}
			catch (Exception sharePointError)
			{
				RemarQLog.LogError("Error in BalsamicSolutions.ReadUnreadSiteColumn.FeatureActivated ", sharePointError);
				throw;
			}
		}

		public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, IDictionary<string, string> parameters)
		{
			try
			{
				RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.Features.Installation.FeatureUpgrading");
				this.ProcessActivation();
				DailyMaintenanceJob.RunJobNow();
				RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.Features.Installation.FeatureUpgrading");
			}
			catch (Exception sharePointError)
			{
				RemarQLog.LogError("Error in BalsamicSolutions.ReadUnreadSiteColumn.FeatureUpgrading ", sharePointError);
				throw;
			}
		}

		public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
		{
			try
			{
				RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.Features.Installation.FeatureDeactivating");
				this.ProcessDeActivation();
				RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.Features.Installation.FeatureDeactivating");
			}
			catch (Exception sharePointError)
			{
				RemarQLog.LogError("Error in BalsamicSolutions.ReadUnreadSiteColumn.FeatureDeactivating ", sharePointError);
				throw;
			}
		}

		public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
		{
			try
			{
				RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.Features.Installation.FeatureUninstalling");
				Utilities.UninstallAllJobs();
				ReadUnreadHttpModule.UnregisterModule(false);
				RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.Features.Installation.FeatureUninstalling");
			}
			catch (Exception sharePointError)
			{
				RemarQLog.LogError("Error in BalsamicSolutions.ReadUnreadSiteColumn.FeatureUninstalling", sharePointError);
				throw;
			}
		}

		void ProcessDeActivation()
		{
			FarmSettings farmSettings = SPFarm.Local.GetObject(FarmSettings.SettingsId) as  FarmSettings;
			if (null != farmSettings)
			{
				farmSettings.Activated = false;
				farmSettings.Update();
			}
			Utilities.UninstallAllJobs();
			ReadUnreadHttpModule.UnregisterModule(false);
		}

		void ProcessActivation()
		{
			Utilities.UninstallAllJobs();
			FarmLicense.ExpireCachedObject();
			FarmSettings.ExpireCachedObject();
			FarmSettings farmSettings = SPFarm.Local.GetObject(FarmSettings.SettingsId) as  FarmSettings;
			if (null == farmSettings)
			{
				farmSettings = new FarmSettings();
				farmSettings.TrackDocuments = false;
				farmSettings.SqlConnectionString = string.Empty;
				farmSettings.TestedOk = false;
				var threadPrincipal = System.Threading.Thread.CurrentPrincipal;
				farmSettings.FarmAdmin = string.Empty;
				if (null != threadPrincipal && null != threadPrincipal.Identity)
				{
					farmSettings.FarmAdmin = threadPrincipal.Identity.AuthenticationType + "://" + threadPrincipal.Identity.Name;
				}
				farmSettings.Update();
			}
			farmSettings.Activated = true;
			farmSettings.Update();

			if (FarmSettings.Settings.IsOk)
			{
				Framework.ResourceManager.Initialize(false);
			}
				 
			

			if (FarmSettings.Settings.IsOk && FarmLicense.License.IsLicensed())
			{
				SqlRemarQ.ProvisionConfigurationTables();
					
				if (farmSettings.TrackDocuments)
				{
					ReadUnreadHttpModule.RegisterModule(true);
				}
				else
				{
					ReadUnreadHttpModule.UnregisterModule(true);
				}
				Utilities.InstallJobsIfValid(); 
			}
			else
			{
				ReadUnreadHttpModule.UnregisterModule(true);
			}
		}
	}
}