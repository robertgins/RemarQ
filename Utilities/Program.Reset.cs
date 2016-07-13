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
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BalsamicSolutions.ReadUnreadSiteColumn;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Utilities
{
	partial class Program
	{
		/// <summary>
		/// Resets the activation settings and if active resets all of the
		/// http modules and activation configuration setttings
		/// </summary>
		static void ResetFarmSettings()
		{
			FarmSettings farmSettings = SPFarm.Local.GetObject(FarmSettings.SettingsId) as  FarmSettings;
			if (null != farmSettings)
			{
				farmSettings.Activated = true;
				farmSettings.Update();
			}
			if (FarmLicense.License.IsLicensed() && farmSettings.TestedOk && farmSettings.Activated)
			{
				BalsamicSolutions.ReadUnreadSiteColumn.Utilities.InstallJobsIfValid();
				if (farmSettings.TrackDocuments)
				{
					ReadUnreadHttpModule.RegisterModule(true);
				}
			}
		}
	}
}