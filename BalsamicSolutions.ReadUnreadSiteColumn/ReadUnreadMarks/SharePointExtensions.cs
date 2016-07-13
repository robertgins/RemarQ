// -----------------------------------------------------------------------------
//  Copyright 5/2/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration.Claims;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// Utility extensions for SharePoint objects
	/// </summary>
	internal static class SharePointExtensions
	{
		/// <summary>
		/// get an item  from a Url, but only get the properties we need
		/// </summary>
		/// <param name="thisWeb"></param>
		/// <param name="itemOrFilePath"></param>
		/// <returns></returns>
		public static SPListItem GetReadUnreadItemByUrl(this SPWeb thisWeb, string itemOrFilePath)
		{
			if (null == thisWeb)
			{
				throw new ArgumentNullException("thisWeb");
			}
			SPListItem returnValue = null;
			try
			{
				returnValue = thisWeb.GetListItemFields(itemOrFilePath, Constants.ReadUnreadQueryFields());
			}
			catch (ArgumentException)
			{
				RemarQLog.LogMessage(string.Format(CultureInfo.InvariantCulture, "Nothing found in call to GetReadUnreadItemByUrl for '{0}'", itemOrFilePath));
				returnValue = null;
			}
			return returnValue;
		}

		/// <summary>
		/// get an item  from an Id, but only get the properties we need
		/// </summary>
		/// <param name="thisList"></param>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public static SPListItem GetReadUnreadItemById(this SPList thisList, int itemId)
		{
			if (null == thisList)
			{
				throw new ArgumentNullException("thisList");
			}
			SPListItem returnValue = null;
			try
			{
				returnValue = thisList.GetItemByIdSelectedFields(itemId, Constants.ReadUnreadQueryFields());
			}
			catch (ArgumentException)
			{
				RemarQLog.LogMessage(string.Format(CultureInfo.InvariantCulture, "Nothing found in call to GetReadUnreadItemById for '{0}'", itemId));
				returnValue = null;
			}
			return returnValue;
		}

		/// <summary>
		/// checks to see if a list is a discussion board
		/// </summary>
		/// <param name="thisList"></param>
		/// <returns></returns>
		public static bool IsDiscussionBoard(this SPList thisList)
		{
			if (null == thisList)
			{
				throw new ArgumentNullException("thisList");
			}
			return (thisList.BaseTemplate == SPListTemplateType.DiscussionBoard);
		}

		/// <summary>
		/// checks to see if a list is a library
		/// </summary>
		/// <param name="thisList"></param>
		/// <returns></returns>
		public static bool IsDocumentLibrary(this SPList thisList)
		{
			if (null == thisList)
			{
				throw new ArgumentNullException("thisList");
			}
			return (thisList.BaseTemplate == SPListTemplateType.DocumentLibrary);
		}

		/// <summary>
		/// checks to see if a list is a "simple one" that we
		/// know how to render
		/// </summary>
		/// <param name="thisList"></param>
		/// <returns></returns>
		public static bool IsSimpleList(this SPList thisList)
		{
			if (null == thisList)
			{
				throw new ArgumentNullException("thisList");
			}

			if (thisList.BaseTemplate == SPListTemplateType.GenericList)
			{
				return true;
			}
			if (thisList.BaseTemplate == SPListTemplateType.Announcements)
			{
				return true;
			}
			if (thisList.BaseTemplate == SPListTemplateType.Links)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// checks to see if a view exists without throwing an error
		/// </summary>
		/// <param name="thisList"></param>
		/// <returns></returns>
		public static bool ContainsView(this SPList thisList, string viewTitle)
		{
			if (null == thisList)
			{
				throw new ArgumentNullException("thisList");
			}
			foreach (SPView checkMe in thisList.Views)
			{
				if (checkMe.Title.Equals(viewTitle, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// deletes a view by title if it exists
		/// </summary>
		/// <param name="thisList"></param>
		/// <param name="viewTitle"></param>
		public static void DeleteView(this SPList thisList, string viewTitle)
		{
			if (null == thisList)
			{
				throw new ArgumentNullException("thisList");
			}
			List<SPView> deleteThese = new List<SPView>();
			//yes we need to collect a list of them as its possible 
			//for titles to overlap in very odd circumstances
			foreach (SPView checkMe in thisList.Views)
			{
				if (checkMe.Title.Equals(viewTitle, StringComparison.OrdinalIgnoreCase))
				{
					deleteThese.Add(checkMe);
				}
			}
			foreach (SPView deleteMe in deleteThese)
			{
				thisList.Views.Delete(deleteMe.ID);
			}
		}
        
		/// <summary>
		/// check to see if an item is a folder
		/// </summary>
		/// <param name="thisItem"></param>
		/// <returns></returns>
		public static bool IsFolder(this SPListItem thisItem)
		{
			bool returnValue = false;
			if (null != thisItem)
			{
				returnValue = thisItem.ContentTypeId.IsChildOf(SPBuiltInContentTypeId.Folder);
			}
			return returnValue;
		}

		/// <summary>
		/// check to see if an item is a file
		/// </summary>
		/// <param name="thisItem"></param>
		/// <returns></returns>
		public static bool IsFile(this SPListItem thisItem)
		{
			bool returnValue = false;
			if (null != thisItem)
			{
				returnValue = thisItem.ContentTypeId.IsChildOf(SPBuiltInContentTypeId.Document);
			}
			return returnValue;
		}

		/// <summary>
		/// check to see if an item is an Item (not a file or folder
		/// </summary>
		/// <param name="thisItem"></param>
		/// <returns></returns>
		public static bool IsItem(this SPListItem thisItem)
		{
			bool returnValue = false;
			if (null != thisItem)
			{
				returnValue = !IsFile(thisItem) && !IsFolder(thisItem);
			}
			return returnValue;
		}

		/// <summary>
		/// returns the parent item in a discussiosn thread
		/// </summary>
		/// <param name="thisItem"></param>
		/// <returns></returns>
		public static SPListItem ParentDiscussionThreadItem(this SPListItem thisItem)
		{
			if (null == thisItem)
			{
				throw new ArgumentNullException("thisItem");
			}
			SPListItem returnValue = null;
			if (thisItem.ParentList.IsDiscussionBoard())
			{
				int parentId = thisItem.IntValue("ParentItemID");
				if (parentId > int.MinValue)
				{
					returnValue = thisItem.ParentList.GetReadUnreadItemById(parentId);
				}
			}
			return returnValue;
		}

		/// <summary>
		/// returns the best name for an item
		/// </summary>
		/// <param name="thisItem"></param>
		/// <returns></returns>
		public static string FileOrFolderName(this SPListItem thisItem)
		{
			if (null == thisItem)
			{
				throw new ArgumentNullException("thisItem");
			}
			string returnValue = thisItem.Name;
			if (returnValue.IsNullOrEmpty())
			{
				try
				{
					returnValue = thisItem[SPBuiltInFieldId.Name].ToString();
				}
				catch (ArgumentException)
				{
					//SharePointLog.Current.LogError("invalid file or field name", argError);
					returnValue = thisItem[SPBuiltInFieldId.FileDirRef].ToString();
				}
			}
			return returnValue;
		}

		/// <summary>
		/// returns the path for an item
		/// </summary>
		/// <param name="thisItem"></param>
		/// <returns></returns>
		public static string ReadUnreadPath(this SPListItem thisItem)
		{
			if (null == thisItem)
			{
				throw new ArgumentNullException("thisItem");
			}
			if (thisItem.ParentList.IsDiscussionBoard())
			{
				return thisItem.StringValue("ThreadIndex");
			}
			else
			{
				string rootFolderPath = thisItem.ParentList.RootFolderPath();
				return thisItem.Url.Substring(rootFolderPath.Length);
			}
		}

		public static string ReadUnreadLeafName(this SPListItem thisItem)
		{
			if (null == thisItem)
			{
				throw new ArgumentNullException("thisItem");
			}
			string itemPath = thisItem.ReadUnreadPath();
			string parentPath = thisItem.ReadUnreadParentPath();
			//special case for root of a discussion board
			if(itemPath.StartsWith("0x") && parentPath.Equals("/"))
			{
				return itemPath;
			}
			return itemPath.Substring(parentPath.Length).Trim(new char[] { '/' });
		}

		/// <summary>
		/// returns the parent path for an item 
		/// </summary>
		/// <param name="thisItem"></param>
		/// <returns></returns>
		public static string ReadUnreadParentPath(this SPListItem thisItem)
		{
			if (null == thisItem)
			{
				throw new ArgumentNullException("thisItem");
			}
			if (thisItem.ParentList.IsDiscussionBoard())
			{
				string[] parentIndices = thisItem.ParentThreadIndices(false).ToArray();
				if (parentIndices.Length == 0)
				{
					return "/";
				}
				else
				{
					return parentIndices[parentIndices.Length - 1];
				}
			}
			else
			{
				string itemPath = thisItem.ReadUnreadPath();
				string[] pathParts = itemPath.Split(new string[] { "/" }, 255, StringSplitOptions.RemoveEmptyEntries);
				if (pathParts.Length == 1 || pathParts.Length == 0)
				{
					return "/";
				}
				else
				{
					return "/" + string.Join("/", pathParts, 0, pathParts.Length - 1);
				}
			}
		}

		/// <summary>
		/// breaks the item path string into an array
		/// representing the parent path to this item
		/// </summary>
		/// <param name="thisItem"></param>
		/// <returns></returns>
		public static string[] ReadUnreadParentPaths(this SPListItem thisItem)
		{
			if (null == thisItem)
			{
				throw new ArgumentNullException("thisItem");
			}
			List<string> returnValue = null;
			if (thisItem.ParentList.IsDiscussionBoard())
			{
				returnValue = new List<string>(thisItem.ParentThreadIndices(true));
				returnValue.Reverse();
			}
			else
			{
				returnValue = new List<string>();
				string parentPath = thisItem.ReadUnreadParentPath();
				returnValue.Add("/");
				string[] pathParts = parentPath.Split(new string[] { "/" }, 255, StringSplitOptions.RemoveEmptyEntries);
				for (int partIdx = 0; partIdx < pathParts.Length; partIdx++)
				{
					returnValue.Add("/" + string.Join("/", pathParts, 0, partIdx + 1));
				}
			}
			return returnValue.ToArray();
		}
        
		/// <summary>
		/// returns the name of the item
		/// </summary>
		/// <param name="thisFolder"></param>
		/// <returns></returns>
		public static string FileOrFolderName(this SPFolder thisFolder)
		{
			if (null == thisFolder)
			{
				throw new ArgumentNullException("thisFolder");
			}
			return thisFolder.Item.FileOrFolderName();
		}

		/// <summary>
		/// returns the sharepoint path of the parent container
		/// </summary>
		/// <param name="thisItem"></param>
		/// <returns></returns>
		public static string ParentFolderPath(this SPListItem thisItem)
		{
			if (null == thisItem)
			{
				throw new ArgumentNullException("thisItem");
			}
			string returnValue = thisItem[SPBuiltInFieldId.FileDirRef].ToString();
			return Utilities.CleanUpSharePointPathName(returnValue);
		}

		/// <summary>
		///  returns the sharepoint path of the parent container
		/// </summary>
		/// <param name="thisFolder"></param>
		/// <returns></returns>
		public static string ParentFolderPath(this SPFolder thisFolder)
		{
			if (null == thisFolder)
			{
				throw new ArgumentNullException("thisFolder");
			}
			return thisFolder.Item.ParentFolderPath();
		}

		/// <summary>
		///    returns the sharepoint path of the parent list
		/// </summary>
		/// <param name="thisItem"></param>
		/// <returns></returns>
		public static string RootFolderPath(this SPListItem thisItem)
		{
			if (null == thisItem)
			{
				throw new ArgumentNullException("thisItem");
			}
			return thisItem.ParentList.RootFolderPath();
		}

		/// <summary>
		///    returns the sharepoint path of the parent list
		/// </summary>
		/// <param name="thisFolder"></param>
		/// <returns></returns>
		public static string RootFolderPath(this SPFolder thisFolder)
		{
			if (null == thisFolder)
			{
				throw new ArgumentNullException("thisFolder");
			}
			return thisFolder.Item.RootFolderPath();
		}

		/// <summary>
		///    returns the sharepoint path of the parent list
		/// </summary>
		/// <param name="thisFolder"></param>
		/// <returns></returns
		public static string RootFolderPath(this SPList thisList)
		{
			if (null == thisList)
			{
				throw new ArgumentNullException("thisList");
			}
			return thisList.RootFolder.Url;
		}

	 

		/// <summary>
		/// returns an array of thread indices for each of the
		/// parent itemss up the thread tree
		/// </summary>
		/// <param name="thisItem"></param>
		/// <param name="includeRootIndex"></param>
		/// <returns></returns>
		public static IEnumerable<string> ParentThreadIndices(this SPListItem thisItem, bool includeRootIndex)
		{
			List<string> returnValue = new List<string>();
			string threadIndex = thisItem.StringValue("ThreadIndex");
			if (!threadIndex.IsNullOrWhiteSpace())
			{
				int lengthIdx = threadIndex.Length;
				while (lengthIdx > 46)
				{
					lengthIdx -= 10;
					threadIndex = threadIndex.Substring(0, lengthIdx);
					returnValue.Add(threadIndex);
				}
			}
			if (includeRootIndex  )
			{
				returnValue.Add("/");
			}
			return returnValue;
		}

		/// <summary>
		/// get an int value or return int.minvalue
		/// </summary>
		/// <param name="thisItem"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static int IntValue(this SPListItem thisItem, string propertyName)
		{
			int returnValue = int.MinValue;
			if (null != thisItem && thisItem.Fields.ContainsField(propertyName))
			{
				object propValue = thisItem[propertyName];
				if (null != propValue)
				{
					returnValue = Convert.ToInt32(propValue, CultureInfo.InvariantCulture);
				}
			}
			return returnValue;
		}

		/// <summary>
		/// get a string value or return null
		/// </summary>
		/// <param name="thisItem"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static string StringValue(this SPListItem thisItem, string propertyName)
		{
			string returnValue = null;
			if (null != thisItem && thisItem.Fields.ContainsField(propertyName))
			{
				object propValue = thisItem[propertyName];
				if (null != propValue)
				{
					returnValue = propValue.ToString();
				}
			}
			return returnValue;
		}

		/// <summary>
		/// return the folder of an item if it is a file item
		/// </summary>
		/// <param name="thisItem"></param>
		/// <returns></returns>
		public static SPFolder ParentFolder(this SPListItem thisItem)
		{
			if (thisItem == null)
			{
				return null;
			}
			if (thisItem.FileSystemObjectType == SPFileSystemObjectType.File && thisItem.File != null)
			{
				return thisItem.File.ParentFolder;
			}
			if (thisItem.FileSystemObjectType != SPFileSystemObjectType.Folder || thisItem.Folder == null)
			{
				return null;
			}
			return thisItem.Folder.ParentFolder;
		}
	}
}