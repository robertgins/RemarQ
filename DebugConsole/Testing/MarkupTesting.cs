// -----------------------------------------------------------------------------
//  Copyright 7/4/2016 (c) Balsamic Software, LLC. All rights reserved.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF  ANY KIND, EITHER 
//  EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR 
//  A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
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
