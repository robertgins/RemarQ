// -----------------------------------------------------------------------------
//  Copyright 5/31/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
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