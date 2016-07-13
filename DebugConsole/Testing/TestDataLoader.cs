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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BalsamicSolutions.ReadUnreadSiteColumn;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Navigation;
using Microsoft.SharePoint.Utilities;

namespace DebugConsole
{
	public class TestDataLoader
	{
		static readonly Random _NumberGen = new Random();
		const string CONNECTION_STRING = "Data Source=vm-bss-dc;Initial Catalog=RemarQ;User Id=_RemarQ;Password=Password!";
		 
		static readonly string[] LISTNAMES = { "One", "Two" };
		static readonly string[] CONTACTLISTNAMES = { "Friends", "Enemies" };
		static readonly string[] TASKLISTNAMES = { "Honey-do", "Construction" };
		static readonly string[] CALENDARLISTNAMES = { "Birthdays", "Events" };
		static readonly string[] DISCUSSIONLISTNAMES = { "Code Monkey", "Arguments about nothing" };
		static readonly string[] SURVAYLISTNAMES = { "Why is there air?" };
		static readonly string[] PICTURELISTNAMES = { "Pretty Pics", "Is it Art" };
		static readonly string[] LIBNAMES = { "Documents One", "Documents Two" };
		static readonly string[] LARGELIST = { "Big List One" };
		static readonly string[] LARGELIB = { "Big Library One" };

		public static void LoadData(string siteUrl, bool deleteOnly = false, bool largeFileSet = false, bool includeUnsupportedListTypes = false)
		{
			CleanUpSite(siteUrl, deleteOnly, largeFileSet, deleteOnly);
		}


		 
 
		public static void DeleteConfig()
		{
			ReadUnreadHttpModule.UnregisterModule(true);
			var checkSettings = Microsoft.SharePoint.Administration.SPFarm.Local.GetObject(FarmSettings.SettingsId);
			if (null != checkSettings)
			{
				checkSettings.Delete();
			}
		}

		public static void InstallConfig()
		{
			FarmSettings checkSettings = new FarmSettings();
			checkSettings.SqlConnectionString = CONNECTION_STRING;
            
			checkSettings.Update(true);
		}

		public static void LoadLargeLists(SPWeb webSite, byte[] smallFile)
		{
			Console.WriteLine("Starting large list population");
			foreach (string listName in LARGELIST)
			{
				SPList deleteMe = webSite.Lists.TryGetList(listName);
				if (deleteMe != null)
				{
					webSite.Lists.Delete(deleteMe.ID);
					webSite.Update();
					Console.WriteLine(listName + "<removed>");
				}
			}
			foreach (string listName in LARGELIB)
			{
				SPList deleteMe = webSite.Lists.TryGetList(listName);
				if (deleteMe != null)
				{
					webSite.Lists.Delete(deleteMe.ID);
					webSite.Update();
					Console.WriteLine(listName + "<removed>");
				}
			}
			foreach (string listName in LARGELIST)
			{
				AddList(webSite, listName, SPListTemplateType.Announcements, SPQuickLaunchHeading.Recent, 10000);
			}
			foreach (string listName in LARGELIB)
			{
				Guid listGuid = webSite.Lists.Add(listName, "Test Library", SPListTemplateType.DocumentLibrary);
				SPList updateMe = webSite.Lists.GetList(listGuid, false);
				SPNavigationNode listNode = webSite.Navigation.GetNodeByUrl(updateMe.DefaultViewUrl);
				if (listNode == null)
				{
					listNode = new SPNavigationNode(updateMe.Title, updateMe.DefaultViewUrl);
					listNode = webSite.Navigation.AddToQuickLaunch(listNode, SPQuickLaunchHeading.Recent);
				}
				SPFolder docFolder = webSite.Folders[listName];
				for (int rootFolderId = 1; rootFolderId < 11; rootFolderId++)
				{
					SPFolder subfolder = docFolder.SubFolders.Add(rootFolderId.ToText());
					subfolder.Update();
					for (int fileId = 1; fileId < 10; fileId++)
					{
						string fileName =   fileId.ToText() + ".txt";
						SPFile uploadedFile = subfolder.Files.Add(fileName, smallFile);
						subfolder.Update();
						Console.Write(".");
					}
                    
					for (int childFolderId = 1; childFolderId < 10; childFolderId++)
					{
						string folderName =  childFolderId.ToText();
						SPFolder childFolder = subfolder.SubFolders.Add(folderName);
						childFolder.Update();
						for (int fileId = 1; fileId < 10; fileId++)
						{
							string fileName =  fileId.ToText() + ".txt";
							SPFile uploadedFile = childFolder.Files.Add(fileName, smallFile);
							childFolder.Update();
							Console.Write(".");
						}
						for (int childChildFolderId = 1; childChildFolderId < 10; childChildFolderId++)
						{
							string childFolderName =  childChildFolderId.ToText();
							SPFolder childChildFolder = childFolder.SubFolders.Add(childFolderName);
							childChildFolder.Update();
							for (int fileId = 1; fileId < 10; fileId++)
							{
								string fileName = fileId.ToText() + ".txt";
								SPFile uploadedFile = childChildFolder.Files.Add(fileName, smallFile);
								childFolder.Update();
								Console.Write(".");
							}
							for (int childChildChildFolderId = 1; childChildChildFolderId < 10; childChildChildFolderId++)
							{
								string childChildFolderName =childChildChildFolderId.ToText();
								SPFolder childChildChildFolder = childChildFolder.SubFolders.Add(childChildFolderName);
								childChildChildFolder.Update();
								for (int fileId = 1; fileId < 10; fileId++)
								{
									string fileName = fileId.ToText() + ".txt";
									SPFile uploadedFile = childChildChildFolder.Files.Add(fileName, smallFile);
									childFolder.Update();
									Console.Write(".");
								}
							}
						}
					}
				}
				Console.WriteLine(listName + "<created>");
			}
		}

