// -----------------------------------------------------------------------------
//  Copyright 6/28/2014 (c) Balsamic Solutions, Inc. All rights reserved.
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
using System.Threading.Tasks;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// specialized 
	/// collections of lists, with associated web and site id's
	/// </summary>
	public class SharePointListDictionary
	{
		readonly Dictionary<Guid, Dictionary<Guid, HashSet<Guid>>> _AllLists = new Dictionary<Guid, Dictionary<Guid, HashSet<Guid>>>();

		internal void Add(ListConfiguration listConfig)
		{
			this.Add(listConfig.SiteId,listConfig.WebId,listConfig.ListId);
		}

		public void Add(Guid siteId, Guid webId, Guid listId)
		{
			if (!this._AllLists.ContainsKey(siteId))
			{
				this._AllLists.Add(siteId, new Dictionary<Guid, HashSet<Guid>>());
			}
			if (!this._AllLists[siteId].ContainsKey(webId))
			{
				this._AllLists[siteId].Add(webId, new HashSet<Guid>());
			}
			this._AllLists[siteId][webId].Add(listId);
		}

		public ICollection<Guid> SiteIds()
		{
			return new List<Guid>(this._AllLists.Keys).AsReadOnly();
			//return this._AllLists.Keys;
		}

		public ICollection<Guid> WebIds(Guid siteId)
		{
			return new List<Guid>(this._AllLists[siteId].Keys).AsReadOnly();
			//return this._AllLists[siteId].Keys;
		}

		public ICollection<Guid> ListIds(Guid siteId, Guid webId)
		{
			//return this._AllLists[siteId][webId];
			return new List<Guid>(this._AllLists[siteId][webId]).AsReadOnly();
		}

		public int Count
		{
			get
			{
				int returnValue = 0;
				foreach (Guid siteId in this._AllLists.Keys)
				{
					foreach (Guid webId in this._AllLists[siteId].Keys)
					{
						returnValue += this._AllLists[siteId][webId].Count;
					}
				}
				return returnValue;
			}
		}
	}
}