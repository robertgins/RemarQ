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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// public class that allows Api Access to the 
	/// RemarQ system
	/// </summary>
	public static class ApiAccess
	{
		 

		/// <summary>
		/// determines if the item has been read by the user
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="listItem"></param>
		/// <returns></returns>
		public static bool IsRead(SPUser userId, SPListItem listItem)
		{
			if (null == userId)
			{
				throw new ArgumentNullException("userId");
			}
			if (null == listItem || null == listItem.ParentList)
			{
				throw new ArgumentNullException("listItem");
			}
			bool returnValue = false;
			if (FarmLicense.License.LicenseMode >= LicenseModeType.Enterprise)
			{
				RangedReadMarksQuery rmQuery = new RangedReadMarksQuery(userId,listItem.ParentList.ID,listItem.ID,1,listItem.ParentList.IsDiscussionBoard());
				returnValue = rmQuery.IsRead(listItem.ID);
			}
			return returnValue;
		}

		/// <summary>
		/// marks an item read for the user
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="listItem"></param>
		/// <returns></returns>
		public static bool MarkRead(SPUser userId, SPListItem listItem)
		{
			if (null == userId)
			{
				throw new ArgumentNullException("userId");
			}
			if (null == listItem || null == listItem.ParentList)
			{
				throw new ArgumentNullException("listItem");
			}
			bool returnValue = false;
			if (FarmLicense.License.LicenseMode >= LicenseModeType.Enterprise)
			{
				SharePointRemarQ.MarkRead(listItem, userId);
				returnValue = true;
			}
			return returnValue;
		}

		/// <summary>
		/// marks an item as not read for the user
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="listItem"></param>
		/// <returns></returns>
		public static bool MarkUnread(SPUser userId, SPListItem listItem)
		{
			if (null == userId)
			{
				throw new ArgumentNullException("userId");
			}
			if (null == listItem || null == listItem.ParentList)
			{
				throw new ArgumentNullException("listItem");
			}
			bool returnValue = false;
			if (FarmLicense.License.LicenseMode >= LicenseModeType.Enterprise)
			{
				SharePointRemarQ.MarkUnRead(listItem, userId);
				returnValue = true;
			}
			return returnValue;
		}

		/// <summary>
		/// returns all the item ids in a list that are read for a particular user
		/// </summary>
		/// <param name="userId">User to find </param>
		/// <param name="sharePointList"></param>
		/// <returns></returns>
		public static IEnumerable<int> ReadMarks(SPUser userId, SPList sharePointList)
		{
			if (null == userId)
			{
				throw new ArgumentNullException("userId");
			}
			if (null == sharePointList)
			{
				throw new ArgumentNullException("sharePointList");
			}
			IEnumerable<int> returnValue = null;
			if (FarmLicense.License.LicenseMode >= LicenseModeType.Enterprise)
			{
				RangedReadMarksQuery rmQuery = new RangedReadMarksQuery(userId,sharePointList.ID,0,int.MaxValue,sharePointList.IsDiscussionBoard());
				returnValue = rmQuery.ReadMarks;
			}
			return returnValue;
		}

		/// <summary>
		/// returns report information about a specific list item
		/// </summary>
		/// <param name="listItem"></param>
		/// <returns></returns>
		public static IEnumerable<ReportData> GetReport(SPListItem listItem)
		{
			if (null == listItem || null == listItem.ParentList)
			{
				throw new ArgumentNullException("listItem");
			}
			List<ReportData> returnValue = null;
			if (FarmLicense.License.LicenseMode >= LicenseModeType.Enterprise)
			{
				if (SharePointRemarQ.HasPermissionToRunReport(listItem.ParentList.ParentWeb.CurrentUser, listItem, listItem.ParentList, listItem.ParentList.ParentWeb))
				{ 
					RemarQuery ruQuery = new RemarQuery(listItem);
					List<ReportData> versionIds = null;
					returnValue = ruQuery.AllReaders(-1,out versionIds);
				}
			}
			return returnValue.AsEnumerable();
		}
	}
}