		static void CleanUpSite(string siteurl, bool deleteOnly, bool largeFileSet, bool includeUnsupportedListTypes)
		{
			Dictionary<string, byte[]> sampleFiles = GetEmbeddedFiless("DebugConsole.Files.");
			Dictionary<string, byte[]> samplePhotos = GetEmbeddedFiless("DebugConsole.Pictures.");

			List<string> toDelete = new List<string>(LISTNAMES);
			toDelete.AddRange(CONTACTLISTNAMES);
			toDelete.AddRange(CALENDARLISTNAMES);
			toDelete.AddRange(DISCUSSIONLISTNAMES);
			toDelete.AddRange(SURVAYLISTNAMES);
			toDelete.AddRange(PICTURELISTNAMES);
			toDelete.AddRange(LIBNAMES);
			toDelete.AddRange(TASKLISTNAMES);
			toDelete.AddRange(LARGELIST);
			toDelete.AddRange(LARGELIB);

			using (SPSite testSite = new SPSite(siteurl))
			{
				using (SPWeb testWeb = testSite.RootWeb)
				{
					//using (SPWeb subWeb = testWeb.Webs["SW1"])
					//{
					//	foreach (string listName in toDelete)
					//	{
					//		SPList deleteMe = subWeb.Lists.TryGetList(listName);
					//		if (deleteMe != null)
					//		{
					//			subWeb.Lists.Delete(deleteMe.ID);
					//			subWeb.Update();
					//			Console.WriteLine(listName + "<removed>");
					//		}
					//	}

					//	if (!deleteOnly)
					//	{
					//		foreach (string listName in LIBNAMES)
					//		{
					//			AddLibrary(subWeb, listName, 2, sampleFiles, 7);
					//		}
					//	}
					//}

					foreach (string listName in toDelete)
					{
						SPList deleteMe = testWeb.Lists.TryGetList(listName);
						if (deleteMe != null)
						{
							testWeb.Lists.Delete(deleteMe.ID);
							testWeb.Update();
							Console.WriteLine(listName + "<removed>");
						}
					}
					if (deleteOnly)
					{
						return;
					}
					int itemsToAdd = 30;
					foreach (string listName in LISTNAMES)
					{
						AddList(testWeb, listName, SPListTemplateType.Announcements, SPQuickLaunchHeading.Lists, itemsToAdd);
						itemsToAdd *= 5;
					}

					itemsToAdd = 5;
					foreach (string listName in DISCUSSIONLISTNAMES)
					{
						AddList(testWeb, listName, SPListTemplateType.DiscussionBoard, SPQuickLaunchHeading.Discussions, 0);
						PopulateDiscussionBoard(testWeb, listName, itemsToAdd);
						itemsToAdd *= 2;
					}
					int filesToAdd = 3;
					foreach (string listName in LIBNAMES)
					{
						AddLibrary(testWeb, listName, filesToAdd, sampleFiles, 0);
						filesToAdd += 3;
					}

					if (includeUnsupportedListTypes)
					{
						itemsToAdd = 30;
						foreach (string listName in CALENDARLISTNAMES)
						{
							AddList(testWeb, listName, SPListTemplateType.Events, SPQuickLaunchHeading.Lists, itemsToAdd);
							itemsToAdd *= 5;
						}
						itemsToAdd = 30;
						foreach (string listName in TASKLISTNAMES)
						{
							AddList(testWeb, listName, SPListTemplateType.Tasks, SPQuickLaunchHeading.Lists, itemsToAdd);
							itemsToAdd *= 5;
						}

						itemsToAdd = 30;
						List<NameInfo> contactNames = RandomStuff.RandomNames(1000, null, null);
						foreach (string listName in CONTACTLISTNAMES)
						{
							AddList(testWeb, listName, SPListTemplateType.Contacts, SPQuickLaunchHeading.PeopleAndGroups, 0);
							PopulateContacts(testWeb, listName, contactNames, itemsToAdd);
							itemsToAdd *= 5;
						}

						filesToAdd = 5;
						foreach (string listName in PICTURELISTNAMES)
						{
							AddPhotosLibrary(testWeb, listName, filesToAdd, samplePhotos);
							filesToAdd *= 2;
						}
					}
					if (largeFileSet)
					{
						Console.WriteLine("Large data set");
						byte[] scratchPad = sampleFiles["ScratchPad.txt"];
						LoadLargeLists(testWeb, scratchPad);
					}
				}
			}
			Console.Write("Press ENTER to continue....");
			Console.ReadKey();
		}

