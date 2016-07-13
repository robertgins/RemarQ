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

namespace DebugConsole
{
	/// <summary>
	/// this project is used for loading data , testing job service
	/// and generally doing development outside of the sharepoint envionrment
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolver);
			try
			{
				//put stuff for testing here
				//LoadSampleDataToSharePointSite("http://vm-bss-sfs/");
				//List<SPUser> allUsers = Testing.ActiveDirectory.EnsureAllusers("http://vm-bss-sfs/", "BALSAMIC");
				// MarkupLibrary("http://vm-bss-sfs/","Documents two","BALSAMIC");

				 

			}
			catch (Exception badThing)
			{
				Console.WriteLine("******ERROR********");
				Console.WriteLine(badThing.Message);
				Console.WriteLine("******ERROR********");
				Console.WriteLine(badThing.StackTrace);
			}
			Console.WriteLine("press any key...");
			Console.ReadKey();
		}


		/// <summary>
		/// removes the configuration entry
		/// </summary>
		private static void RemoveConfiguration()
		{
			SPPersistedObject checkSettings = Microsoft.SharePoint.Administration.SPFarm.Local.GetObject(FarmSettings.SettingsId);
			if (null != checkSettings)
			{
				checkSettings.Delete();
			}
		}

		/// <summary>
		/// creates a bunch of lists and loads data to the lists for testing
		/// </summary>
		/// <param name="siteUrl"></param>
		private static void LoadSampleDataToSharePointSite(string siteUrl)
		{
			TestDataLoader.LoadData(siteUrl, false, true, false);
		}

		/// <summary>
		/// finds a list and on that list marks all itmes read with the names
		/// of all users in the AD domain (fails if not using ad authentication)
		/// </summary>
		/// <param name="siteUrl"></param>
		/// <param name="listName"></param>
		private static void MarkupLibrary(string siteUrl, string listName, string activeDirectoryNetBiosDomainName)
		{
			///collect all users and ensure them on the site
			List<SPUser> allUsers = Testing.ActiveDirectory.EnsureAllusers(siteUrl, activeDirectoryNetBiosDomainName);
			using (SPSite currentSite = new SPSite(siteUrl))
			{
				using (SPWeb rootWeb = currentSite.RootWeb)
				{
					SPList docsOne = rootWeb.Lists.TryGetList(listName);
					MarkupLibrary(docsOne, allUsers);
				}
			}

		}

		/// <summary>
		/// places one read mark for each user on each item in the list
		/// </summary>
		/// <param name="populateMe"></param>
		/// <param name="sharepointUsers"></param>
		private static void MarkupLibrary(SPList populateMe, List<SPUser> sharepointUsers)
		{
			Console.WriteLine("re-marQing " + populateMe.Title);
			List<int> itemIds = new List<int>();
			foreach (SPListItem listItem in populateMe.Items)
			{
				itemIds.Add(listItem.ID);
			}
			int[] allItemIds = itemIds.ToArray();
			foreach (SPUser sharepointUser in sharepointUsers)
			{
				SqlRemarQ.MarkRead(allItemIds, populateMe.ID, sharepointUser);
			}
		}

		/// <summary>
		/// wire up to resovle to the locla copy instead of the GAC copy, 
		/// just so we can debug  through changes without republishing the solution
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private static Assembly CurrentDomain_AssemblyResolver(object sender, ResolveEventArgs args)
		{
			if (args.Name.StartsWith("BalsamicSolutions.ReadUnreadSiteColumn", StringComparison.OrdinalIgnoreCase))
			{
				string filePath = Path.Combine(Environment.CurrentDirectory, "BalsamicSolutions.ReadUnreadSiteColumn.dll");
				if (File.Exists(filePath))
				{
					return Assembly.LoadFile(filePath);
				}
			}
			return null;
		}
	}
}