// -----------------------------------------------------------------------------
//  Copyright 6/11/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.SharePoint;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// this class represents the runtime configuration for a list
	/// it is built from the the configuration table in the 
	/// ReadUnread database.
	/// </summary>
	internal class ListConfiguration
	{
		internal enum ContextMenuType : int
		{
			All = 0,
			None = 1,
			ReadTool = 2,
			ReportTool = 3
		}

		internal enum VersionUpdateType : int
		{
			All = 0,
			None = 1,
			VersionChange = 2
		}

		#region Fields & Declarations
		
		//common identifier use for forcing cache updates
		//by including a version number vairation in the client Urls
		static long _VersionNumber = DateTime.UtcNow.Ticks;
		
		/// <summary>
		/// mode to render the column in
		/// </summary>
		internal ColumnRenderMode ColumnRenderMode { get; private set; }
		
		/// <summary>
		/// Read image to use if the columne render mode Is Iconic
		/// </summary>
		internal string ReadImagePath { get; private set; }
		
		/// <summary>
		/// Unread image to use if the columne render mode Is Iconic
		/// </summary>
		internal string UnreadImagePath { get; private set; }
		
		/// <summary>
		/// Unread font color to use if the column render mode Is BoldDisplay
		/// </summary>
		internal string UnreadHtmlColor { get; private set; }
		
		/// <summary>
		/// if true, the context menu is added to the list 
		/// </summary>
		internal ContextMenuType ContextMenu { get; private set; }

		/// <summary>
		/// if true, the context menu is added to the list 
		/// </summary>
		internal VersionUpdateType VersionUpdate { get; private set; }

		/// <summary>
		/// field identifier
		/// </summary>
		internal Guid FieldId { get; private set; }
		
		/// <summary>
		/// list identifier
		/// </summary>
		internal Guid ListId { get; private set; }
		
		/// <summary>
		/// web identifier
		/// </summary>
		internal Guid WebId { get; private set; }
		
		/// <summary>
		/// site identifier
		/// </summary>
		internal Guid SiteId { get; private set; }
		
		/// <summary>
		/// web relative layouts url
		/// </summary>
		internal string LayoutsPath { get; private set; }
		
		/// <summary>
		/// field public name
		/// </summary>
		internal string PublicName { get; private set; }
		
		/// <summary>
		/// web site culture name 
		/// </summary>
		internal CultureInfo Locale { get; private set; }
		
		internal int RefreshInterval { get; private set; }

		internal int ConfigOne { get; private set; }

		internal int ConfigTwo { get; private set; }

		internal int ConfigThree { get; private set; }

		long _CacheVersion = -1;
		readonly string _JSLinkJavaScript = string.Empty;
		readonly string _JSLinkDiscussionViewSubjectJavaScript = string.Empty;
		readonly string _JSLinkDiscussionViewFlatJavaScript = string.Empty;
		readonly string _JSLinkDiscussionViewThreadedJavaScript = string.Empty;
		readonly string _JavaScriptJsLinkTemplateAnonymous = string.Empty;

		#endregion
		
		#region CTORs
		
		internal ListConfiguration(Guid listId)
		{
			ListConfiguration cloneMe = SqlRemarQ.GetListConfiguration(listId);
			if (null == cloneMe)
			{
				throw new ArgumentOutOfRangeException("listId", "No list found with the id " + listId.ToString("B"));
			}
			this.ReadImagePath = cloneMe.ReadImagePath;
			this.UnreadImagePath = cloneMe.UnreadImagePath;
			this.UnreadHtmlColor = cloneMe.UnreadHtmlColor;
			this.ColumnRenderMode = cloneMe.ColumnRenderMode;
			this.ContextMenu = cloneMe.ContextMenu;
			
			this.FieldId = cloneMe.FieldId;
			this.ListId = cloneMe.ListId;
			this.WebId = cloneMe.WebId;
			this.SiteId = cloneMe.SiteId;
			this.PublicName = cloneMe.PublicName;
			this.LayoutsPath = cloneMe.LayoutsPath;
			this.Locale = cloneMe.Locale;
			this._JSLinkJavaScript = cloneMe._JSLinkJavaScript;
			this._JSLinkDiscussionViewSubjectJavaScript = cloneMe._JSLinkDiscussionViewSubjectJavaScript;
			this._JSLinkDiscussionViewFlatJavaScript = cloneMe._JSLinkDiscussionViewFlatJavaScript;
			this._JSLinkDiscussionViewThreadedJavaScript = cloneMe._JSLinkDiscussionViewThreadedJavaScript;
			this._JavaScriptJsLinkTemplateAnonymous = cloneMe._JavaScriptJsLinkTemplateAnonymous;
			this._CacheVersion = Interlocked.Increment(ref _VersionNumber);
			this.VersionUpdate = cloneMe.VersionUpdate;
			this.RefreshInterval = cloneMe.RefreshInterval;
			this.ConfigOne = cloneMe.ConfigOne;
			this.ConfigTwo = cloneMe.ConfigTwo;
			this.ConfigThree = cloneMe.ConfigThree;
		}
		
		internal ListConfiguration(SqlDataReader dataReader)
		{
			if (null == dataReader)
			{
				throw new ArgumentNullException("dataReader");
			}
			int colIdx = dataReader.GetOrdinal("ListId");
			this.ListId = dataReader.GetGuid(colIdx);
			colIdx = dataReader.GetOrdinal("WebId");
			this.WebId = dataReader.GetGuid(colIdx);
			colIdx = dataReader.GetOrdinal("SiteId");
			this.SiteId = dataReader.GetGuid(colIdx);
			colIdx = dataReader.GetOrdinal("FieldId");
			this.FieldId = dataReader.GetGuid(colIdx);
			colIdx = dataReader.GetOrdinal("ColumnRenderMode");
			this.ColumnRenderMode = (ColumnRenderMode)dataReader.GetInt32(colIdx);
			colIdx = dataReader.GetOrdinal("ReadImageUrl");
			this.ReadImagePath = dataReader.GetString(colIdx);
			colIdx = dataReader.GetOrdinal("LayoutsUrl");
			this.LayoutsPath = dataReader.GetString(colIdx);
			if (!this.LayoutsPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				this.LayoutsPath += "/";
			}
			colIdx = dataReader.GetOrdinal("UnreadImageUrl");
			this.UnreadImagePath = dataReader.GetString(colIdx);
			
			colIdx = dataReader.GetOrdinal("UnreadhHtmlColor");
			this.UnreadHtmlColor = dataReader.GetString(colIdx);
			colIdx = dataReader.GetOrdinal("ShowEditingTools");
			this.ContextMenu = (ContextMenuType)dataReader.GetInt32(colIdx);
			
			colIdx = dataReader.GetOrdinal("VersionUpdateFlags");
			this.VersionUpdate = (VersionUpdateType)dataReader.GetInt32(colIdx);

			colIdx = dataReader.GetOrdinal("PublicName");
			this.PublicName = dataReader.GetString(colIdx);
			
			colIdx = dataReader.GetOrdinal("CultureName");
			this.Locale = CultureInfo.CreateSpecificCulture(dataReader.GetString(colIdx));
			
			colIdx = dataReader.GetOrdinal("RefreshInterval");
			this.RefreshInterval = dataReader.GetInt32(colIdx);
			colIdx = dataReader.GetOrdinal("ConfigOne");
			this.ConfigOne = dataReader.GetInt32(colIdx);
			colIdx = dataReader.GetOrdinal("ConfigTwo");
			this.ConfigTwo = dataReader.GetInt32(colIdx);
			colIdx = dataReader.GetOrdinal("ConfigThree");
			this.ConfigThree = dataReader.GetInt32(colIdx);

			this._JSLinkJavaScript = this.ReplaceTokens(Constants.JavaScriptJsLinkTemplate);
			
			this._JSLinkDiscussionViewSubjectJavaScript = this.ReplaceTokens(Constants.JavaScriptDiscussionBoardSubjectJsLinkTemplate);
			this._JSLinkDiscussionViewFlatJavaScript = this.ReplaceTokens(Constants.JavaScriptDiscussionBoardFlatJsLinkTemplate);
			this._JSLinkDiscussionViewThreadedJavaScript = this.ReplaceTokens(Constants.JavaScriptDiscussionBoardThreadedJsLinkTemplate);
			this._JavaScriptJsLinkTemplateAnonymous = this.ReplaceTokens(Constants.JavaScriptJsLinkTemplateAnonymous);
			#if !DEBUG

_JSLinkJavaScript = _JSLinkJavaScript.MinifyJavaScript();
 
			#endif
			this._CacheVersion = Interlocked.Increment(ref _VersionNumber);
		}
		
		#endregion
		
		#region Markup 
		
		/// <summary>
		/// markup the javascript with this list's instance values
		/// </summary>
		/// <param name="updateThis">script to update</param>
		/// <returns></returns>
		string ReplaceTokens(string updateThis)
		{
			string returnValue = updateThis.Replace("_FIELDGUID_", this.FieldId.ToString("D"));
			returnValue = returnValue.Replace("_FIELDID_", this.FieldId.ToString("N"));
			returnValue = returnValue.Replace("_FIELDNAME_", Constants.ReadUnreadFieldName);
			returnValue = returnValue.Replace("_LISTID_", this.ListId.ToString("N"));
			returnValue = returnValue.Replace("_LISTGUID_", this.ListId.ToString("B").ToUpperInvariant());
			returnValue = returnValue.Replace("_SITEID_", this.SiteId.ToString("N"));
			returnValue = returnValue.Replace("_SITEGUID_", this.SiteId.ToString("D"));
			returnValue = returnValue.Replace("_WEBID_", this.WebId.ToString("N"));
			returnValue = returnValue.Replace("_WEBGUID_", this.WebId.ToString("D"));
			
			returnValue = returnValue.Replace("_REPORTTITLE_", Framework.ResourceManager.GetString(this.Locale, "RemarQReportTitle"));
			returnValue = returnValue.Replace("_REMINDERTITLE_", Framework.ResourceManager.GetString(this.Locale, "RemarQReminderTitle"));
			
			returnValue = returnValue.Replace("_MARKUNREADPROMPT_", Framework.ResourceManager.GetString(this.Locale, "ItemContextMenuMarkUnread"));
			returnValue = returnValue.Replace("_MARKREADPROMPT_", Framework.ResourceManager.GetString(this.Locale, "ItemContextMenuMarkRead"));
			 returnValue = returnValue.Replace("_MENUIMGURL_", Constants.InlineBMenuImageUrl);
			 
			returnValue = returnValue.Replace("_LOADINGIMGURL_", Constants.LoadingImageUrl);

			returnValue = returnValue.Replace("_RUSERVICEURL_", this.LayoutsPath + Constants.LazyUpdateServiceUrl);
			returnValue = returnValue.Replace("_RUQUERY_", this.LayoutsPath + Constants.ReadUnreadQueryServiceUrl + "?listId=" + this.ListId.ToString("N"));
			returnValue = returnValue.Replace("_REPORTURL_", this.LayoutsPath + Constants.ReadUnreadReportUrl);
			returnValue = returnValue.Replace("_LICENSEREMINDERURL_", this.LayoutsPath + Constants.LicenseReminderUrl);
			
			returnValue = returnValue.Replace("_UNREADIMAGEURL_", this.UnreadImagePath);
			returnValue = returnValue.Replace("_READIMAGEURL_", this.ReadImagePath);
			returnValue = returnValue.Replace("_ERRORIMAGEURL_", Constants.ErrorImageUrl);
			returnValue = returnValue.Replace("_COLRENDERMODE_", ((int)this.ColumnRenderMode).ToString(CultureInfo.InvariantCulture));
			returnValue = returnValue.Replace("_COLRENDERCOLOR_", this.UnreadHtmlColor);
			returnValue = returnValue.Replace("_HTTPMODULE_", FarmSettings.Settings.TrackDocuments.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture));
			int refreshInterval = this.RefreshInterval;

			//if we dont allow refresh then set it to 0
			if (FarmSettings.Settings.MinJavascriptClientRefreshInterval <= 0)
			{
				refreshInterval = 0;
			}
			else
			{
				//but it cant be less than the allowed minimum poll value
				if (refreshInterval < FarmSettings.Settings.MinJavascriptClientRefreshInterval)
				{
					refreshInterval = FarmSettings.Settings.MinJavascriptClientRefreshInterval;
				}
			}
			returnValue = returnValue.Replace("_REFRESHINTERVAL_", refreshInterval.ToString(CultureInfo.InvariantCulture));
			return returnValue;
		}
		
		#endregion
		
		#region Accessors
		
		/// <summary>
		/// current cache version
		/// </summary>
		internal long CacheVersion 
		{
			get
			{
				//If we are unlicensed then we dont want the client to
				//cache the javascript. So we increment this, which will
				//cause the jslink url to be different
				if (!FarmLicense.License.IsLicensed())
				{
					this._CacheVersion = Interlocked.Increment(ref _VersionNumber);
				}
				return this._CacheVersion;
			}
		}
		
		/// <summary>
		/// script for the JSLink aspx page to emit for this list
		/// when there is no authentication context
		/// </summary>
		internal string JSLinkJavaScriptAnonymous(string pathAndQuery)
		{ 
			return this._JavaScriptJsLinkTemplateAnonymous.Replace("_PATHANDQUERY_", pathAndQuery);
		}
		
		/// <summary>
		/// script for the JSLink aspx page to emit for this list
		/// </summary>
		internal string JSLinkJavaScript(string pathAndQuery)
		{ 
			if (FarmLicense.License.ShouldShowLicenseReminder())
			{
				return this._JSLinkJavaScript.Replace("_ALERT_", true.ToString().ToLowerInvariant());
			}
			return this._JSLinkJavaScript.Replace("_PATHANDQUERY_", pathAndQuery);
		}

		/// <summary>
		/// script for the JSLink aspx page to emit for this discussion board
		/// </summary>
		internal string JSLinkDiscussionViewJavaScript(string pathAndQuery, string templateName)
		{
			templateName = templateName.ToUpperInvariant().Trim();
			if (templateName.Equals("THREADED"))
			{
				return this.JSLinkThreadedDiscussionViewJavaScript(pathAndQuery, Guid.Empty,-1);
			}
			else
			{
				string returnValue = string.Empty;
				if (!string.IsNullOrWhiteSpace(templateName))
				{
					switch(templateName)
					{
						case "SUBJECT":
						case "ALLITEMS":
							returnValue = this._JSLinkDiscussionViewSubjectJavaScript;
							break;
						case "FLAT":
							returnValue = this._JSLinkDiscussionViewFlatJavaScript;
							break;
					}
				}
				if (FarmLicense.License.ShouldShowLicenseReminder())
				{
					returnValue = returnValue.Replace("_ALERT_", true.ToString().ToLowerInvariant());
				}
				return returnValue.Replace("_PATHANDQUERY_", pathAndQuery);
			}
		}
		
		/// <summary>
		/// script for threaded js that matches RemarQThread.xsl
		/// </summary>
		/// <param name="pathAndQuery"></param>
		/// <param name="viewId"></param>
		/// <returns></returns>
		internal string JSLinkThreadedDiscussionViewJavaScript(string pathAndQuery, Guid viewId, int userId)
		{
			string returnValue = this._JSLinkDiscussionViewThreadedJavaScript;
			if (FarmLicense.License.ShouldShowLicenseReminder())
			{
				returnValue = returnValue.Replace("_ALERT_", true.ToString().ToLowerInvariant());
			}
			returnValue = returnValue.Replace("_PATHANDQUERY_", pathAndQuery);
			returnValue = returnValue.Replace("_VIEWGUID_", viewId.ToString("B").ToUpperInvariant());
			returnValue = returnValue.Replace("_JSUSERID_", userId.ToString());
			return returnValue;
		}
		
		#endregion
	}
}