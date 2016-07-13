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