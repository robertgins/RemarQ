// -----------------------------------------------------------------------------
//  Copyright 6/5/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// this class handles querying our list hierarchy 
	/// from the hierarchy view, it also queries
	/// read mark status to determine which items
	/// are impacted by a particular read/unread 
	/// processing request
	/// </summary>
	internal class RemarQuery
	{
		readonly SPListItem _RootItem = null;
		readonly bool _IsDicussion = false;
		readonly Guid _ListId = Guid.Empty;
		readonly string _ItemPath = string.Empty;
		readonly string _ParentPath = string.Empty;
		readonly string _TableName = string.Empty;
		readonly string _ViewName = string.Empty;
		 
		List<HierarchyItem> _ParentFolders = null;

		internal RemarQuery(SPListItem listItem)
			: this(listItem.ParentList)
		{
			this._RootItem = listItem;
			this._ItemPath = this._RootItem.ReadUnreadPath();
			this._ParentPath = this._RootItem.ReadUnreadParentPath();
		}

		internal RemarQuery(SPList queryList)
		{
			if (null == queryList)
			{
				throw new ArgumentNullException("queryList");
			}
			this._RootItem = null;
			this._ItemPath = "/";
			this._ParentPath = string.Empty;
			this._ListId = queryList.ID;
			this._IsDicussion = queryList.IsDiscussionBoard();
			this._TableName = SqlRemarQ.TableName(this._ListId);
			this._ViewName = SqlRemarQ.HierarchyViewName(this._ListId);
		}

		internal RemarQuery(ListConfiguration listConfig, bool isDiscussionBoard)
		{
			if (null == listConfig)
			{
				throw new ArgumentNullException("listConfig");
			}
			this._RootItem = null;
			this._ItemPath = "/";
			this._ParentPath = string.Empty;
			this._ListId = listConfig.ListId;
			this._IsDicussion = isDiscussionBoard;
			this._TableName = SqlRemarQ.TableName(this._ListId);
			this._ViewName = SqlRemarQ.HierarchyViewName(this._ListId);
		}

		#region List and document library querying
		
		/// <summary>
		/// is this a discussion board
		/// </summary>
		internal bool IsDiscussionBoard
		{
			get { return this._IsDicussion; }
		}
		
		/// <summary>
		/// returns a  list of items at the same hierarchy 
		/// position   
		/// </summary>
		/// <returns></returns>
		internal List<int> PeerItemsAndFolders()
		{
			StringBuilder sqlQuery = new StringBuilder();
			sqlQuery.AppendFormat(CultureInfo.InvariantCulture, "SELECT ItemId FROM [{0}] WHERE Path = ", this._ViewName);
			sqlQuery.AppendFormat(CultureInfo.InvariantCulture, "'{0}' ", this._ParentPath);
			List<int> returnValue = SqlRemarQ.ItemIDQuery(sqlQuery.ToString());
			return returnValue;
		}
		
		/// <summary>
		/// returns a list of any child objects
		/// </summary>
		/// <returns></returns>
		internal List<int> ChildItemsAndFolders()
		{
			return SqlRemarQ.ChildItems(this._ListId, this._ItemPath);
		}
		
		/// <summary>
		/// return a list of parent folders
		/// for this item
		/// </summary>
		/// <returns></returns>
		internal List<int> ParentFolderIds()
		{
			List<int> returnValue = new List<int>();
			foreach (HierarchyItem parentItem in this.ParentFolders)
			{
				returnValue.Add(parentItem.ItemId);
			}
			return returnValue;
		}
		
		/// <summary>
		/// review the item path and if its a discussion board break it up by lengty
		/// otherwise break it up by /
		/// </summary>
		/// <param name="itemPath"></param>
		/// <param name="pathPart"></param>
		/// <param name="leafPart"></param>
		static void AnalyzePathPartsForQuery(string itemPath, out string pathPart, out string leafPart)
		{
			pathPart = null;
			leafPart = null;
			if (itemPath.StartsWith("0x", StringComparison.Ordinal))
			{
				//This is a discussion thread so its length based not / delimited
				//the root item is 46 characters long, then each node is 10 more characters
				if (itemPath.Length <= 46)
				{
					pathPart = "/";
					leafPart = itemPath;
				}
				else
				{
					int pathLength = itemPath.Length - 10;
					pathPart = itemPath.Substring(0, pathLength);
					leafPart = itemPath.Substring(pathLength);
				}
			}
			else
			{
				string[] pathParts = itemPath.Split('/');
				if (pathParts.Length > 0)
				{
					leafPart = pathParts[pathParts.Length - 1];
					pathPart = string.Join("/", pathParts, 0, pathParts.Length - 1);
					if (string.IsNullOrEmpty(pathPart))
					{
						pathPart = "/";
					}
				}
			}
		}

		/// <summary>
		/// return a list of parent folders
		/// for this item
		/// </summary>
		/// <returns></returns>
		internal List<HierarchyItem> ParentFolders
		{
			get
			{
				if (null == this._ParentFolders)
				{
					string[] parentPaths = this._RootItem.ReadUnreadParentPaths();
					if (parentPaths.Length > 1)
					{
						StringBuilder sqlQuery = new StringBuilder();
						sqlQuery.AppendFormat(CultureInfo.InvariantCulture, "SELECT ItemId, Path, Leaf FROM [{0}] WHERE ", this._ViewName);
				
						string pathPart = string.Empty;
						string leafPart = string.Empty;
						AnalyzePathPartsForQuery(parentPaths[1], out pathPart, out leafPart);
						sqlQuery.AppendFormat(CultureInfo.InvariantCulture, "(Path='{0}' AND Leaf='{1}')", pathPart, leafPart);
						for (int pathIdx = 2; pathIdx < parentPaths.Length; pathIdx++)
						{
							AnalyzePathPartsForQuery(parentPaths[pathIdx], out pathPart, out leafPart);
							sqlQuery.AppendFormat(CultureInfo.InvariantCulture, " OR (Path='{0}' AND Leaf='{1}')", pathPart, leafPart);
						}
						 
						sqlQuery.AppendLine();
						this._ParentFolders = SqlRemarQ.HierarchyItemQuery(sqlQuery.ToString());
					}
					else
					{
						this._ParentFolders = new List<HierarchyItem>();
					}
				}
				return this._ParentFolders;
			}
		}
		
		/// <summary>
		///  checks to see if all the items in this items folder are also read
		/// </summary>
		/// <param name="eventProperties"></param>
		/// <param name="itemPath"></param>
		/// <param name="folderId"></param>
		/// <returns></returns>
		internal bool AreAllPeerItemsRead(SPItemEventProperties eventProperties, string itemPath, int folderId)
		{
			SPWeb eventWeb = eventProperties.Web;
			SPUser eventUser = eventWeb.AllUsers[eventProperties.UserLoginName];
			return this.AreAllPeerItemsRead(eventUser, itemPath, folderId);
		}

		/// <summary>
		/// checks to see if all the items in this items folder are also read
		/// </summary>
		/// <param name="spUser"></param>
		/// <param name="itemPath"></param>
		/// <param name="folderId"></param>
		/// <returns></returns>
		internal bool AreAllPeerItemsRead(SPUser spUser, string itemPath, int folderId)
		{
			//Select the fist item from the hierarchivy view where the path matchs the 
			//items path and has not alreadly been marked as read
			string storedProcName = "dbo.rq_AreAllPeerItemsRead" + this._ListId.ToString("N");
			List<object[]> queryResults = SqlRemarQ.ExecRqStoredQuery(storedProcName, this._RootItem.ID, folderId, spUser.ID, itemPath, null);
			return queryResults.Count == 0;
		}
		
		/// <summary>
		/// return all the unread items in this list for this user
		/// </summary>
		/// <param name="userid"></param>
		/// <returns></returns>
		internal List<int> AllUnreadItems(SPUser spUser)
		{
			string storedProcName = "dbo.rq_AllUnreadItems" + this._ListId.ToString("N");
			List<object[]> queryResults = SqlRemarQ.ExecRqStoredQuery(storedProcName, -1, -1, spUser.ID, null, null);
			List<int> returnValue = new List<int>();
			if (queryResults.Count > 0)
			{
				foreach (object[] rowResults in queryResults)
				{
					foreach (object columValue in rowResults)
					{
						returnValue.Add((int)columValue);
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// gets a list of all readers of the current report item
		/// this should only be called for a report which is only an 
		/// enterprise version but at the moment it is also available
		/// for other versions
		/// </summary>
		/// <returns></returns>
		internal List<ReportData>AllReaders(int versionNum, out List<ReportData> historyInfo)
		{
			List<ReportData> returnValue = new List<ReportData>();
			historyInfo = new List<ReportData>();
			if (null != this._RootItem)
			{
				string storedProcName = "dbo.rq_AllReaders" + this._ListId.ToString("N");
				//TODO optimize this so the query does the work and not the result sort
				List<object[]> queryResults = SqlRemarQ.ExecRqStoredQuery(storedProcName, this._RootItem.ID, -1, int.MinValue, null, null);
				foreach (object[] rowResults in queryResults)
				{
					try
					{
						ReportData reportItem = new ReportData();
						reportItem.ReadOn = (DateTime)rowResults[0];
						reportItem.UserId = (int)rowResults[1];
						reportItem.Version = (int)(long)rowResults[2];
						//if its a version flag then pull it out this only occurs in AllReaders for the current history
						//item 
						if (reportItem.UserId == Constants.VersionFlagId || reportItem.UserId == Constants.HierarchySystemUserId)
						{
							reportItem.UserId = Constants.HierarchySystemUserId;
							historyInfo.Add(reportItem);
						}
						else 
						{ 
							if (versionNum == -1 || reportItem.Version == versionNum)
							{
								returnValue.Add(reportItem);
							}
						}
					}
					catch (IndexOutOfRangeException)
					{
					}
					catch (InvalidCastException)
					{
					}
				}
			}
		 
			return returnValue;
		}
		
		#endregion
	}
}