// -----------------------------------------------------------------------------
//  Copyright 6/28/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;

namespace BalsamicSolutions.ReadUnreadSiteColumn.Features.SiteColumn
{
	/// <summary>
	/// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
	/// </summary>
	/// <remarks>
	/// The GUID attached to this class may be used during packaging and should not be modified.
	/// </remarks>
	[Guid("580a383d-3176-4981-aae9-79b9241c0a19")]
	public class SiteColumnEventReceiver : SPFeatureReceiver
	{
		// Uncomment the method below to handle the event raised after a feature has been activated.
		public override void FeatureActivated(SPFeatureReceiverProperties properties)
		{
			try
			{
				RemarQLog.LogMessage("Entering BalsamicSolutions.ReadUnreadSiteColumn.Features.SiteColumn.FeatureActivated");
				FarmLicense.ExpireCachedObject();
				FarmSettings.ExpireCachedObject();
				if (FarmLicense.License.IsLicensed() && FarmSettings.Settings.TestedOk)
				{
					//TODO place holder for eventual site specific database connection
					RemarQLog.LogMessage("BalsamicSolutions.ReadUnreadSiteColumn.Features.SiteColumn.FeatureActivated with good configuration");
				}
				else
				{
					throw new BalsamicSolutions.ReadUnreadSiteColumn.RemarQException("The feature cannot be activated until it is configured");
				}
				RemarQLog.LogMessage("Exiting BalsamicSolutions.ReadUnreadSiteColumn.Features.SiteColumn.FeatureActivated");
			}
			catch (Exception sharePointError)
			{
				RemarQLog.LogError("Error in BalsamicSolutions.ReadUnreadSiteColumn.FeatureDeactivating ", sharePointError);
				throw;
			}
		}
	}
}