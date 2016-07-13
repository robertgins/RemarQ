// -----------------------------------------------------------------------------
//  Copyright 5/7/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// cache of list configurations so they dont have to be
	/// retrieved from Sql every time
	/// </summary>
	internal static class ListConfigurationCache
	{
		#region Fields & Declarations
		
		static readonly Dictionary<Guid, ListConfiguration> _ListInformationCache = new Dictionary<Guid, ListConfiguration>();
		static readonly object _LockProxy = new object();
		static DateTime _CacheExpires = DateTime.MinValue;
		
		#endregion
		
		#region static methods to get a configuration object
		
		static void CheckExpiration()
		{
			if (_CacheExpires < DateTime.Now)
			{
				lock (_LockProxy)
				{
					if (_CacheExpires < DateTime.Now)
					{
						_ListInformationCache.Clear();
#if DEBUG
						_CacheExpires = DateTime.Now.AddMinutes(1);
#else
						_CacheExpires = DateTime.Now.AddMinutes(5);
#endif
					}
				}
			}
		}
		
		/// <summary>
		/// get a list configuration 
		/// </summary>
		/// <param name="listId">List identifier </param>
		/// <returns></returns>
		internal static ListConfiguration GetListConfiguration(Guid listId)
		{
			CheckExpiration();
			ListConfiguration returnValue = null;
			lock (_LockProxy)
			{
				_ListInformationCache.TryGetValue(listId, out returnValue);
				if (null == returnValue)
				{
					try
					{
						returnValue = new ListConfiguration(listId);
						_ListInformationCache.Add(returnValue.ListId, returnValue);
					}
					catch (ArgumentOutOfRangeException)
					{
						returnValue = null;
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// verify that the list configuration for the list associated 
		/// with this field is in the cache
		/// </summary>
		/// <param name="readUnreadField"></param>
		internal static void EnsureCache(ReadUnreadField readUnreadField)
		{
			lock (_LockProxy)
			{
				if (null != readUnreadField && null != readUnreadField.ParentList)
				{
					Guid listId = readUnreadField.ParentList.ID;
					if (!_ListInformationCache.ContainsKey(listId))
					{
						try
						{
							ListConfiguration listInfo = new ListConfiguration(listId);
							_ListInformationCache.Add(listInfo.ListId, listInfo);
						}
						catch (ArgumentOutOfRangeException)
						{
						}
					}
				}
			}
		}
	 
		internal static void UpdateCache(Guid listId)
		{
			lock (_LockProxy)
			{
				if (_ListInformationCache.ContainsKey(listId))
				{
					_ListInformationCache.Remove(listId);
				}
				try
				{
					ListConfiguration listInfo = new ListConfiguration(listId);
					_ListInformationCache.Add(listInfo.ListId, listInfo);
				}
				catch (ArgumentOutOfRangeException)
				{
					//happens during the first time a field is added
					//to a list, because Update is called before
					//OnAdded which is where we provision the list
				}
			}
		}
		
		#endregion
	}
}