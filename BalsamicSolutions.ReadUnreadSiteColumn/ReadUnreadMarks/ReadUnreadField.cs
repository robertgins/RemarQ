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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.WebControls;

///ignore ASP.NET warning for SharePoint mobile field control
#pragma warning disable 0672,0618

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// the implementation of our field
	/// </summary>
	public class ReadUnreadField : SPFieldText
	{
		#region Fields & Declarations
		
		string _JSLink = string.Empty;
		bool _JSLinkEvaluated = false;
		bool _BypassVerify = false;
		
		readonly object _LockProxy = new object();
		RangedReadMarksQuery _ThisUsersReadMarks = null;
		
		bool _SupportedListType = false;
		bool _SupportedListEvaluated = false;
		bool _IsDiscussionBoard = false;
		bool _IsDiscussionBoardEvaluated = false;
		bool _IsProvisioned = false;
		bool _IsProvisionedEvaluated = false;
		readonly bool _ConfigurationIsOk = FarmSettings.Settings.IsOk;
		
		#endregion
		
		#region CTORs
		
		//this constructor is called with the public name of the field
		public ReadUnreadField(SPFieldCollection fields, string fieldName)
			: base(fields, fieldName)
		{
		}
		
		//new field constructor we need to initialize the display name
		public ReadUnreadField(SPFieldCollection fields, string typeName, string displayName)
			: base(fields, typeName, displayName)
		{
		}
		
		#endregion
		
		#region SPField implementation
		
		public override Type FieldValueType
		{
			get { return typeof(ReadUnreadFieldValue); }
		}
		
		public override BaseFieldControl FieldRenderingControl
		{
			get
			{
				BaseFieldControl fieldControl = new ReadUnreadFieldControl(this);
				fieldControl.FieldName = this.InternalName;
				return fieldControl;
			}
		}
		
		public override Microsoft.SharePoint.MobileControls.SPMobileBaseFieldControl FieldRenderingMobileControl
		{
			get
			{
				Microsoft.SharePoint.MobileControls.SPMobileBaseFieldControl mobileControl = new ReadUnreadMobileFieldControl(this);
				mobileControl.FieldName = this.InternalName;
				return mobileControl;
			}
		}
		
		public override string GetValidatedString(object value)
		{
			if (null == value)
			{
				throw new ArgumentNullException("value");
			}
			return value.ToString();
		}
		
		public override object GetFieldValue(string value)
		{
			if (!this.ConfigurationIsOk || !this.ParentListIsSupportedListType)
			{
				return new ReadUnreadFieldValue();
			}
			return new ReadUnreadFieldValue(value);
		}
		
		public override string GetFieldValueAsText(object value)
		{
			if (!this.ConfigurationIsOk || !this.ParentListIsSupportedListType)
			{
				return Constants.TextError;
			}
			
			if (this.IsAnonymous)
			{
				return Constants.TextAnonymous;
			}
			
			if (null == this.CurrentUser)
			{
				return Constants.TextAnonymous;
			}
			ReadUnreadFieldValue fieldValue = GetFieldValueFromRowData(value);
			if (null == fieldValue)
			{
				return Constants.TextInitializing;
			}
			if (!fieldValue.IsValid)
			{
				return Constants.TextError;
			}
			if (this.IsRead(fieldValue.ItemId))
			{
				return Constants.TextRead;
			}
			return Constants.TextUnread;
		}
		
		protected override bool HasValue(object value)
		{
			if (!this.ConfigurationIsOk || !this.ParentListIsSupportedListType)
			{
				return false;
			}
			ReadUnreadFieldValue fieldValue = GetFieldValueFromRowData(value);
			if (null == fieldValue)
			{
				return false;
			}
			return fieldValue.IsValid;
		}
		
		public override string GetFieldValueAsHtml(object value)
		{
			if (!this.ConfigurationIsOk)
			{
				return string.Format(CultureInfo.InvariantCulture, Constants.ConfigErrorImageTagTemplate, Framework.ResourceManager.GetString(this.ParentList.ParentWeb.Locale, "ConfigErrorImageTag"));
			}
			
			if (!this.ParentListIsSupportedListType)
			{
				return string.Format(CultureInfo.InvariantCulture, Constants.WarningImageTagTemplate, Framework.ResourceManager.GetString(this.ParentList.ParentWeb.Locale, "WarningImageTagAltText"));
			}
			
			if (this.IsAnonymous)
			{
				return string.Format(CultureInfo.InvariantCulture, Constants.AnonymousImageTag, string.Empty); 
			}
			
			ReadUnreadFieldValue fieldValue = GetFieldValueFromRowData(value);
			if (!this.IsProvisioned)
			{
				return string.Format(CultureInfo.InvariantCulture, Constants.LoadingImageTagTemplate, Framework.ResourceManager.GetString(this.ParentList.ParentWeb.Locale, "LoadingImageTagAltText"));
			}
			ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(this.ParentList.ID);
			//if there is no configuration we are  seriously broken
			if (null == listConfig)
			{
				return string.Format(CultureInfo.InvariantCulture, Constants.ErrorImageTagTemplate, Framework.ResourceManager.GetString(this.ParentList.ParentWeb.Locale, "ErrorImageTagAltText"));
			}
			
			//if this field is still initializing than send the loading tag
			if (null == fieldValue)
			{
				return string.Format(CultureInfo.InvariantCulture, Constants.LoadingImageTagTemplate, Framework.ResourceManager.GetString(this.ParentList.ParentWeb.Locale, "LoadingImageTagAltText"));
			}
			
			//this gets called for each field to write to the CSR client
			//so we cache the last list query
			string returnValue = string.Format(CultureInfo.InvariantCulture, Constants.ErrorImageTagTemplate, Framework.ResourceManager.GetString(this.ParentList.ParentWeb.Locale, "ErrorImageTagAltText"));
			if (fieldValue.IsValid && FarmSettings.Settings.IsOk)
			{
				if (null != listConfig)
				{
					//anonymous users get a blank tag and no markup
					returnValue = this.IsDiscussionBoard ? Constants.EmptyDivTag : Constants.AnonymousImageTag;
					if (null != this.CurrentUser)
					{
						returnValue = this.RenderFieldAsDivTag(listConfig, fieldValue, !this.IsDiscussionBoard);
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// emit the html for a field on a document library or 
		/// a standard list type
		/// </summary>
		/// <param name="listConfig"></param>
		/// <param name="fieldValue"></param>
		/// <returns></returns>
		string RenderFieldAsDivTag(ListConfiguration listConfig, ReadUnreadFieldValue fieldValue, bool includeDisplay)
		{
			string listId = fieldValue.ListId.ToString("N");
			string itemId = fieldValue.ItemId.ToString(CultureInfo.InvariantCulture);
			string baseName = listId + "_" + itemId;
			string rowId = "rum_" + baseName;
			string isUnread = "0";
			string isFolder = fieldValue.IsFolder ? "1" : "0";
			string displayTag = string.Empty;
			if (this.IsRead(fieldValue.ItemId))
			{
				isUnread = "1";
			}
			if (includeDisplay && listConfig.ColumnRenderMode == ColumnRenderMode.Iconic)
			{
				string imgId = "img_" + baseName;
				displayTag = string.Format(CultureInfo.InvariantCulture, Constants.ImageTagTemplate, imgId, listConfig.UnreadImagePath);
			}
			return string.Format(CultureInfo.InvariantCulture, Constants.FieldHtmlTemplate, rowId, isUnread, isFolder, listId, itemId, displayTag);
		}
		
		public override string RenderFieldValueAsJson(object value)
		{
			if (!this.ConfigurationIsOk || !this.ParentListIsSupportedListType)
			{
				return null;
			}
			//this is the flag that tells SharePoint we know how
			//to render the data as Json, for standard lists we defer 
			//to the Html rendering as its faster, but for discussion 
			//boards its all done in Ajax and Jquery so we may as well
			//send Json data
			if (this.IsDiscussionBoard)
			{
				if (null == value)
				{
					return true.ToString();
				}
				ReadUnreadFieldValue fieldValue = GetFieldValueFromRowData(value);
				if (null == fieldValue)
				{
					return null;
				}
				ReadUnreadJsonValue returnValue = new ReadUnreadJsonValue(fieldValue, this.IsRead(fieldValue.ItemId));
				return returnValue.ToJson();
			}
			return null;
		}
		
		static ReadUnreadFieldValue GetFieldValueFromRowData(object value)
		{
			if (value == null)
			{
				return null;
			}
			ReadUnreadFieldValue returnValue = value as ReadUnreadFieldValue;
			if (null != returnValue)
			{
				return returnValue;
			}
			string fieldText = value as string;
			if (string.IsNullOrEmpty(fieldText))
			{
				return null;
			}
			returnValue = new ReadUnreadFieldValue(fieldText);
			return returnValue;
		}
		
		public override bool Filterable
		{
			get { return false; }
		}
		
		public override bool Sortable
		{
			get { return false; }
		}
		
		public override bool FilterableNoRecurrence
		{
			get { return false; }
		}
		
		public override object PreviewValueTyped
		{
			get { return new ReadUnreadFieldValue(); }
		}
		
		public override string DefaultValue
		{
			get { return this.DefaultValueTyped.ToString(); }
			set { base.DefaultValue = value; }
		}
		
		public override object DefaultValueTyped
		{
			get
			{
				ReadUnreadFieldValue fieldValue = new ReadUnreadFieldValue();
				return fieldValue;
			}
		}
		
		/// <summary>
		/// Provision our support lists and verify we are the 
		/// only field on teh list
		/// </summary>
		/// <param name="op"></param>
		public override void OnAdded(SPAddFieldOptions op)
		{
			this.LogMessage("Field added");
			if (this.ConfigurationIsOk && this.ParentListIsSupportedListType)
			{
				SqlRemarQ.ProvisionReadUnreadTable(this.ParentList.ID);
				SqlRemarQ.CreateListConfiguration(this.ParentList.ID,
					this.ParentList.ParentWeb.ID,
					this.ParentList.ParentWeb.Site.ID,
					this.Id,
					ColumnRenderMode.BoldDisplay,
					Constants.DefaultReadImageUrl,
					Constants.DefaultUnreadImageUrl,
					Constants.DefaultUnreadColor,
					Constants.DefaultUnreadColor,
					ListConfiguration.ContextMenuType.All,
					ListConfiguration.VersionUpdateType.All,
					this.InternalName,
					this.ParentList.ParentWeb.Language,
					this.ParentList.ParentWebUrl);
				
				base.OnAdded(op);
				string defaultDescription = Framework.ResourceManager.GetString(this.ParentList.ParentWeb.Locale, "FieldDefaultFieldDescription");
				if (!defaultDescription.IsNullOrEmpty())
				{
					this.Description = defaultDescription;
				}
				
				//Set the flag to kick off a full scan and install of items
				SqlRemarQ.QueueListCommand(this.ParentList, ListCommand.Provision);
			}
		}
		
		void LogMessage(string logThis)
		{
			if (null != this.ParentList)
			{
				string listName = this.ParentList.Title;
				SPUser actor = this.ParentList.ParentWeb.CurrentUser;
				string messagetoLog = string.Format("{0} executed the action '{1}' on the list named {2} in the web {3} at {4}", actor.LoginName, logThis, listName, this.ParentList.ParentWeb.Url, DateTime.UtcNow.ToString(this.ParentList.ParentWeb.Locale));
				RemarQLog.LogWarning(messagetoLog);
			}
		}
		
		public override void OnDeleting()
		{
			this.LogMessage("Field deleted");
			if (!this._BypassVerify)
			{
				base.OnDeleting();
				if (this.ConfigurationIsOk)
				{
					SqlRemarQ.QueueListCommand(this.ParentList, ListCommand.Deprovision);
				}
			}
		}
		
		public override void Update()
		{
			this.LogMessage("Field updated");
			if (this.ConfigurationIsOk)
			{
				if (this.Title != Constants.ReadUnreadFieldName)
				{
					this.Title = Constants.ReadUnreadFieldName;
				}
				if (this.ParentListIsSupportedListType)
				{ 
					base.Update();
					SqlRemarQ.QueueListCommand(this.ParentList, ListCommand.Verify);
				}
			}
		}
		
		public override bool NoCrawl
		{
			get { return true; }
			set { }
		}
		
		bool ConfigurationIsOk
		{
			get { return this._ConfigurationIsOk; }
		}
		
		/// <summary>
		/// determines if the host  list is a discussion board
		/// </summary>
		internal bool IsDiscussionBoard
		{
			get
			{
				if (!this._IsDiscussionBoardEvaluated)
				{
					this._IsDiscussionBoardEvaluated = true;
					if (null != this.ParentList)
					{
						this._IsDiscussionBoard = this.ParentList.IsDiscussionBoard();
					}
				}
				return this._IsDiscussionBoard;
			}
		}
		
		/// <summary>
		/// failsafe check for template deployed
		/// lists that still need to be provisioned
		/// </summary>
		internal bool IsProvisioned
		{
			get
			{
				if (!this._IsProvisionedEvaluated)
				{
					this._IsProvisionedEvaluated = true;
					//default to an ok to let the render fall to the broken indicator
					this._IsProvisioned = true;
					if (null != this.ParentList)
					{
						ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(this.ParentList.ID);
						if (null == listConfig)
						{
							if (this.ParentListIsSupportedListType)
							{
								//we exist but have not yet been setup, this happens during a template deployment
								SqlRemarQ.ProvisionReadUnreadTable(this.ParentList.ID);
								SqlRemarQ.CreateListConfiguration(this.ParentList.ID,
									this.ParentList.ParentWeb.ID,
									this.ParentList.ParentWeb.Site.ID,
									this.Id,
									ColumnRenderMode.BoldDisplay,
									Constants.DefaultReadImageUrl,
									Constants.DefaultUnreadImageUrl,
									Constants.DefaultUnreadColor,
									Constants.DefaultUnreadColor,
									ListConfiguration.ContextMenuType.All,
									ListConfiguration.VersionUpdateType.All,
									this.InternalName,
									this.ParentList.ParentWeb.Language,
									this.ParentList.ParentWebUrl);
								
								//Set the flag to kick off a full scan and install of items
								SqlRemarQ.QueueListCommand(this.ParentList, ListCommand.ReInitialize);
								//tell the renderer to show the working gif
								this._IsProvisioned = false;
							}
						}
					}
				}
				return this._IsProvisioned;
			}
		}
		
		/// <summary>
		/// determines if we are a supportd type or not
		/// based on the license, the list type and the
		/// release id status of the configuration
		/// </summary>
		internal bool ParentListIsSupportedListType
		{
			get
			{
				if (!this._SupportedListEvaluated)
				{
					this._SupportedListEvaluated = true;
					this._SupportedListType = IsSupportedListType(this.ParentList);
				}
				return this._SupportedListType;
			}
		}
		
		bool IsAnonymous
		{
			get
			{
				if (null == this.ParentList)
				{
					return true;
				}
				if (null == this.ParentList.ParentWeb)
				{
					return true;
				}
				if (null == this.ParentList.ParentWeb.CurrentUser)
				{
					return true;
				}
				if (null == HttpContext.Current.User)
				{
					return true;
				}
				if (null == HttpContext.Current.User.Identity)
				{
					return true;
				}
				return false;// !HttpContext.Current.User.Identity.IsAuthenticated;
			}
		}
		
		/// <summary>
		/// determines if we are a supported type or not
		/// based on the license, the list type and the
		/// release id status of the configuration
		/// </summary>
		internal static bool IsSupportedListType(SPList parentList)
		{
			bool returnValue = false;
			if (null != parentList)
			{
				if (FarmSettings.Settings.IsOk)
				{
					if (FarmLicense.License.LicenseMode > LicenseModeType.Invalid)
					{
						returnValue = parentList.IsDocumentLibrary();
						if (!returnValue)
						{
							if (FarmLicense.License.LicenseMode >= LicenseModeType.Professional)
							{
								returnValue = parentList.IsSimpleList() || parentList.IsDiscussionBoard();
							}
						}
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// Calculate the link to our javascript emitter (/_layouts.../readunreadjslink.asxp
		///SharePoint does not provide access to any of the fields custom propeties for 
		///the client side rendering code. In previous versions we encoded that value
		///into the direction attribute of the CAML or XSL, with JSLink we have no
		///opportunity to render the field so instead of doing that we will provide 
		///a unique JSLink url, and on the server side we will lookup the settings and
		///markup the javascript rendering template on the server. We append the version
		///to the query string so browsers know when to reload the script
		///unfortunately someone on the SharePoint team decided to HTMLEncode anything on 
		///the querystring so we do use the index of the list in the cache to get
		///back to it , otherwise the url ends up looking like this
		///versionNum=0&amp;viewType=HTML&amp;listId=0f4d67f5-71f2-4983-9101-a16f0c64d06f
		///This only returns a path to a script if its a list view, form parts 
		///are handled in the rendering control
		/// </summary>
		public override string JSLink
		{
			get
			{
				if (!this._JSLinkEvaluated)
				{
					lock (this._LockProxy)
					{
						this._JSLink = null;
						this._JSLinkEvaluated = true;
						
						//break early _JSLink is null 
						if (!this.ConfigurationIsOk || !this.ParentListIsSupportedListType)
						{
							return null;
						}
						
						//discussion boards are handled by the view JSLink not
						//the individual field, so if its a discussion return null
						if (this.IsDiscussionBoard)
						{
							return null;
						}
						
						//in form mode we let the field render control do its work
						//and dont monkey with teh field display since we dont even
						//want to have a presense on the form
						SPFormContext formCtx = SPContext.Current.FormContext;
						SPViewContext viewCtx = SPContext.Current.ViewContext;
						
						if (null != formCtx)
						{
							if (formCtx.FormMode == SPControlMode.New || formCtx.FormMode == SPControlMode.Edit)
							{
								return null;
							}
							if (formCtx.FormMode == SPControlMode.Display)
							{
								//this may be a web part display in another page
								//if there is a CSRFieldValueCollection then we are on an an item display form
								if (null != formCtx.CSRFieldValueCollection ||
									null != formCtx.CSRDefaultValuesCollection)
								{
									return null;
								}
							}
						}
						
						string viewType = "HTML";
						//Our JSLink is optimized for views, not forms so check our current context
						if (null != viewCtx && null != viewCtx.View)
						{
							viewType = viewCtx.View.Type; 
						}
						ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(this.ParentList.ID);
						if (null != listConfig)
						{
							//I tried to include the JS with dependancies here but 
							//it did not work right 100% of the time, so the library is 
							//included with the additional page head from the control library
							string jsLinkUrl = this.ParentList.ParentWebUrl;
							if (!jsLinkUrl.EndsWith("/", StringComparison.OrdinalIgnoreCase))
							{
								jsLinkUrl += "/";
							}
							
							jsLinkUrl += Constants.JSLinkServiceUrl;
							bool isDiscussion = false;
							string listId = System.Web.HttpUtility.UrlEncode(listConfig.ListId.ToString("D"));
							string versionNum = listConfig.CacheVersion.ToString(CultureInfo.InvariantCulture);
							string queryString = listId + "\t" + viewType + "\t" + versionNum + "\t" + isDiscussion.ToString();
							string base64 = Convert.ToBase64String(System.Text.UnicodeEncoding.Unicode.GetBytes(queryString));
							jsLinkUrl += "?" + base64;
							
							System.Diagnostics.Trace.WriteLine(jsLinkUrl);
							this._JSLink = jsLinkUrl;
						}
					}
				}
				return this._JSLink;
			}
			set { base.JSLink = value; }
		}
		
		#endregion
		
		#region Utilities
		
		/// <summary>
		/// delete without deprovisioning
		/// </summary>
		internal void DeleteWithoutDeprovisioning()
		{
			this._BypassVerify = true;
			this.Delete();
			this._BypassVerify = false;
		}
		
		/// <summary>
		/// the actural read/unread status (from our Sql table) for this 
		/// user and this row 
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		bool IsRead(int itemId)
		{
			lock (this._LockProxy)
			{
				if (null == this._ThisUsersReadMarks)
				{
					this._ThisUsersReadMarks = new RangedReadMarksQuery(this.CurrentUser, this.ParentList.ID, itemId, this.IsDiscussionBoard);
				}
				return this._ThisUsersReadMarks.IsRead(itemId);
			}
		}
		
		SPUser CurrentUser
		{
			get { return this.ParentList.ParentWeb.CurrentUser; }
		}
		
		/// <summary>
		/// install the context menu on the list
		/// </summary>
		static internal void InstallContextMenuInternal(ListConfiguration listConfig, SPList parentList)
		{
			if (null != listConfig && IsSupportedListType(parentList))
			{ 
				if (FarmLicense.License.LicenseMode >= LicenseModeType.Professional)
				{
					//IEnumerable<CultureInfo> installedCultures = Framework.ResourceManager.GetInstalledCultures();
					string readTitle = Framework.ResourceManager.GetString(parentList.ParentWeb.Locale, "ItemContextMenuMarkRead");
					string unreadTitle = Framework.ResourceManager.GetString(parentList.ParentWeb.Locale, "ItemContextMenuMarkUnread");
					
					string listId = parentList.ID.ToString("N");
					string javascriptFunctionName = "remarQContextMenu" + listId ;
					if (listConfig.ContextMenu == ListConfiguration.ContextMenuType.All || listConfig.ContextMenu == ListConfiguration.ContextMenuType.ReadTool)
					{
						SPUserCustomAction markRead = parentList.UserCustomActions.Add();
						markRead.Title = readTitle;
						if (null != markRead.TitleResource)
						{
							foreach (CultureInfo installedCulture in parentList.ParentWeb.SupportedUICultures)
							{
								string promptValue = Framework.ResourceManager.GetString(installedCulture, "ItemContextMenuMarkRead");
								markRead.TitleResource.SetValueForUICulture(installedCulture, promptValue);
								markRead.TitleResource.Update();
							}
						}
						markRead.Name = "MarkRead";
						markRead.Location = "EditControlBlock";
						markRead.Url = "javascript:" + javascriptFunctionName + "('{ItemId}',false);";
						markRead.Sequence = 2000;
						
						markRead.Update();
						
						SPUserCustomAction markUnRead = parentList.UserCustomActions.Add();
						markUnRead.Title = unreadTitle;
						if (null != markUnRead.TitleResource)
						{
							foreach (CultureInfo installedCulture in parentList.ParentWeb.SupportedUICultures)
							{
								string promptValue = Framework.ResourceManager.GetString(installedCulture, "ItemContextMenuMarkUnread");
								markUnRead.TitleResource.SetValueForUICulture(installedCulture, promptValue);
								markUnRead.TitleResource.Update();
							}
						}
						
						markUnRead.Name = "MarkUnRead";
						markUnRead.Location = "EditControlBlock";
						markUnRead.Url = "javascript:" + javascriptFunctionName + "('{ItemId}',true);";
						markUnRead.Sequence = 2002;
						markUnRead.Update();
					}
					
					if (FarmLicense.License.LicenseMode >= LicenseModeType.Enterprise)
					{
						if (listConfig.ContextMenu == ListConfiguration.ContextMenuType.All || listConfig.ContextMenu == ListConfiguration.ContextMenuType.ReportTool)
						{
							javascriptFunctionName = "remarQReport" + listId ;
							string remarqReportTitle = Framework.ResourceManager.GetString(parentList.ParentWeb.Locale, "ItemContextMenuRemarQReport");
							SPUserCustomAction remarqReport = parentList.UserCustomActions.Add();
							remarqReport.Title = remarqReportTitle;
							
							if (null != remarqReport.TitleResource)
							{
								foreach (CultureInfo installedCulture in parentList.ParentWeb.SupportedUICultures)
								{
									string promptValue = Framework.ResourceManager.GetString(installedCulture, "ItemContextMenuRemarQReport");
									remarqReport.TitleResource.SetValueForUICulture(installedCulture, promptValue);
									remarqReport.TitleResource.Update();
								}
							}
							
							remarqReport.Name = "RemarQReport";
							remarqReport.Location = "EditControlBlock";
							remarqReport.Url = "javascript:" + javascriptFunctionName + "('{ItemId}');";
							remarqReport.Sequence = 2004;
							
							remarqReport.Update();
						}
					}
					
					parentList.Update();
				}
			}
		}
		
		/// <summary>
		/// verifies that context menus are installed on a list
		/// </summary>
		/// <param name="parentList"></param>
		/// <param name="menuType"></param>
		/// <returns></returns>
		static internal bool AreAllContextMenusInstalled(SPList parentList, ListConfiguration.ContextMenuType menuType)
		{
			bool returnValue = false;
			if (IsSupportedListType(parentList))
			{
				bool markRead = false;
				bool markUnread = false;
				bool remarQReport = FarmLicense.License.LicenseMode < LicenseModeType.Enterprise;
				foreach (SPUserCustomAction userAction in parentList.UserCustomActions)
				{
					if (userAction.Name == "MarkRead")
					{
						markRead = true;
					}
					if (userAction.Name == "MarkUnRead")
					{
						markUnread = true;
					}
					
					if (FarmLicense.License.LicenseMode >= LicenseModeType.Enterprise)
					{
						if (userAction.Name == "RemarQReport")
						{
							remarQReport = true;
						}
					}
				}
				if (menuType == ListConfiguration.ContextMenuType.All)
				{
					returnValue = markUnread && markRead && remarQReport;
				}
				else if (menuType == ListConfiguration.ContextMenuType.ReadTool)
				{
					returnValue = markUnread && markRead && !remarQReport;
				}
				else if (menuType == ListConfiguration.ContextMenuType.ReportTool)
				{
					returnValue = !markUnread && !markRead && remarQReport;
				}
				else
				{
					returnValue = !markUnread && !markRead && !remarQReport;
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// remove the context menu
		/// </summary>
		
		/// <summary>
		/// remove the context menu
		/// </summary>
		static internal void UninstallContextMenuInternal(SPList parentList)
		{
			List<SPUserCustomAction> deleteThese = new List<SPUserCustomAction>();
			foreach (SPUserCustomAction userAction in parentList.UserCustomActions)
			{
				if (userAction.Name == "MarkRead" || userAction.Name == "MarkUnRead" || userAction.Name == "RemarQReport")
				{
					deleteThese.Add(userAction);
				}
			}
			foreach (SPUserCustomAction deleteMe in deleteThese)
			{
				deleteMe.Delete();
				parentList.Update();
			}
		}
		
		/// <summary>
		/// install the event handlers on the list
		/// </summary>
		static internal void InstallEventHandlersInternal(SPList parentList)
		{
			string eventAssembly = Assembly.GetExecutingAssembly().FullName;
			string eventClass = typeof(ReadUnreadListItemEventReceiver).ToString();
			parentList.EventReceivers.Add(SPEventReceiverType.ItemFileMoved, eventAssembly, eventClass);
			parentList.EventReceivers.Add(SPEventReceiverType.ItemUpdated, eventAssembly, eventClass);
			parentList.EventReceivers.Add(SPEventReceiverType.ItemDeleted, eventAssembly, eventClass);
			parentList.EventReceivers.Add(SPEventReceiverType.ItemAdded, eventAssembly, eventClass);
			parentList.EventReceivers.Add(SPEventReceiverType.ItemCheckedIn, eventAssembly, eventClass);
			parentList.Update();
		}
		
		/// <summary>
		/// checks to see if the event handlers are correctly installed
		/// </summary>
		/// <returns></returns>
		static internal bool AreAllEventHandlersInstalled(SPList parentList)
		{
			string eventAssembly = Assembly.GetExecutingAssembly().FullName;
			string eventClass = typeof(ReadUnreadListItemEventReceiver).ToString();
			int itemFileMovedCount = 0;
			int itemUpdatedCount = 0;
			int itemDeletedCount = 0;
			int itemAddedCount = 0;
			int itemCheckedInCount = 0;
			for (int eventIndex = parentList.EventReceivers.Count; eventIndex > 0; eventIndex--)
			{
				SPEventReceiverDefinition eventDefinition = parentList.EventReceivers[eventIndex - 1];
				if (eventDefinition.Assembly == eventAssembly && eventDefinition.Class == eventClass)
				{
					if (eventDefinition.Type == SPEventReceiverType.ItemFileMoved)
					{
						itemFileMovedCount++;
					}
					if (eventDefinition.Type == SPEventReceiverType.ItemUpdated)
					{
						itemUpdatedCount++;
					}
					if (eventDefinition.Type == SPEventReceiverType.ItemDeleted)
					{
						itemDeletedCount++;
					}
					if (eventDefinition.Type == SPEventReceiverType.ItemAdded)
					{
						itemAddedCount++;
					}
					if (eventDefinition.Type == SPEventReceiverType.ItemCheckedIn)
					{
						itemCheckedInCount++;
					}
				}
			}
			return (itemFileMovedCount == 1 && itemUpdatedCount == 1 && itemDeletedCount == 1 && itemAddedCount == 1 && itemCheckedInCount == 1);
		}
		
		/// <summary>
		/// remove all event handlers
		/// </summary>
		static internal void UninstallAllEventHandlersInternal(SPList parentList)
		{
			//Remove all registered copies of our ReadUnreadListItemEventHandler 
			string eventAssembly = Assembly.GetExecutingAssembly().FullName;
			string eventClass = typeof(ReadUnreadListItemEventReceiver).ToString();
			bool updateList = false;
			for (int eventIndex = parentList.EventReceivers.Count; eventIndex > 0; eventIndex--)
			{
				SPEventReceiverDefinition eventDefinition = parentList.EventReceivers[eventIndex - 1];
				if (eventDefinition.Assembly == eventAssembly)
				{
					if (eventDefinition.Class == eventClass)
					{
						eventDefinition.Delete();
						updateList = true;
					}
				}
			}
			if (updateList)
			{
				parentList.Update();
			}
		}
		
		static internal void ProvisionSpecialViews(SPList parentList)
		{
			SharePointRemarQ.ProvisionViews(parentList);
		}
		
		static internal void DeProvisionSpecialViews(SPList parentList)
		{
			SharePointRemarQ.DeProvisionViews(parentList);
		}
		
		static internal void VerifyViews(SPList parentList)
		{
			SharePointRemarQ.VerifyViews(parentList);
		}
		
		#endregion
	}
}