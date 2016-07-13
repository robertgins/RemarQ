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
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml;
using BalsamicSolutions.ReadUnreadSiteColumn;
using Microsoft.Office.Server.Utilities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Navigation;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebPartPages;

namespace DebugConsole.Testing
{
	internal class MarkupTesting
	{
		List<SPUser> _SharePointUsers = new List<SPUser>();

		internal MarkupTesting(string siteUrl, string[] userNames)
		{
			using (SPSite thisSite = new SPSite(siteUrl))
			{
				using (SPWeb thisWeb = thisSite.RootWeb)
				{
					foreach (string logonName in userNames)
					{
						SPUser sharePointUser = thisWeb.EnsureUser(logonName);
						this._SharePointUsers.Add(sharePointUser);
					}
				}
			}
		}

		internal MarkupTesting(List<SPUser> sharePointUsers)
		{
			this._SharePointUsers.AddRange(sharePointUsers);
		}

		internal void MarkupLibrary(SPList populateMe)
		{
			Console.WriteLine("re-marQing " + populateMe.Title);
			List<int> itemIds = new List<int>();
			foreach (SPListItem listItem in populateMe.Items)
			{
				itemIds.Add(listItem.ID);
			}
			int[] allItemIds = itemIds.ToArray();
			foreach (SPUser sharepointUser in this._SharePointUsers)
			{
				SqlRemarQ.MarkRead(allItemIds, populateMe.ID, sharepointUser);
			}
		}

		internal void MarkupLibraires(SharePointListDictionary listDict)
		{
			foreach (Guid siteId in listDict.SiteIds())
			{
				using (SPSite listSite = new SPSite(siteId))
				{
					foreach (Guid webId in listDict.WebIds(siteId))
					{
						using (SPWeb listWeb = listSite.AllWebs[webId])
						{
							foreach (Guid listId in listDict.ListIds(siteId, webId))
							{
								SPList populateMe = listWeb.Lists[listId];
								Console.WriteLine("re-marQing " + populateMe.Title);
								List<int> itemIds = new List<int>();
								foreach (SPListItem listItem in populateMe.Items)
								{
									itemIds.Add(listItem.ID);
								}
								int[] allItemIds = itemIds.ToArray();
								foreach (SPUser sharepointUser in this._SharePointUsers)
								{
									SqlRemarQ.MarkRead(allItemIds, listId, sharepointUser);
								}
							}
						}
					}
				}
			}
		}

		internal void MarkupDiscussionBoards(SharePointListDictionary listDict)
		{
			foreach (Guid siteId in listDict.SiteIds())
			{
				using (SPSite listSite = new SPSite(siteId))
				{
					foreach (Guid webId in listDict.WebIds(siteId))
					{
						using (SPWeb listWeb = listSite.AllWebs[webId])
						{
							foreach (Guid listId in listDict.ListIds(siteId, webId))
							{
								SPList populateMe = listWeb.Lists[listId];
								Console.WriteLine("re-marQing " + populateMe.Title);
								List<int> itemIds = new List<int>();
								foreach (SPListItem listItem in populateMe.Items)
								{
									itemIds.Add(listItem.ID);
								}
								int[] allItemIds = itemIds.ToArray();
								foreach (SPUser sharepointUser in this._SharePointUsers)
								{
									SqlRemarQ.MarkRead(allItemIds, listId, sharepointUser);
								}
							}
						}
					}
				}
			}
		}

		internal void MarkupAnnouncments(SharePointListDictionary listDict)
		{
			foreach (Guid siteId in listDict.SiteIds())
			{
				using (SPSite listSite = new SPSite(siteId))
				{
					foreach (Guid webId in listDict.WebIds(siteId))
					{
						using (SPWeb listWeb = listSite.AllWebs[webId])
						{
							foreach (Guid listId in listDict.ListIds(siteId, webId))
							{
								SPList populateMe = listWeb.Lists[listId];
								Console.WriteLine("re-marQing " + populateMe.Title);
								List<int> itemIds = new List<int>();
								foreach (SPListItem listItem in populateMe.Items)
								{
									itemIds.Add(listItem.ID);
								}
								int[] allItemIds = itemIds.ToArray();
								foreach (SPUser sharepointUser in this._SharePointUsers)
								{
									SqlRemarQ.MarkRead(allItemIds, listId, sharepointUser);
								}
							}
						}
					}
				}
			}
		}
	}
}
