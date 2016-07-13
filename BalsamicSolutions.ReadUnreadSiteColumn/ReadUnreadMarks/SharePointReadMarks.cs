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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Server.Utilities;
using Microsoft.SharePoint;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// utility class for processing the SharePoint side of
	/// the read mark calculations. Mostly for folder processing
	/// </summary>
	internal static class SharePointRemarQ
	{
		#region Special view provisioning
		
		// we add to the Threaded (1) view, the Flat (2) view and the Subject (3) view
		//Even in alternate character languages, the view urls we support in a discussion board are AllItems.aspx, Flat.aspx and Threaded.aspx
		static readonly List<string> _DiscussionboardJSLinkViewNames = new List<string>(new string[] { "ALLITEMS.ASPX", "FLAT.ASPX" });
		static readonly List<string> _DiscussionboardXslViewNames = new List<string>(new string[] { "THREADED.ASPX" });
		
		/// <summary>
		/// make sure no views have the ReadUnreadField
		/// </summary>
		/// <param name="discussionBoard"></param>
		static void DeProvisionDiscussionView(SPList discussionBoard)
		{
			List<Guid> viewsToProcess = GetDiscussionBoardJSLinkViews(discussionBoard);
			foreach (Guid viewId in viewsToProcess)
			{
				SPView listView = discussionBoard.Views[viewId];
				try
				{
					if (listView.ViewFields.Exists(Constants.ReadUnreadFieldName))
					{
						listView.ViewFields.Delete(Constants.ReadUnreadFieldName);
					}
					if (null != listView.JSLink)
					{
						//if there was an original its now a multi value JsLink line
						listView.JSLink = RemoveRemarQjsLinks(listView.JSLink);
					}
					//listView.XslLink="thread.xsl";
					listView.Update();
				}
				catch (SPException)
				{
				}
				catch (IndexOutOfRangeException)
				{
				}
			}
			//now do xsl links
			viewsToProcess = GetDiscussionBoardXslLinkViews(discussionBoard);
			foreach (Guid viewId in viewsToProcess)
			{
				SPView listView = discussionBoard.Views[viewId];
				try
				{
					if (listView.ViewFields.Exists(Constants.ReadUnreadFieldName))
					{
						listView.ViewFields.Delete(Constants.ReadUnreadFieldName);
					}
					
					listView.XslLink = "thread.xsl";
					listView.Update();
				}
				catch (SPException)
				{
				}
				catch (IndexOutOfRangeException)
				{
				}
			}
		}
		
		/// <summary>
		/// generate a JsLink Url for a disucssion board view
		/// they are different for different types of views
		/// </summary>
		/// <param name="listView"></param>
		/// <returns></returns>
		static string JsLinkFromView(SPView listView)
		{
			string returnValue = listView.ParentList.ParentWebUrl;
			if (!returnValue.EndsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				returnValue += "/";
			}
			returnValue += Constants.JSLinkServiceUrl;
			bool isDiscussion = listView.ParentList.IsDiscussionBoard();
			string listId = System.Web.HttpUtility.UrlEncode(listView.ParentList.ID.ToString("D"));
			string versionNum = "0";
			string queryString = listId + "\t" + "HTML" + "\t" + versionNum + "\t" + isDiscussion.ToString();
			if (isDiscussion)
			{
				string[] urlParts = listView.Url.Split('/');
				string templateName = urlParts[urlParts.Length - 1].Split('.')[0];
				queryString += "\t" + templateName.ToUpperInvariant();
			}
			string base64 = Convert.ToBase64String(System.Text.UnicodeEncoding.Unicode.GetBytes(queryString));
			returnValue += "?" + base64;
			return returnValue;
		}
		
		/// <summary>
		/// clean up a JSLInk url that we may have munged up
		/// </summary>
		/// <param name="jsLink"></param>
		/// <returns></returns>
		static string RemoveRemarQjsLinks(string jsLink)
		{
			string returnValue = null;
			if (!string.IsNullOrEmpty(jsLink))
			{
				string[] jsLinks = jsLink.Split('|');
				if (jsLinks.Length > 0)
				{
					List<string> newLinks = new List<string>();
					for (int linkIdx = 0; linkIdx < jsLinks.Length; linkIdx++)
					{
						if (!jsLinks[linkIdx].CaseInsensitiveContains(Constants.JSLinkServiceUrl) &&
							!jsLinks[linkIdx].CaseInsensitiveContains(Constants.ReadUnreadJavaScriptPath))
						{
							newLinks.Add(jsLinks[linkIdx]);
						}
					}
					if (newLinks.Count > 0)
					{
						returnValue = string.Join("|", newLinks);
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// check the views and install a remarq field  in
		/// position 1 for each of the disucssion board special 
		/// views
		/// </summary>
		/// <param name="discussionBoard"></param>
		/// <returns></returns>
		static void ProvisionDiscussionViews(SPList discussionBoard)
		{
			ReadUnreadField viewField = Utilities.FindFirstFieldOnList(discussionBoard);
			if (null != viewField)
			{
				List<Guid> viewsToProcess = GetDiscussionBoardJSLinkViews(discussionBoard);
				foreach (Guid viewId in viewsToProcess)
				{
					SPView listView = discussionBoard.Views[viewId];
					if (!listView.ViewFields.Exists(viewField.InternalName))
					{
						listView.ViewFields.Add(viewField);
					}
					if (string.IsNullOrWhiteSpace(listView.JSLink))
					{
						listView.JSLink = JsLinkFromView(listView);
					}
					else
					{
						string newJsLink = RemoveRemarQjsLinks(listView.JSLink);
						if (null == newJsLink)
						{
							newJsLink = JsLinkFromView(listView);
						}
						else
						{
							listView.JSLink = newJsLink + "|" + JsLinkFromView(listView);
						}
					}
					listView.Update();
				}
				//now deal with xsl
				viewsToProcess = GetDiscussionBoardXslLinkViews(discussionBoard);
				foreach (Guid viewId in viewsToProcess)
				{
					SPView listView = discussionBoard.Views[viewId];
					//The xsl cant handle the field existing in the view
					//which is ok because we do all the work anyway
					if (listView.ViewFields.Exists(Constants.ReadUnreadFieldName))
					{
						listView.ViewFields.Delete(Constants.ReadUnreadFieldName);
					}
					listView.XslLink = "RemarQThread.xsl";
					listView.Update();
				}
			}
		}
		
		/// <summary>
		/// collects the Guids of the "JSLink supported views" in a discussion board
		/// </summary>
		/// <param name="discussionBoard"></param>
		/// <returns></returns>
		static List<Guid> GetDiscussionBoardJSLinkViews(SPList discussionBoard)
		{
			List<Guid> returnValue = new List<Guid>();
			foreach (SPView listView in discussionBoard.Views)
			{
				// we add to the Threaded (1) view, the Flat (2) view and the Subject (3) view
				foreach (string viewName in _DiscussionboardJSLinkViewNames)
				{
					if (listView.Url.EndsWith(viewName, StringComparison.OrdinalIgnoreCase))
					{
						returnValue.Add(listView.ID);
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// collects the Guids of the "Xsl supported views" in a discussion board
		/// </summary>
		/// <param name="discussionBoard"></param>
		/// <returns></returns>
		static List<Guid> GetDiscussionBoardXslLinkViews(SPList discussionBoard)
		{
			List<Guid> returnValue = new List<Guid>();
			foreach (SPView listView in discussionBoard.Views)
			{
				// we add to the Threaded (1) view, the Flat (2) view and the Subject (3) view
				foreach (string viewName in _DiscussionboardXslViewNames)
				{
					if (listView.Url.EndsWith(viewName, StringComparison.OrdinalIgnoreCase))
					{
						returnValue.Add(listView.ID);
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// check the views and make sure a remarq field is in
		/// them, in position 1 for each of the disucssion board special 
		/// views
		/// </summary>
		/// <param name="discussionBoard"></param>
		/// <returns></returns>
		static bool AreAllDiscussionViewsInstalled(SPList discussionBoard)
		{
			foreach (SPView listView in discussionBoard.Views)
			{
				foreach (string viewName in _DiscussionboardJSLinkViewNames)
				{
					if (listView.Url.EndsWith(viewName, StringComparison.OrdinalIgnoreCase))
					{
						if (string.IsNullOrEmpty(listView.JSLink))
						{
							return false;
						}
						if (!listView.JSLink.CaseInsensitiveContains(Constants.JSLinkServiceUrl))
						{
							return false;
						}
					}
				}
				foreach (string viewName in _DiscussionboardXslViewNames)
				{
					if (listView.Url.EndsWith(viewName, StringComparison.OrdinalIgnoreCase))
					{
						if (!listView.XslLink.CaseInsensitiveContains("RemarQThread.xsl"))
						{
							return false;
						}
					}
					//make sure our field is not actually in the view fields
					//list otherwise the Xsl is not going to be right
					if (listView.ViewFields.Exists(Constants.ReadUnreadFieldName))
					{
						return false;
					}
				}
			}
			return true;
		}
		
		/// <summary>
		/// Place Holder for future feature
		/// </summary>
		/// <param name="discussionBoard"></param>
		/// <returns></returns>
		static bool AreAllUnreadViewsInstalled(SPList discussionBoard)
		{
			return true;
		}
		
		/// <summary>
		/// Place Holder for future feature
		/// </summary>
		/// <param name="spList"></param>
		static void ProvisionUnreadItemsView(SPList spList)
		{
		}
		
		/// <summary>
		/// Place Holder for future feature
		/// </summary>
		/// <param name="spList"></param>
		static void DeProvisionUnreadItemsView(SPList spList)
		{
		}
		
		/// <summary>
		/// Creates any specialized views this lsit may need
		/// </summary>
		/// <param name="spList"></param>
		internal static void ProvisionViews(SPList spList)
		{
			if (spList.IsDiscussionBoard())
			{
				ProvisionDiscussionViews(spList);
			}
			else
			{
				ProvisionUnreadItemsView(spList);
			}
		}
		
		/// <summary>
		/// Removes specials views created for this list
		/// </summary>
		/// <param name="spList"></param>
		internal static void DeProvisionViews(SPList spList)
		{
			if (spList.IsDiscussionBoard())
			{
				DeProvisionDiscussionView(spList);
			}
			else
			{
				DeProvisionUnreadItemsView(spList);
			}
		}
		
		internal static bool AreAllViewsInstalled(SPList spList)
		{
			if (spList.IsDiscussionBoard())
			{
				return AreAllDiscussionViewsInstalled(spList);
			}
			else
			{
				return AreAllUnreadViewsInstalled(spList);
			}
		}
		
		internal static void VerifyViews(SPList spList)
		{
			if (!AreAllViewsInstalled(spList))
			{
				DeProvisionViews(spList);
				try
				{
					ProvisionViews(spList);
				}
				catch (SPException sharePointError)
				{
					RemarQLog.LogWarning("An error occurred updateing the views for " + spList.Title + " The Error was " + sharePointError.Message);
				}
			}
		}
		
		#endregion
		
		#region Item Provisioning
		
		/// <summary>
		/// Update the item path in our Hierarchy reference
		/// </summary>
		/// <param name="listItem"></param>
		internal static void UpdateItemPath(SPListItem listItem)
		{
			if (null == listItem)
			{
				throw new ArgumentNullException("listItem");
			}
			
			SqlRemarQ.UpdateItemPath(listItem);
		}
		
		/// <summary>
		/// remove  the item from read marks and the hierarchy reference
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="listId"></param>
		internal static void RemoveAllItemReferences(int itemId, Guid listId)
		{
			SqlRemarQ.RemoveAllItemReferences(itemId, listId);
		}
		
		#endregion
		
		#region Marking activities
		
		/// <summary>
		/// reset all the read marks for a specific item, usually
		/// only called from the event handler when the item is updated
		/// </summary>
		/// <param name="listItem"></param>
		internal static void ResetReadMarks(SPListItem listItem)
		{
			if (null != listItem)
			{
				RemarQuery ruQuery = new RemarQuery(listItem);
				HashSet<int> itemsToUnMark = new HashSet<int>();
				itemsToUnMark.Add(listItem.ID);
				//We always unmark the parent threads when child item is unread 
				itemsToUnMark.UnionWith(ruQuery.ParentFolderIds());
				if (listItem.IsFolder()) 
				{
					//If its a  folder we also mark all child elements 
					itemsToUnMark.UnionWith(ruQuery.ChildItemsAndFolders());
				}
				SqlRemarQ.ResetReadMarks(  itemsToUnMark.ToArray(), listItem.ParentList.ID);
			}
		}
		
		/// <summary>
		/// Mark an item unread, if the item is a folder mark
		/// all its contents unread, if the item is in a folder
		/// check and see if all other peer items are unread, and if so
		/// then also mark the parent item unread
		/// </summary>
		/// <param name="listItem"></param>
		/// <param name="userId"></param>
		internal static void MarkUnRead(SPListItem listItem, SPUser spUser)
		{
			if (null != listItem && null != spUser)
			{
				RemarQuery ruQuery = new RemarQuery(listItem);
				HashSet<int> itemsToUnMark = new HashSet<int>();
				itemsToUnMark.Add(listItem.ID);
				//We always unmark the parent threads when child item is unread 
				itemsToUnMark.UnionWith(ruQuery.ParentFolderIds());
				if (listItem.IsFolder()) 
				{
					//If its a  folder we also mark all child elements 
					itemsToUnMark.UnionWith(ruQuery.ChildItemsAndFolders());
				}
				
				SqlRemarQ.MarkUnRead(itemsToUnMark.ToArray(), listItem.ParentList.ID, spUser);
			}
		}
		
		/// <summary>
		/// read marks from the event handler, specifically handled so we 
		/// dont have to lookup an SPUser object
		/// </summary>
		/// <param name="eventProperties"></param>
		internal static void MarkRead(SPItemEventProperties eventProperties)
		{
			//properties.ListItem, properties.UserLoginName
			if (null != eventProperties && null != eventProperties.ListItem && !string.IsNullOrEmpty(eventProperties.UserLoginName))
			{
				HashSet<int> itemsToMark = new HashSet<int>();
				itemsToMark.Add(eventProperties.ListItem.ID);
				RemarQuery ruQuery = new RemarQuery(eventProperties.ListItem);
				if (eventProperties.ListItem.IsFolder())
				{
					//its a folder or a thread so collect all child items
					itemsToMark.UnionWith(ruQuery.ChildItemsAndFolders());
				}
				//collect the item and all of its children objects
				HierarchyItem[] parentFolders = ruQuery.ParentFolders.ToArray();
				if (parentFolders.Length > 0)
				{
					int ignoreHierarchyItemId = eventProperties.ListItem.ID;
					for (int pathIdx = parentFolders.Length - 1; pathIdx >= 0; pathIdx--)
					{
						//we are walking "up the tree" to mark the parent folder read
						//but we can only do that if we are the last item in the folder
						//that was unread, so we have to check all peer items at each level
						//and if they are all read, then we can move up and mark the parent folder
						//If the parent folder is the root folder then we also mark it
						HierarchyItem peerContainer = parentFolders[pathIdx];
						string queryPath = CombinePath(peerContainer.Path, peerContainer.Leaf);
						if ((ruQuery.AreAllPeerItemsRead(eventProperties, queryPath, ignoreHierarchyItemId)))
						{
							itemsToMark.Add(peerContainer.ItemId);
							//in the next query up the chain, ignore this container
							//because its not marked yet
							ignoreHierarchyItemId = peerContainer.ItemId;
						}
						else
						{
							break;
						}
					}
				}
				SqlRemarQ.MarkRead(itemsToMark.ToArray(), eventProperties);
			}
		}
		
		/// <summary>
		/// mark the item read, if the item is a folder mark all of its contents read
		/// if the item is in a folder check all the other peer items and if they 
		/// are read mark the parent folder read, 
		/// </summary>
		/// <param name="listItem"></param>
		/// <param name="userId"></param>
		internal static void MarkRead(SPListItem listItem, SPUser spUser)
		{
			if (null != listItem && null != spUser)
			{
				//now collect any related items this would impact
				HashSet<int> itemsToMark = new HashSet<int>();
				itemsToMark.Add(listItem.ID);
				RemarQuery ruQuery = new RemarQuery(listItem);
				if (listItem.IsFolder())
				{
					//its a folder or a thread so collect all child items
					itemsToMark.UnionWith(ruQuery.ChildItemsAndFolders());
				}
				//collect the item and all of its children objects
				HierarchyItem[] parentFolders = ruQuery.ParentFolders.ToArray();
				if (parentFolders.Length > 0)
				{
					int ignoreHierarchyItemId = listItem.ID;
					for (int pathIdx = parentFolders.Length - 1; pathIdx >= 0; pathIdx--)
					{
						//we are walking "up the tree" to mark the parent folder read
						//but we can only do that if we are the last item in the folder
						//that was unread, so we have to check all peer items at each level
						//and if they are all read, then we can move up and mark the parent folder
						//If the parent folder is the root folder then we also mark it
						HierarchyItem peerContainer = parentFolders[pathIdx];
						string queryPath = CombinePath(peerContainer.Path, peerContainer.Leaf);
						if ((ruQuery.AreAllPeerItemsRead(spUser, queryPath, ignoreHierarchyItemId)))
						{
							itemsToMark.Add(peerContainer.ItemId);
							//in the next query up the chain, ignore this container
							//because its not marked yet
							ignoreHierarchyItemId = peerContainer.ItemId;
						}
						else
						{
							break;
						}
					}
				}
				SqlRemarQ.MarkRead(itemsToMark.ToArray(), listItem.ParentList.ID, spUser);
			}
		}
		
		/// <summary>
		/// combine path strings for folders and files
		/// or for discission boards
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="suffix"></param>
		/// <returns></returns>
		static string CombinePath(string prefix, string suffix)
		{
			string returnValue = prefix;
			if (!prefix.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				//no / in discussion path
				if (!prefix.EndsWith("/", StringComparison.OrdinalIgnoreCase))
				{
					returnValue += "/";
				}
			}
			if (prefix.Equals("/") && suffix.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				//root of discussion board
				returnValue = suffix;
			}
			else
			{
				returnValue += suffix;
			}
			return returnValue;
		}
		
		#endregion
		
		#region Event handler checks
		
		/// <summary>
		/// set the flag that tells the event handler and the UI
		/// that Provisioning is running
		/// </summary>
		/// <param name="thisList"></param>
		internal static void SetProvisioningFlag(Guid siteId, Guid webId, Guid listId)
		{
			string keyName = listId.ToString() + Constants.ReadUnreadFieldId.ToString();
			using (SPSite spSite = new SPSite(siteId))
			{
				using (SPWeb spWeb = spSite.OpenWeb(webId))
				{
					object existingProp = spWeb.GetProperty(keyName);
					if (null == existingProp)
					{
						spWeb.AddProperty(keyName, "Provisioning");
						spWeb.Update();
					}
				}
			}
		}
		
		/// <summary>
		/// set the flag that tells the event handler and the UI
		/// that Provisioning is not running
		/// </summary>
		/// <param name="thisList"></param>
		internal static void ClearProvisioningFlag(Guid siteId, Guid webId, Guid listId)
		{
			string keyName = listId.ToString() + Constants.ReadUnreadFieldId.ToString();
			using (SPSite spSite = new SPSite(siteId))
			{
				using (SPWeb spWeb = spSite.OpenWeb(webId))
				{
					object existingProp = spWeb.GetProperty(keyName);
					if (null != existingProp)
					{
						spWeb.DeleteProperty(keyName);
						spWeb.Update();
					}
				}
			}
		}
		
		/// <summary>
		/// check to see if provisioning is Running
		/// </summary>
		/// <param name="thisList"></param>
		/// <returns></returns>
		internal static bool ProvisiongFlagIsSet(Guid siteId, Guid webId, Guid listId)
		{
			string keyName = listId.ToString() + Constants.ReadUnreadFieldId.ToString();
			using (SPSite spSite = new SPSite(siteId))
			{
				using (SPWeb spWeb = spSite.OpenWeb(webId))
				{
					object existingProp = spWeb.GetProperty(keyName);
					return (existingProp != null);
				}
			}
		}
		
		/// <summary>
		/// set the flag that tells the event handler and the UI
		/// that Provisioning is running
		/// </summary>
		/// <param name="thisList"></param>
		internal static void SetProvisioningFlag(SPList thisList)
		{
			string keyName = thisList.ID.ToString() + Constants.ReadUnreadFieldId.ToString();
			object existingProp = thisList.ParentWeb.GetProperty(keyName);
			if (null == existingProp)
			{
				thisList.ParentWeb.AddProperty(keyName, "Provisioning");
				thisList.ParentWeb.Update();
			}
		}
		
		/// <summary>
		/// set the flag that tells the event handler and the UI
		/// that Provisioning is not running
		/// </summary>
		/// <param name="thisList"></param>
		internal static void ClearProvisioningFlag(SPList thisList)
		{
			string keyName = thisList.ID.ToString() + Constants.ReadUnreadFieldId.ToString();
			object existingProp = thisList.ParentWeb.GetProperty(keyName);
			if (null != existingProp)
			{
				thisList.ParentWeb.DeleteProperty(keyName);
				thisList.ParentWeb.Update();
			}
		}
		
		/// <summary>
		/// check to see if provisioning is Running
		/// </summary>
		/// <param name="thisList"></param>
		/// <returns></returns>
		internal static bool ProvisiongFlagIsSet(SPList thisList)
		{
			//if update is disabled this should be a string
			string keyName = thisList.ID.ToString() + Constants.ReadUnreadFieldId.ToString();
			object existingProp = thisList.ParentWeb.GetProperty(keyName);
			return (existingProp != null);
		}
		
		#endregion
		
		#region Report Permission check
		
		/// <summary>
		/// reports can be run by site administrators, web administrators
		/// list administrators, web site owners and document authors
		/// </summary>
		/// <param name="spUser"></param>
		/// <param name="listItem"></param>
		/// <param name="spList"></param>
		/// <param name="spWeb"></param>
		/// <returns></returns>
		internal static bool HasPermissionToRunReport(SPUser spUser, SPListItem listItem, SPList spList, SPWeb spWeb)
		{
			//check the owners role
			SPRoleDefinition adminRole = spWeb.RoleDefinitions.GetByType(SPRoleType.Administrator);
			if (spList.DoesUserHavePermissions(spUser, adminRole.BasePermissions))
			{
				return true;
			}
			//check list specific permissions
			if (spList.DoesUserHavePermissions(spUser, SPBasePermissions.FullMask))
			{
				return true;
			}
			//check item specific permissions
			if (listItem.DoesUserHavePermissions(spUser, SPBasePermissions.FullMask))
			{
				return true;
			}
			//check author field
			SPFieldUserValue userValue = new SPFieldUserValue(spWeb, listItem[SPBuiltInFieldId.Author].ToString());
			if (userValue.User.ID == spUser.ID)
			{
				return true;
			}
			
			return false;
		}
		
		#endregion
	}
}