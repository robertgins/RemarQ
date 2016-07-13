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
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace BalsamicSolutions.ReadUnreadSiteColumn.Layouts.BalsamicSolutions.ReadUnreadSiteColumn
{
	public partial class ReadUnreadQuery : LayoutsPageBase
	{
		/// <summary>
		/// executes a ranged read unread query
		/// and returns results in response text (not js formatted)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			this.Response.CacheControl = "no-cache";
			this.Response.AddHeader("Pragma", "no-cache");
			this.Response.Expires = -1;

			string itemIdsQS = this.Request.QueryString["itemIds"];
			string listIdQS = this.Request.QueryString["listId"];
			string userIdQS = this.Request.QueryString["userId"];
			string returnValue = "invalid-query";
			try
			{
				Guid listId = new Guid(listIdQS);
				int userId = int.Parse(userIdQS, CultureInfo.InvariantCulture);
				//create a list of ints to check and the range
				int itemFloor = int.MaxValue;
				int itemCeiling = int.MinValue;
				List<int> itemIds = new List<int>();
				foreach (string itemIdQs in itemIdsQS.Split(','))
				{
					int tempInt = -1;
					if (int.TryParse(itemIdQs, out tempInt) && !itemIds.Contains(tempInt))
					{
						if (tempInt < itemFloor)
						{
							itemFloor = tempInt;
						}
						if (tempInt > itemCeiling)
						{
							itemCeiling = tempInt;
						}
						itemIds.Add(tempInt);
					}
				}
				Dictionary<int, bool> readMarks = null;
				ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(listId);
				if (null != listConfig)
				{
					if (null != SPContext.Current && null != SPContext.Current.Web && listConfig.WebId == SPContext.Current.Web.ID)
					{
						readMarks = GetReadMarks(listConfig, SPContext.Current.Web, userId, itemFloor, itemCeiling, itemIds.ToArray());
					}
					else
					{
						readMarks = GetReadMarks(listConfig, userId, itemFloor, itemCeiling, itemIds.ToArray());
					}
				}
				returnValue = string.Empty;
				foreach (int itemId in readMarks.Keys)
				{
					returnValue += string.Format(CultureInfo.InvariantCulture, ",{0}={1}", itemId, readMarks[itemId].ToString().ToLowerInvariant());
				}
				this.Response.StatusCode = 200;
			}
			catch (SqlException sqlError)
			{
				RemarQLog.LogError(string.Format(CultureInfo.InvariantCulture, "Unexpected data error processing query request for '{0}", this.Request.Url), sqlError);
				//returnValue = sqlError.Message;
				this.Response.StatusCode = 205;
			}
			catch (SPException innerErr)
			{
				RemarQLog.LogError(string.Format(CultureInfo.InvariantCulture, "Unexpected SharePoint error processing query request for '{0}", this.Request.Url), innerErr);
				//returnValue = innerErr.Message;
				this.Response.StatusCode = 206;
			}
			this.Response.Write(returnValue);
			System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest();
		}

		static	Dictionary<int, bool> GetReadMarks(ListConfiguration listConfig, int userId, int itemFloor, int itemCeiling, int[] itemIds)
		{
			using (SPSite spSite = new SPSite(listConfig.SiteId))
			{
				using (SPWeb spWeb = spSite.OpenWeb(listConfig.WebId))
				{
					return GetReadMarks(listConfig, spWeb, userId, itemFloor, itemCeiling, itemIds);
				}
			}
		}

		static Dictionary<int, bool> GetReadMarks(ListConfiguration listConfig, SPWeb spWeb, int userId, int itemFloor, int itemCeiling, int[] itemIds)
		{
			Dictionary<int, bool> returnValue = new Dictionary<int, bool>();
			if (spWeb.CurrentUser.ID == userId)
			{ 
				RangedReadMarksQuery rangeQuery = new RangedReadMarksQuery(spWeb.CurrentUser, listConfig.ListId, itemFloor, itemCeiling - itemFloor, true);
				foreach (int itemId in itemIds)
				{
					returnValue.Add(itemId, rangeQuery.IsRead(itemId));
				}
			}
			return returnValue;
		}
	}
}