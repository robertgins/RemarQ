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