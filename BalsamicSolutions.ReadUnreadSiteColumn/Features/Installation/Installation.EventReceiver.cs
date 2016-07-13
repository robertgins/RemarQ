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