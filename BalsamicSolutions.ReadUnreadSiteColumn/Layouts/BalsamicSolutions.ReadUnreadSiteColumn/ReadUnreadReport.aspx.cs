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
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace BalsamicSolutions.ReadUnreadSiteColumn.Layouts.BalsamicSolutions.ReadUnreadSiteColumn
{
	public partial class ReadUnreadReport : LayoutsPageBase
	{
		string _NameColumnTitle = "RES:ERR";
	 
		string _ReadOnColumnTitle = "RES:ERR";
		string _QueryUrl = "";

		protected void Page_Load(object sender, EventArgs e)
		{
			this.Response.CacheControl = "no-cache";
			this.Response.AddHeader("Pragma", "no-cache");
			this.Response.Expires = -1;

			string itemIdsQS = this.Request.QueryString["itemId"];
			string listIdQS = this.Request.QueryString["listId"];
	 
			int itemId = int.Parse(itemIdsQS, CultureInfo.InvariantCulture);
			Guid listId = new Guid(listIdQS);
			 
			ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(listId);
			if (null != listConfig)
			{
				using (SPSite spSite = new SPSite(listConfig.SiteId))
				{
					using (SPWeb spWeb = spSite.OpenWeb(listConfig.WebId))
					{
						SPList spList = spWeb.Lists[listConfig.ListId];
						SPListItem spItem = spList.GetItemById(itemId);
					 
						this.LoadVersionIdsIntoComboBox(spItem);
						if (spWeb.UserIsSiteAdmin || SharePointRemarQ.HasPermissionToRunReport(spWeb.CurrentUser, spItem, spList, spWeb))
						{
							this.kendoStuff.Text = FarmSettings.Settings.KendoHeader(listConfig.Locale);
						
							string queryUrl = spList.ParentWebUrl;
							if (!queryUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
							{
								queryUrl += "/";
							}
							queryUrl += SPUtility.ContextLayoutsFolder + "/BalsamicSolutions.ReadUnreadSiteColumn/ReadUnreadReportQuery.aspx?listId=";
							queryUrl += System.Web.HttpUtility.UrlEncode(listConfig.ListId.ToString("D"));
							queryUrl += "&itemId=" + itemIdsQS;
							this._QueryUrl = queryUrl;
							this._NameColumnTitle = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,spWeb.Locale, "ReportNameColumnTitle");
							 
							this._ReadOnColumnTitle = Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,spWeb.Locale, "ReportReadOnColumnTitle");
						}
						else
						{
							//set the flag so we get a good error icon
							this.DisplayError(Framework.ResourceManager.GetString(CultureInfo.CurrentUICulture,  "ReportAccessDenied"),
								SPUtility.ContextImagesRoot + "/BalsamicSolutions.ReadUnreadSiteColumn/AccessDenied.png");
						}
					}
				}
			}
		}

		
		/// <summary>
		/// loads any recognized document versions into the combo box using dates
		/// for display and the version indicator for the value
		/// 
		/// TODO we really should coordiante the items version label with the version id
		/// </summary>
		/// <param name="listItem"></param>
		void LoadVersionIdsIntoComboBox(SPListItem listItem)
		{
			RemarQuery ruQuery = new RemarQuery(listItem);
			List<ReportData> versionIds = null;
			//need to ignore the MSOCAF warning for abandoning readHistory
			List<ReportData> readHistory = ruQuery.AllReaders(Constants.VersionFlagId , out versionIds);
			if (readHistory.Count > 0 || versionIds.Count > 0)
			{
				foreach (ReportData historyId in versionIds)
				{
					string readOn = System.Web.Security.AntiXss.AntiXssEncoder.HtmlEncode(historyId.ReadOn.ToString(listItem.ParentList.ParentWeb.Locale), false);
					ListItem addMe = new ListItem(readOn, historyId.Version.ToString());
					if (historyId.Version == 0)
					{
						addMe.Selected = true;
					}
					this.fileVersions.Items.Insert(0, addMe);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		void DisplayError(string errorMessage, string imageUrl)
		{
			//per MSDN we dont dispose these since "The Dispose method leaves the Control in an unusable state. "
			this.errorTable.Visible = true;
			TableRow textRow = new TableRow();
			TableCell textCell = new TableCell();
		
			textCell.Text = System.Web.Security.AntiXss.AntiXssEncoder.HtmlEncode(errorMessage, false);
			textCell.CssClass = "ms-error";
			textCell.HorizontalAlign = HorizontalAlign.Center;
			textCell.VerticalAlign = VerticalAlign.Middle;
			textRow.Cells.Add(textCell);
			this.errorTable.Rows.Add(textRow);

			TableRow imageRow = new TableRow();
			TableCell imageCell = new TableCell();
			imageCell.HorizontalAlign = HorizontalAlign.Center;
			imageCell.VerticalAlign = VerticalAlign.Top;
			Image errorImage = new Image();
			errorImage.ImageUrl = imageUrl;
			errorImage.Height = new Unit("32px");
			errorImage.Width = new Unit("32px");
			errorImage.BorderWidth = new Unit("0px");
			imageCell.Controls.Add(errorImage);
			imageRow.Cells.Add(imageCell);
			this.errorTable.Rows.Add(imageRow);
		}

		protected string QueryUrl
		{
			get { return this._QueryUrl; }
		}

		protected string NameColumnTitle
		{
			get { return this._NameColumnTitle; }
		}
	 
		protected string ReadOnColumnTitle
		{
			get { return this._ReadOnColumnTitle; }
		}
	}
}