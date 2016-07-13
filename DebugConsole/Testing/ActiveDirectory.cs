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
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration.Claims;

namespace DebugConsole.Testing
{
	public class ActiveDirectory
	{
		public static string[] GetAllUsers(string domainPrefix)
		{
			if (!string.IsNullOrEmpty(domainPrefix))
			{
				domainPrefix = domainPrefix.ToUpperInvariant().Trim();
			}
			HashSet<string> returnValue = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			string baseDn = string.Empty;
			using (DirectoryEntry tmpde = new DirectoryEntry("LDAP://RootDSE"))
			{
				baseDn = tmpde.Properties["defaultNamingContext"][0].ToString();
			}
			string ldapPath = "LDAP://OU=RemarQ Test Users," + baseDn;
			using (DirectoryEntry rootOu = new DirectoryEntry(ldapPath))
			{
				DirectorySearcher userSearch = new DirectorySearcher(rootOu);
				userSearch.Filter = "objectClass=user";
				userSearch.PropertiesToLoad.Add("sAMAccountName");
				foreach (SearchResult foundUser in userSearch.FindAll())
				{
					if (foundUser.Properties.Contains("sAMAccountName"))
					{
						string samName = (string)foundUser.Properties["sAMAccountName"][0];
						//exclude service accounts
						if (!samName.StartsWith("_"))
						{
							if (!string.IsNullOrEmpty(domainPrefix))
							{
								samName = domainPrefix + "\\" + samName;
							}
							returnValue.Add(samName);
						}
					}
				}
			}

			return returnValue.ToArray();
		}

		public static string[] GetAllUsersClaimNames(string domainPrefix)
		{
			SPClaimProviderManager cpm = SPClaimProviderManager.Local;
			string[] allUsers = GetAllUsers(domainPrefix);
			for (int i = 0; i < allUsers.Length; i++)
			{
				allUsers[i] = cpm.ConvertIdentifierToClaim(allUsers[i], SPIdentifierTypes.WindowsSamAccountName).ToEncodedString();
			}
			return allUsers;
		}

		public static List<SPUser> EnsureAllusers(string rootUrl, string domainPrefix)
		{
			string[] allUsers = GetAllUsersClaimNames(domainPrefix);
			List<SPUser> returnValue = new List<SPUser>();
			using (SPSite currentSite = new SPSite(rootUrl))
			{
				using (SPWeb currentWeb = currentSite.RootWeb)
				{
					foreach (string userClaim in allUsers)
					{
						SPUser oneUser = currentWeb.EnsureUser(userClaim);
						Console.WriteLine(oneUser.Name); 
						returnValue.Add(oneUser);
					}
				}
			}
			return returnValue;
		}

		public static void EnsureAllusers(SPWeb currentWeb,string[] allUsers)
		{
			foreach (string userClaim in allUsers)
			{
				var user = currentWeb.EnsureUser(userClaim);
				Console.WriteLine(user.Name); 
			}
		}
	}
}