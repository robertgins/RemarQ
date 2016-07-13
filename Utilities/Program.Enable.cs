// -----------------------------------------------------------------------------
//  Copyright 7/9/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
 
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BalsamicSolutions.ReadUnreadSiteColumn;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Utilities
{
	partial class Program
	{
		static void Activate()
		{
			Guid featureGuid = new Guid("830c7e22-415b-4efb-bee3-407fa24faeb1");
			try
			{
				Console.WriteLine("Collecting farm topology");
				StartProgress();
				//collect all lists of our license type

				SPWebService webSvc = SPFarm.Local.Services.GetValue<SPWebService>(string.Empty);
				EndProgress();
				foreach (SPWebApplication webApp in webSvc.WebApplications)
				{
					foreach (SPSite webSite in webApp.Sites)
					{
						if (!RemarQIsActive(webSite))
						{
							if (Confirm("Do you want to activate RemarQ on " + webSite.Url.ToString()))
							{
								webSite.RootWeb.Features.Add(featureGuid);
								webSite.RootWeb.Update();
							}
						}
						webSite.Dispose();
					}
				}
				EndProgress();
				Console.WriteLine("All SharePoint sites have been scanned and activated as requested");
			}
			finally
			{
				EndProgress();
			}
		}

		static void Enable()
		{
			try
			{
				Console.WriteLine("Collecting farm topology and qualifiying lists");
				StartProgress();
				//collect all lists of our license type
				SharePointListDictionary allListsInFarm = new SharePointListDictionary();
				SPWebService webSvc = SPFarm.Local.Services.GetValue<SPWebService>(string.Empty);
				foreach (SPWebApplication webApp in webSvc.WebApplications)
				{
					foreach (SPSite webSite in webApp.Sites)
					{
						EndProgress();
						if (!RemarQIsActive(webSite))
						{
							Console.WriteLine("The RemarQ feature is not activated for site " + webSite.Url.ToString());
							StartProgress();
						}
						else
						{
							if (Confirm(string.Format("Process site {0}", webSite.Url)))
							{
								StartProgress();
								foreach (SPWeb siteWeb in webSite.AllWebs)
								{
									foreach (SPList webList in siteWeb.Lists)
									{
										if (!webList.Hidden)
										{
											if (ReadUnreadField.IsSupportedListType(webList))
											{
												string folderPath = webList.RootFolderPath();
												if (!folderPath.CaseInsensitiveContains("SiteAssets") &&
													!folderPath.CaseInsensitiveContains("SitePages") &&
													!folderPath.CaseInsensitiveContains("Style Library"))
												{
													allListsInFarm.Add(webSite.ID, siteWeb.ID, webList.ID);
													Console.Write("+");
												}
											}
										}
									}
									siteWeb.Dispose();
								}
							}
							else
							{
								StartProgress();
							}
						}
						webSite.Dispose();
					}
				}
				EndProgress();
				Console.WriteLine(string.Format("{0} Lists to enable", allListsInFarm.Count));
				if (allListsInFarm.Count > 0)
				{
					Enable(allListsInFarm);
				}
			}
			finally
			{
				EndProgress();
			}
		}

		static void Enable(SharePointListDictionary listDict)
		{
			int largeListCount = 0;
			int listCount = 0;
			foreach (Guid siteId in listDict.SiteIds())
			{
				try
				{
					using (SPSite listSite = new SPSite(siteId))
					{
						Console.WriteLine("Processing site " + listSite.Url.ToString());
						foreach (Guid webId in listDict.WebIds(siteId))
						{
							try
							{
								using (SPWeb listWeb = listSite.AllWebs[webId])
								{
									Console.WriteLine("Processing web " + listWeb.Title);
									foreach (Guid listId in listDict.ListIds(siteId, webId))
									{
										try
										{
											SPList remarQMe = listWeb.Lists[listId];
											if (ReadUnreadField.IsSupportedListType(remarQMe))
											{
												var existingField = BalsamicSolutions.ReadUnreadSiteColumn.Utilities.FindFirstFieldOnList(remarQMe);
												if (null == existingField)
												{
													if (remarQMe.ItemCount > FarmSettings.Settings.MinSizeOfLargeList)
													{
														largeListCount++;
													}
													else
													{
														listCount++;
													}

													Console.WriteLine("RemarQ-ing " + remarQMe.Title);
													SPField remarQField = listSite.RootWeb.AvailableFields["RemarQ"];
													string fieldName = remarQMe.Fields.Add(remarQField);
													remarQMe.Update();
													if(!remarQMe.IsDiscussionBoard())
													{
														SPView defaultView = remarQMe.DefaultView;
														defaultView.ViewFields.Add(remarQMe.Fields["RemarQ"]);
														defaultView.Update();
													}
												}
											}
										}
										catch (ArgumentException)
										{
											Console.WriteLine("A missing list has been removed for " + listId.ToString("B"));
										}
									}
								}
							}
							catch (ArgumentException)
							{
								Console.WriteLine("A missing web has been removed for " + webId.ToString("B"));
							}
						}
					}
					Console.WriteLine(string.Format("Enabled {0} lists and {1} large lists", listCount, largeListCount));
				}
				catch (FileNotFoundException)
				{
					Console.WriteLine("A missing site has been removed for " + siteId.ToString("B"));
				}
			}
			RunJobs();
		}
	}
}