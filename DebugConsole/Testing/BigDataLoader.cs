// -----------------------------------------------------------------------------
//  Copyright 7/4/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
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
	/// <summary>
	/// loads lots of stuff so that we end up with 1K enabled
	/// lists with 1k items in each with 1K users marked read on each
	/// </summary>
	public class BigDataLoader
	{
		static readonly Random _NumberGen = new Random();
		const int FOLDERS = 10; //10
		const int LIBRARIES = 15; // 25
		const int DISCUSSIONBOARDS = 10; // 15
		const int LISTS = 15; //10
		const int ITEMS = 1000; // 1000
		const string TAG = "RemarQ Testing -";

	 
		public static List<string> AllUsers = new List<string>();
		public static List<SPUser> SharePointUsers = new List<SPUser>();
		public static Dictionary<string, byte[]> SampleFiles = null;

		public static void Initialize(string[] rootSites, string userDomain, bool cleanOnly)
		{
			//this will throw if the proposed site does not exist
			foreach (string siteUrl in rootSites)
			{
				using (SPSite existCheck = new SPSite(siteUrl))
				{
					using (SPWeb exitWeb = existCheck.RootWeb)
					{
						Console.WriteLine("Validated " + exitWeb.Title);
					}
				}
			}
 
			AllUsers.Clear();
			AllUsers.AddRange(ActiveDirectory.GetAllUsersClaimNames(userDomain));
			SampleFiles = GetEmbeddedFiless("DebugConsole.Files.");
			foreach (string rootUrl in rootSites)
			{
				using (SPSite rootSite = new SPSite(rootUrl))
				{
					using (SPWeb rootWeb = rootSite.RootWeb)
					{
						Console.WriteLine("Starting root site " + rootUrl);
						Console.WriteLine("Ensuring Users for " + rootUrl);
						SharePointUsers.Clear();
						foreach (string userClaim in AllUsers)
						{
							SPUser webUser = rootWeb.EnsureUser(userClaim);
							SharePointUsers.Add(webUser);
						}

						using (SPWeb spWeb = rootSite.RootWeb)
						{
							Console.WriteLine("Deleting old data");
							List<Guid> toDelete = new List<Guid>();
							foreach (SPList deleteMe in spWeb.Lists)
							{
								if (deleteMe.Description.StartsWith(TAG))
								{
									toDelete.Add(deleteMe.ID);
								}
							}
							foreach (Guid deleteMe in toDelete)
							{
								spWeb.Lists.Delete(deleteMe);
								spWeb.Update();
							}
							Console.WriteLine("Done deleting old data");
							if (!cleanOnly)
							{
								System.Threading.Thread.Yield();
								CreateDocumentLibraries(spWeb, LIBRARIES);
								System.Threading.Thread.Yield();
								CreateDiscussionGroups(spWeb, DISCUSSIONBOARDS);
								System.Threading.Thread.Yield();
								CreateAnnouncements(spWeb, LISTS);
								System.Threading.Thread.Yield();
							}

						}
						System.Threading.Thread.Sleep(10000);

					}
				}
			}
			Console.WriteLine("Done Creating Data");
		}

		public static void PopulateLibrary(SPFolder populateMe)
		{

			for (int rootFolderId = 1; rootFolderId < FOLDERS + 1; rootFolderId++)
			{
				string rootName = rootFolderId.ToText();
				SPFolder subfolder = populateMe.SubFolders.Add(rootName);
				subfolder.Update();
				for (int childFolderId = 1; childFolderId < FOLDERS + 1; childFolderId++)
				{
					string folderName = "(" + rootName + ") " + childFolderId.ToText();
					SPFolder childFolder = subfolder.SubFolders.Add(folderName);
					childFolder.Update();
					foreach (string fileName in SampleFiles.Keys)
					{
						try
						{
							childFolder.Files.Add(fileName, SampleFiles[fileName], true);
							Console.Write(".");
						}
						catch (Exception err)
						{
							System.Diagnostics.Trace.WriteLine(err.Message);
							Console.Write("*");
							break;
						}
					}
				}
			}
			Console.WriteLine();
		}

		static void PopulateDiscussionThread(SPListItem parentThread, int depth)
		{
			if (depth < 4)
			{
				depth++;
				int numReplies = _NumberGen.Next(0, 5);
				for (int replyIdx = 0; replyIdx < numReplies; replyIdx++)
				{
					SPListItem replyItem = SPUtility.CreateNewDiscussionReply(parentThread);
					replyItem["Body"] = RandomStuff.RandomSentance(10, 245);
					replyItem.Update();
					PopulateDiscussionThread(replyItem, depth);
				}
			}
		}




		public static void PopulateDiscussionBoard(SPList populateMe)
		{

			Console.WriteLine("populating " + populateMe.Title);
			for (int rootFolderId = 1; rootFolderId < FOLDERS + 1; rootFolderId++)
			{
				SPListItem newDiscussion = SPUtility.CreateNewDiscussion(populateMe, RandomStuff.RandomSentance(10, 25));
				newDiscussion["Body"] = RandomStuff.RandomSentance(10, 245);
				newDiscussion.Update();
				PopulateDiscussionThread(newDiscussion, 0);
			}
			Console.WriteLine();
		}



		public static void PopulateAnnouncments(SPList populateMe)
		{

			Console.WriteLine("populating " + populateMe.Title);
			for (int itemNum = 0; itemNum < ITEMS; itemNum++)
			{
				SPListItem addMe = populateMe.Items.Add();
				addMe["Title"] = itemNum.ToText();
				if (addMe.Fields.ContainsField("Body"))
				{
					addMe["Body"] = RandomStuff.RandomSentance(10, 245);
				}
				else if (addMe.Fields.ContainsField("Description"))
				{
					addMe["Description"] = RandomStuff.RandomSentance(10, 245);
				}

				addMe.Update();
				System.Threading.Thread.Yield();
				Console.Write("+");
			}
			Console.WriteLine();
		}


		public static void CreateDocumentLibraries(SPWeb rootWeb, int numlibraries)
		{
			Console.WriteLine("Starting document libraries " + rootWeb.Title);
			for (int listIdx = 1; listIdx < numlibraries + 1; listIdx++)
			{
				string libraryName = "Library " + listIdx.ToText();
				string libraryDescription = TAG + listIdx.ToText();
				 
				Guid listGuid = rootWeb.Lists.Add(libraryName, libraryDescription, SPListTemplateType.DocumentLibrary);
				if (listGuid != Guid.Empty)
				{
					SPList remarQMe = rootWeb.Lists.GetList(listGuid, false);
					 
					SPNavigationNode listNode = rootWeb.Navigation.GetNodeByUrl(remarQMe.DefaultViewUrl);
					if (listNode == null)
					{
						listNode = new SPNavigationNode(remarQMe.Title, remarQMe.DefaultViewUrl);
						listNode = rootWeb.Navigation.AddToQuickLaunch(listNode, SPQuickLaunchHeading.Documents);
					}
					Console.WriteLine("Created library [" + libraryName + "]");
					 
					SPFolder docFolder = remarQMe.RootFolder;
					PopulateLibrary(docFolder);
				}
			}
		}

		public static void CreateDiscussionGroups(SPWeb rootWeb, int numLists)
		{
			Console.WriteLine("Starting discussion boards " + rootWeb.Title);
			for (int listIdx = 1; listIdx < numLists + 1; listIdx++)
			{
				string listName = "Discussion " + listIdx.ToText();
				string listDescription = TAG + listIdx.ToText();
				 
				Guid listGuid = rootWeb.Lists.Add(listName, listDescription, SPListTemplateType.DiscussionBoard);
				if (listGuid != Guid.Empty)
				{
					SPList remarQMe = rootWeb.Lists.GetList(listGuid, false);
					SPNavigationNode listNode = rootWeb.Navigation.GetNodeByUrl(remarQMe.DefaultViewUrl);
					if (listNode == null)
					{
						listNode = new SPNavigationNode(remarQMe.Title, remarQMe.DefaultViewUrl);
						listNode = rootWeb.Navigation.AddToQuickLaunch(listNode, SPQuickLaunchHeading.Discussions);
					}
				 
					Console.WriteLine("Created discussion [" + listName + "]");
					PopulateDiscussionBoard(remarQMe);
				}
			}
		}

		public static void CreateAnnouncements(SPWeb rootWeb, int numLists)
		{
			Console.WriteLine("Starting announcement lists " + rootWeb.Title);
			for (int listIdx = 1; listIdx < numLists + 1; listIdx++)
			{
				string listName = "List " + listIdx.ToText();
				string listDescription = TAG + listIdx.ToText();
			 
				Guid listGuid = rootWeb.Lists.Add(listName, listDescription, SPListTemplateType.Announcements);
				if (listGuid != Guid.Empty)
				{
					SPList remarQMe = rootWeb.Lists.GetList(listGuid, false);
					SPNavigationNode listNode = rootWeb.Navigation.GetNodeByUrl(remarQMe.DefaultViewUrl);
					if (listNode == null)
					{
						listNode = new SPNavigationNode(remarQMe.Title, remarQMe.DefaultViewUrl);
						listNode = rootWeb.Navigation.AddToQuickLaunch(listNode, SPQuickLaunchHeading.Lists);
					}
				 
					Console.WriteLine("Created list [" + listName + "]");
					PopulateAnnouncments(remarQMe);
				}
			}
		}

		static bool IsMatch(string candidateName, string prefixName, string suffixName)
		{
			bool prefixMatch = false;
			bool suffixMatch = false;
			candidateName = candidateName.ToUpperInvariant();
			prefixName = prefixName.ToUpperInvariant();
			suffixName = suffixName.ToUpperInvariant();
			if (prefixName.IsNullOrEmpty() || prefixName == "*")
			{
				prefixMatch = true;
			}
			if (suffixName.IsNullOrEmpty() || suffixName == "*" || suffixName == ".*")
			{
				suffixMatch = true;
			}
			if (!suffixMatch)
			{
				suffixName = suffixName.TrimStart('*');
				suffixMatch = candidateName.EndsWith(suffixName);
			}
			if (!prefixMatch)
			{
				prefixName = prefixName.TrimEnd('*');
				prefixMatch = prefixName.IsNullOrEmpty() || candidateName.StartsWith(prefixName);
			}

			return prefixMatch && suffixMatch;
		}

		internal static Dictionary<string, byte[]> GetEmbeddedFiless(string resourceNameSpace, string fileExtension = "")
		{
			Dictionary<string, byte[]> returnValue = new Dictionary<string, byte[]>(StringComparer.InvariantCultureIgnoreCase);
			Assembly thisAssembly = Assembly.GetExecutingAssembly();
			string[] resourceNames = thisAssembly.GetManifestResourceNames();
			foreach (string resourceName in resourceNames)
			{
				if (IsMatch(resourceName, resourceNameSpace, fileExtension))
				{
					string fileName = resourceName.Substring(resourceNameSpace.Length);
					if (!returnValue.ContainsKey(fileName))
					{
						using (Stream templateStream = thisAssembly.GetManifestResourceStream(resourceName))
						{
							using (MemoryStream memStream = new MemoryStream())
							{
								templateStream.CopyTo(memStream, 4096);
								memStream.Seek(0, SeekOrigin.Begin);
								returnValue.Add(fileName, memStream.ToArray());
							}
						}
					}
				}
			}
			return returnValue;
		}
	}
}


