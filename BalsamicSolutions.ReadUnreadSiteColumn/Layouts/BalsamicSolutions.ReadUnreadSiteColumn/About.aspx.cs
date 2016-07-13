// -----------------------------------------------------------------------------
//  Copyright 5/8/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace BalsamicSolutions.ReadUnreadSiteColumn.Layouts.BalsamicSolutions.ReadUnreadSiteColumn
{
	public partial class About : LayoutsPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			this.aboutTitle.Text =  Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"RemarQAboutTitle");
			 
			this.aboutText.Text = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,"RemarQAboutText");
			AssemblyInformationalVersionAttribute infoAttribute = (AssemblyInformationalVersionAttribute)Assembly
																												 .GetExecutingAssembly()
																												 .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
																												 .FirstOrDefault();
			string versionText = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
			versionText += "/" + infoAttribute.InformationalVersion;
			this.versionLabel.Text  = versionText;
		}
	}
}