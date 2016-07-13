// -----------------------------------------------------------------------------
//  Copyright 4/30/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 

using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// This class handled caching the queries for a users read marks
	/// this way we can cache for a single page rendering, but dont
	/// have to query for all of a users list readmarks at once
	/// </summary>
	internal class RangedReadMarksQuery
	{
		#region Fields & Declarations
		
		/// <summary>
		/// current highwater mark for the items in our result set
		/// </summary>
		public int Ceiling { get; private set; }
		
		/// <summary>
		/// user id for this query
		/// </summary>
		public SPUser SharePointUser { get; private set; }
		
		/// <summary>
		/// list identifier for thsi query
		/// </summary>
		public Guid ListId { get; private set; }
		
		/// <summary>
		/// collection of read marks for this query, existance means 
		/// it is read, the bool indicates that it has unread child
		/// items
		/// </summary>
		readonly HashSet<int> _ReadMarks = new HashSet<int>();
		readonly Dictionary<int, string> _ChildPathMap = new Dictionary<int, string>();
		int _Floor = 0;
		readonly bool _IsDiscussonBoard = false;
		readonly object _LockProxy = new object();
		
		#endregion
		
		#region CTORs
		
		internal RangedReadMarksQuery(SPUser spUser, Guid listId, int itemId, bool isDiscussionBoard)
			: this(spUser, listId, itemId, Constants.SqlReadMarkQueryRangePageSize, isDiscussionBoard)
		{
		}
		
		internal RangedReadMarksQuery(SPUser spUser, Guid listId, int itemId, int rangeSize, bool isDiscussionBoard)
		{
			this.ListId = listId;
			this.SharePointUser = spUser;
			this.Floor = itemId - rangeSize;
			this.Ceiling = itemId + rangeSize;
			this._IsDiscussonBoard = isDiscussionBoard;
			this.UpdatePage(this.Floor, this.Ceiling);
		}
		
		#endregion
		
		#region Range checking and updating
		
		internal bool HasUnreadChildren(int itemId)
		{
			//PERFORMANCE ISSUE ?
			//we may not need to do this one at a time any more
			//as we can load a the child read items from the hierarchy view 
			if (itemId < 0)
			{
				return false;
			}
			//This may or may not force a page update
			//bool forceUpdate = this.IsRead(itemId);
			bool returnValue = false;
			lock (this._LockProxy)
			{
				string childParentPath = string.Empty;
				if (this._ChildPathMap.TryGetValue(itemId, out childParentPath))
				{
					//unread children means that the count of items whose parent paths start
					//with childParentPath is > 0;, we can probably do this with a SQL query
					//but I have not figured out a better technique one yet
					List<int> childItems = SqlRemarQ.ChildItems(this.ListId, childParentPath);
					foreach (int childId in childItems)
					{
						if (!this.IsRead(childId))
						{
							returnValue = true;
							break;
						}
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// Check and see if the item has been read, updating the results if necessary
		/// then check to see if tis in the result set
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		internal bool IsRead(int itemId)
		{
			if (itemId < 0)
			{
				return false;
			}
			lock (this._LockProxy)
			{
				if (!this.InRange(itemId))
				{
					if (itemId < this.Floor)
					{
						int newFloor = itemId - Constants.SqlReadMarkQueryRangePageSize;
						if (newFloor < 0)
						{
							newFloor = 0;
						}
						this.UpdatePage(newFloor, this.Floor);
						this.Floor = newFloor;
					}
					else
					{
						int newCeiling = itemId + +Constants.SqlReadMarkQueryRangePageSize;
						this.UpdatePage(this.Ceiling, newCeiling);
						this.Ceiling = newCeiling;
					}
				}
				return this._ReadMarks.Contains(itemId);
			}
		}
		
		/// <summary>
		/// update our readmarks with a new query , expanding the range to the new
		/// floor/ceiling combination
		/// </summary>
		/// <param name="queryFloor"></param>
		/// <param name="queryCeiling"></param>
		void UpdatePage(int queryFloor, int queryCeiling)
		{
			List<int> readMarks = SqlRemarQ.GetReadMarksFromAListForAUser(this.ListId, this.SharePointUser, queryFloor, queryCeiling);
			Dictionary<int, string> pathMap = SqlRemarQ.GetChildPathMap(this.ListId, queryFloor, queryCeiling, this._IsDiscussonBoard);
			if (this._IsDiscussonBoard)
			{
				foreach (int pathKey in pathMap.Keys)
				{
					this._ChildPathMap[pathKey] = pathMap[pathKey];
				}
			}
			this._ReadMarks.UnionWith(readMarks);
		}
		
		/// <summary>
		/// check the range and if necessary update the query data
		/// </summary>
		/// <param name="rangeFloor"></param>
		/// <param name="rangeCeiling"></param>
		internal void RangeUpdate(int rangeFloor, int rangeCeiling)
		{
			lock (this._LockProxy)
			{
				if (rangeFloor < this.Floor || rangeCeiling > this.Ceiling)
				{
					int queryFloor = rangeFloor;
					int queryCeiling = rangeCeiling;
					if (this.InRange(rangeFloor))
					{
						queryFloor = this.Ceiling;
					}
					else if (this.InRange(rangeCeiling))
					{
						queryCeiling = this.Floor;
					}
					this.UpdatePage(queryFloor, queryCeiling);
					if (rangeFloor < this.Floor)
					{
						this.Floor = rangeFloor;
					}
					if (rangeCeiling > this.Ceiling)
					{
						this.Ceiling = rangeCeiling;
					}
				}
			}
		}
		
		/// <summary>
		/// check to see if we have already issued a query that should include
		/// this item
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		bool InRange(int itemId)
		{
			return (itemId >= this.Floor && itemId <= this.Ceiling);
		}
		
		/// <summary>
		/// what is the lowest item number we have queried for
		/// </summary>
		internal int Floor
		{
			get { return this._Floor; }
			private set
			{
				if (value < 0)
				{
					this._Floor = 0;
				}
				else
				{
					this._Floor = value;
				}
			}
		}
		
		internal IEnumerable<int> ReadMarks
		{
			get
			{
				return new List<int>(_ReadMarks).AsEnumerable();
			}
		}
		#endregion
	}
}