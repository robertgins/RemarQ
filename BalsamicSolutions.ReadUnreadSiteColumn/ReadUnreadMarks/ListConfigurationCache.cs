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