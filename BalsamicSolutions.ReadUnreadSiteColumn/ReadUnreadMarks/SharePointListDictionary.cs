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