		internal static void CleanUpReadUnreadDb()
		{
			//these will blow chunks if we dont have permission or 
			//the identity is wrong
			string sqlQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='VIEW'";
			List<string> existingTables = new List<string>();
			using (SqlConnection sqlConn = new SqlConnection(CONNECTION_STRING))
			{
				sqlConn.Open();
				using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn))
				{
					using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							string tableName = sqlReader.GetString(0);
							existingTables.Add(tableName);
						}
					}
				}
				string commandText = "";
				foreach (string tableName in existingTables)
				{
					commandText = "DROP VIEW [dbo].[" + tableName + "]";
					using (SqlCommand sqlCommand = new SqlCommand(commandText, sqlConn))
					{
						sqlCommand.ExecuteNonQuery();
					}
				}
			}
			existingTables.Clear();
			sqlQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'";
			using (SqlConnection sqlConn = new SqlConnection(CONNECTION_STRING))
			{
				sqlConn.Open();
				using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn))
				{
					using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
					{
						while (sqlReader.Read())
						{
							string tableName = sqlReader.GetString(0);
							existingTables.Add(tableName);
						}
					}
				}
				string commandText = "";
				foreach (string tableName in existingTables)
				{
					if (!tableName.Equals(Constants.ReadUnreadResourceTableName))
					{
						commandText = "DROP TABLE [dbo].[" + tableName + "]";
						using (SqlCommand sqlCommand = new SqlCommand(commandText, sqlConn))
						{
							sqlCommand.ExecuteNonQuery();
						}
					}
				}
				commandText = Constants.SqlConfigTableTemplate.Replace("_TABLENAME_", Constants.ReadUnreadConfigurationTableName);
				using (SqlCommand sqlCommand = new SqlCommand(commandText, sqlConn))
				{
					sqlCommand.ExecuteNonQuery();
				}
			}
		}

		static void PopulateDiscussionThread(SPWeb webSite, SPListItem parentThread, int depth)
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
					PopulateDiscussionThread(webSite, replyItem, depth);
				}
			}
		}

		static void PopulateDiscussionBoard(SPWeb webSite, string listName, int numItems)
		{
			SPList addToMe = webSite.Lists.TryGetList(listName);
			for (int numIdx = 0; numIdx < numItems; numIdx++)
			{
				SPListItem newDiscussion = SPUtility.CreateNewDiscussion(addToMe, RandomStuff.RandomSentance(10, 25));
				newDiscussion["Body"] = RandomStuff.RandomSentance(10, 245);
				newDiscussion.Update();
				PopulateDiscussionThread(webSite, newDiscussion, 0);
			}
		}

		static void PopulateContacts(SPWeb webSite, string listName, List<NameInfo> contactNames, int numItems)
		{
			SPList addToMe = webSite.Lists.TryGetList(listName);
			for (int itemNum = 0; itemNum < numItems; itemNum++)
			{
				int contactIdx = itemNum + numItems;
				if (contactIdx < contactNames.Count)
				{
					NameInfo contactName = contactNames[contactIdx];
					SPListItem addMe = addToMe.Items.Add();
					addMe["Title"] = contactName.GivenName;
					addMe["FirstName"] = contactName.SurName;
					addMe["Email"] = contactName.Email;
					string homePhone = RandomStuff.RandomPhoneNumber();
					addMe["HomePhone"] = homePhone;
					addMe["CellPhone"] = RandomStuff.RandomPhoneNumber(homePhone);

					addMe.Update();
					Console.WriteLine(listName + "->" + itemNum.ToText());
				}
			}
		}

		static void AddList(SPWeb webSite, string listName, SPListTemplateType templateType, SPQuickLaunchHeading menuLocation, int numItems)
		{
			Guid listGuid = webSite.Lists.Add(listName, "Test List", templateType);
			SPList updateMe = webSite.Lists.GetList(listGuid, false);
			SPNavigationNode listNode = webSite.Navigation.GetNodeByUrl(updateMe.DefaultViewUrl);
			if (listNode == null)
			{
				// Create the node.
				listNode = new SPNavigationNode(updateMe.Title, updateMe.DefaultViewUrl);

				// Add it to Quick Launch.
				listNode = webSite.Navigation.AddToQuickLaunch(listNode, menuLocation);
			}
			Console.WriteLine(listName + "<created>");
			SPList addToMe = webSite.Lists.TryGetList(listName);
			for (int itemNum = 0; itemNum < numItems; itemNum++)
			{
				SPListItem addMe = addToMe.Items.Add();
				addMe["Title"] = itemNum.ToText();
				if (addMe.Fields.ContainsField("Body"))
				{
					addMe["Body"] = RandomStuff.RandomSentance(10, 245);
				}
				else if (addMe.Fields.ContainsField("Description"))
				{
					addMe["Description"] = RandomStuff.RandomSentance(10, 245);
				}
				if (templateType == SPListTemplateType.Events)
				{
					DateTime startDate = DateTime.Now.AddHours(_NumberGen.Next(0, 100));
					DateTime endDate = startDate.AddMinutes(_NumberGen.Next(15, 200));
					addMe["EventDate"] = startDate.ToShortDateString();
					addMe["EndDate"] = endDate.ToShortDateString();
				}
				else if (templateType == SPListTemplateType.Tasks)
				{
					DateTime startDate = DateTime.Now.AddHours(_NumberGen.Next(0, 100));
					DateTime endDate = startDate.AddDays(_NumberGen.Next(2, 10));
					addMe["StartDate"] = startDate.ToShortDateString();
					addMe["DueDate"] = endDate.ToShortDateString();
					addMe["PercentComplete"] = _NumberGen.Next(0, 99);
				}
				addMe.Update();
				Console.WriteLine(listName + "->" + itemNum.ToText());
			}
		}

		static void AddLibrary(SPWeb webSite, string listName, int numItems, Dictionary<string, byte[]> sampleFiles, int folderDepth)
		{
			Guid listGuid = webSite.Lists.Add(listName, "Test Library", SPListTemplateType.DocumentLibrary);
			SPList updateMe = webSite.Lists.GetList(listGuid, false);
			SPNavigationNode listNode = webSite.Navigation.GetNodeByUrl(updateMe.DefaultViewUrl);
			if (listNode == null)
			{
				// Create the node.
				listNode = new SPNavigationNode(updateMe.Title, updateMe.DefaultViewUrl);

				// Add it to Quick Launch.
				listNode = webSite.Navigation.AddToQuickLaunch(listNode, SPQuickLaunchHeading.Documents);
			}
			Console.WriteLine(listName + "<created>");
			string[] fileNames = sampleFiles.Keys.ToArray();
			SPFolder docFolder = webSite.Folders[listName];
			for (int fileIdx = 1; fileIdx < numItems; fileIdx++)
			{
				if (fileIdx < fileNames.Length)
				{
					string fileName = fileNames[fileIdx];
					SPFile uploadedFile = docFolder.Files.Add(fileName, sampleFiles[fileName]);
					docFolder.Update();
					Console.WriteLine(listName + "->" + fileName);
				}
			}
			int numFolders = _NumberGen.Next(0, 5);

			for (int folderIdx = 0; folderIdx < numFolders; folderIdx++)
			{
				string folderName = "Folder " + folderIdx.ToText();
				AddFolder(docFolder, folderName, numItems, sampleFiles, folderDepth);
			}
		}

		static void AddFolder(SPFolder rootFolder, string folderName, int numItems, Dictionary<string, byte[]> sampleFiles, int folderDepth)
		{
			string fullPath = rootFolder.Url + "\\" + folderName;
			if (fullPath.Length < 255)
			{
				SPFolder subfolder = rootFolder.SubFolders.Add(folderName);
				subfolder.Update();
				string[] fileNames = sampleFiles.Keys.ToArray();
				for (int fileIdx = 1; fileIdx < numItems; fileIdx++)
				{
					if (fileIdx < fileNames.Length)
					{
						string fileName = "(" + subfolder.Item.ID.ToText() + ")" + fileNames[fileIdx];
						SPFile uploadedFile = subfolder.Files.Add(fileName, sampleFiles[fileNames[fileIdx]]);
						subfolder.Update();
						Console.WriteLine(fileName);
					}
				}
				if (folderDepth > 0)
				{
					folderDepth--;
					folderName = "(" + folderDepth.ToString() + ")" + folderName;
					AddFolder(subfolder, folderName, numItems, sampleFiles, folderDepth);
				}
			}
		}

		static void AddPhotosLibrary(SPWeb webSite, string listName, int numItems, Dictionary<string, byte[]> sampleFiles)
		{
			Guid listGuid = webSite.Lists.Add(listName, "Test Photos", SPListTemplateType.PictureLibrary);
			SPList updateMe = webSite.Lists.GetList(listGuid, false);
			SPNavigationNode listNode = webSite.Navigation.GetNodeByUrl(updateMe.DefaultViewUrl);
			if (listNode == null)
			{
				// Create the node.
				listNode = new SPNavigationNode(updateMe.Title, updateMe.DefaultViewUrl);

				// Add it to Quick Launch.
				listNode = webSite.Navigation.AddToQuickLaunch(listNode, SPQuickLaunchHeading.Pictures);
			}
			Console.WriteLine(listName + "<created>");
			string[] fileNames = sampleFiles.Keys.ToArray();
			SPPictureLibrary photoLib = (SPPictureLibrary)webSite.Lists[listName];
			SPFileCollection libPhotos = photoLib.RootFolder.Files;

			SPFolder docFolder = webSite.Folders[listName];
			for (int fileIdx = 0; fileIdx < numItems; fileIdx++)
			{
				if (fileIdx < fileNames.Length)
				{
					string fileName = fileNames[fileIdx];
					SPFile photoFile = libPhotos.Add(fileName, sampleFiles[fileName]);
					photoFile.Item["Description"] = RandomStuff.RandomSentance(10, 50);
					Console.WriteLine(listName + "->" + fileName);
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