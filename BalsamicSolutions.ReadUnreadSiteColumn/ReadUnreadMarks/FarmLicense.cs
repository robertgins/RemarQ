// -----------------------------------------------------------------------------
//  Copyright 6/21/2016 (c) Balsamic Software, LLC. All rights reserved.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  ANY KIND, EITHER 
//  EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR 
//  A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
		//at open source time, we left this in as a static class that 
		//always issues a valid license with all features
	/// </summary>
	public class FarmLicense
	{
		
		internal LicenseModeType LicenseMode
		{
			get
			{
				return LicenseModeType.CloudFarm;
			}
		}

		internal bool ShouldShowLicenseReminder()
		{
			return false;
		}

		internal bool IsLicensed()
		{
			return true;
		}

		static FarmLicense _License = new FarmLicense();
		/// <summary>
		/// just return our static license
		/// </summary>
		internal static FarmLicense License
		{
			get
			{
				return _License;
			}
		}

		internal static void ExpireCachedObject()
		{
			//Nothing to do
		}

	}
}