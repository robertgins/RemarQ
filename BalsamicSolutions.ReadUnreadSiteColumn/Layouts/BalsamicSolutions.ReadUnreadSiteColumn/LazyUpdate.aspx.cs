// -----------------------------------------------------------------------------
//  Copyright 5/30/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Globalization;
using System.Web;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace BalsamicSolutions.ReadUnreadSiteColumn.Layouts.BalsamicSolutions.ReadUnreadSiteColumn
{
	//This page responds to the inline editor, it could have been a nice
	//elegant Ajax or SOAP call but I did it this way , 6 of one 1/2 dozen
	//of the other. The page returns simple text as to the new status of 
	//the row, read, unread or error
	//the requested value is the value of the check box which is checked
	//if the item has been read, unchecked if it has not been read
	//which means on a transition you get a false for unchecked which means
	//set the item to unread. or a true for checked which means set the item
	//as read
	public partial class LazyUpdate : LayoutsPageBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected void Page_Load(object sender, EventArgs e)
		{
			System.Web.HttpContext.Current.Response.CacheControl = "no-cache";
			System.Web.HttpContext.Current.Response.AddHeader("Pragma", "no-cache");
			System.Web.HttpContext.Current.Response.Expires = -1;

			string itemIdQS = this.Request.QueryString["itemId"];
			string listIdQS = this.Request.QueryString["listId"];
			string markUnreadQS = this.Request.QueryString["markUnread"];
			string returnMarksQS = this.Request.QueryString["returnMarks"];
			bool returnMarks = false;
			int itemId = -1;
			Guid listId = Guid.Empty;
			string returnValue = "unknown";
			ListConfiguration listConfig = null;
			try
			{
				bool setUnRead = false;
				bool toggleMark = false;
				
				if (!string.IsNullOrEmpty(returnMarksQS))
				{
					returnMarks = returnMarksQS.Equals("true", StringComparison.OrdinalIgnoreCase);
				}
				if (markUnreadQS.Equals("toggle", StringComparison.OrdinalIgnoreCase))
				{
					toggleMark = true;
				}
				else
				{
					setUnRead = bool.Parse(markUnreadQS);
					toggleMark = false;
				}

				itemId = int.Parse(itemIdQS, CultureInfo.InvariantCulture);
				listId = new Guid(listIdQS);
				listConfig = ListConfigurationCache.GetListConfiguration(listId);
				if (null != listConfig)
				{
					if (null != SPContext.Current && null != SPContext.Current.Web && listConfig.WebId == SPContext.Current.Web.ID)
					{
						returnValue = MarkReadOrUnreadOrToggleIt(listConfig, SPContext.Current.Web, itemId, toggleMark, setUnRead);
					}
					else
					{ 
						returnValue = this.MarkReadOrUnreadOrToggleIt(listConfig, itemId, toggleMark, setUnRead);
					}
				}
				this.Response.StatusCode = 200;
			}
			catch (Exception innerErr)
			{
				RemarQLog.LogError(string.Format(CultureInfo.InvariantCulture, "Unexpected serialization problem accessing page request for '{0}", this.Request.Url), innerErr);
				this.Response.StatusCode = 205;
				returnValue = "error";
			}
			if (this.Response.StatusCode == 200 && returnMarks && null != listConfig)
			{
				this.Server.Transfer("ReadUnreadQuery.aspx");
			}
			else
			{
				this.Response.Write(returnValue);
			}
			HttpContext.Current.ApplicationInstance.CompleteRequest();
		}

		/// <summary>
		/// if the context does not match the list, then do it the slow way
		/// </summary>
		/// <param name="listConfig"></param>
		/// <param name="itemId"></param>
		/// <param name="toggleMark"></param>
		/// <param name="setUnRead"></param>
		/// <returns></returns>
		string MarkReadOrUnreadOrToggleIt(ListConfiguration listConfig, int itemId, bool toggleMark, bool setUnRead)
		{
			string returnValue = string.Empty;
			using (SPSite spSite = new SPSite(listConfig.SiteId))
			{
				using (SPWeb spWeb = spSite.OpenWeb(listConfig.WebId))
				{
					returnValue = MarkReadOrUnreadOrToggleIt(listConfig, spWeb, itemId, toggleMark, setUnRead);
				}
			}
			return returnValue;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="listConfig"></param>
		/// <param name="spWeb"></param>
		/// <param name="itemId"></param>
		/// <param name="toggleMark"></param>
		/// <param name="setUnRead"></param>
		/// <returns></returns>
		static string MarkReadOrUnreadOrToggleIt(ListConfiguration listConfig, SPWeb spWeb, int itemId, bool toggleMark, bool setUnRead)
		{
			string returnValue = string.Empty;
			 
			SPList itemList = spWeb.Lists[listConfig.ListId];
			SPListItem listItem = itemList.GetReadUnreadItemById(itemId);
			if (toggleMark)
			{
				RangedReadMarksQuery readQuery = new RangedReadMarksQuery(spWeb.CurrentUser,listConfig.ListId,itemId,2,itemList.IsDiscussionBoard());
				setUnRead = readQuery.IsRead(itemId);
			}
			if (setUnRead)
			{
				SharePointRemarQ.MarkUnRead(listItem, spWeb.CurrentUser);
				returnValue = "unread";
			}
			else
			{
				SharePointRemarQ.MarkRead(listItem, spWeb.CurrentUser);
				returnValue = "read";
			}
		 
			return returnValue;
		}
	}
}