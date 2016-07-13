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
		static void Disable()
		{
			try
			{
				//collect all installed fields
				List<ListConfiguration> allFields = SqlRemarQ.GetListConfigurations();
				Console.WriteLine(string.Format("{0} Fields to remove ", allFields.Count));
				if (allFields.Count > 0)
				{
					SharePointListDictionary listDict = new SharePointListDictionary();
					foreach (ListConfiguration listConfig in allFields)
					{
						listDict.Add(listConfig.SiteId, listConfig.WebId, listConfig.ListId);
					}
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
													SPList fixMe = listWeb.Lists[listId];
													Console.WriteLine("Un-RemarQ-ing " + fixMe.Title);
													List<ReadUnreadField> fieldsToRemove = BalsamicSolutions.ReadUnreadSiteColumn.Utilities.FindFieldsOnList(fixMe);
													foreach (ReadUnreadField removeMe in fieldsToRemove)
													{
														removeMe.Delete();
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
						}
						catch (FileNotFoundException)
						{
							Console.WriteLine("A missing site has been removed for " + siteId.ToString("B"));
						}
					}
					Console.WriteLine("Processing cleanup");
				}
			}
			finally
			{
				EndProgress();
			}
			RunJobs();
			foreach (ListConfiguration listConfig in SqlRemarQ.GetListConfigurations())
			{
				SqlRemarQ.DeProvisionReadUnreadTableAndRemoveConfigurationEntry(listConfig.ListId);
			}
		}
	}
}