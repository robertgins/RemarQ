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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Utilities;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// constants and common statics for the project
	/// </summary>
	internal static class Constants
	{
		static readonly string[] _ReadUnreadQueryFields = new string[] { "ID", "ContentTypeId", "FileDirRef", "FileLeafRef", "FSObjType", "URL", "FileRef", "ParentFolderId", "ParentItemID", "ThreadIndex", Constants.ReadUnreadFieldName };

		//per MSOCAF performance analysis an existing property 
		//should not return an array
		//dont know why, its actually slower this way
		internal static string[] ReadUnreadQueryFields()
		{
			return _ReadUnreadQueryFields; 
		}

		//****************Warning
		//any change to this also be updated in the fldtypes_ReadUnreadField.xml 
		internal readonly static Guid ReadUnreadFieldId = new Guid("{C1F6E461-F17A-4E13-B1E3-71841F7B0672}");
		//*******************
		internal readonly static string JavaScriptJsLinkTemplate = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadMarks.JSLinkTemplate.js");
		internal readonly static string JavaScriptJsLinkTemplateAnonymous = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadMarks.JSLinkTemplateAnonymous.js");

		internal readonly static string JavaScriptDiscussionBoardSubjectJsLinkTemplate = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadMarks.JSLinkDiscussionBoardSubjectTemplate.js");
		internal readonly static string JavaScriptDiscussionBoardFlatJsLinkTemplate = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadMarks.JSLinkDiscussionBoardFlatTemplate.js");
		internal readonly static string JavaScriptDiscussionBoardThreadedJsLinkTemplate = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadMarks.JSLinkDiscussionBoardThreadedTemplate.js");

		internal readonly static string SqlConfigTableTemplate = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadMarks.CreateConfigTableTemplate.sql");
		internal readonly static string SqlResourceTableTemplate = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.Framework.CreateResourceTableTemplate.sql");
		internal readonly static string SqlQueueTableTemplate = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.Framework.CreateQueueTableTemplate.sql");

		internal readonly static string SqlReadUnreadTableTemplate = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadMarks.CreateReadUnreadTableTemplate.sql");
		internal readonly static string SqlReadUnreadTableIndicesTemplate = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadMarks.CreateReadUnreadIndicesTemplate.sql");
		internal readonly static string SqlReadUnreadTableIndicesWithVersionTemplate = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadMarks.CreateReadUnreadIndicesWithVersionTemplate.sql");
		internal readonly static string SqlVersionsTableTemplate = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadMarks.CreateVersionsTableTemplate.sql");
		internal readonly static string SqlReadUnreadDropTableIndicesTemplate = Utilities.GetEmbeddedResourceString("BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadMarks.DropReadUnreadIndicesTemplate.sql");

		internal readonly static string ProductName = "BalsamicSolutions.RemarQ";
		internal readonly static string AlternateProductName = "BalsamicSolutions.ReadUnreadSiteColumn";
		internal readonly static ColumnRenderMode DefaultColumnRenderMode = ColumnRenderMode.BoldDisplay;

		internal readonly static string DefaultUnreadColor = "#000000";
		internal readonly static string DefaultUnreadImageUrl = SPUtility.ContextImagesRoot + "BalsamicSolutions.ReadUnreadSiteColumn/clscUnReadMark.gif";
		internal readonly static string DefaultReadImageUrl = SPUtility.ContextImagesRoot + "BalsamicSolutions.ReadUnreadSiteColumn/clscReadMark.gif";
        
		//****************Warning
		//any change to this must also be changed in the Elemenents.xml for the ReadUnreadMarkSiteColumn
		internal readonly static string ReadUnreadFieldName = "RemarQ";
		//*******************

		internal readonly static int HierarchySystemUserId = -1;
		internal readonly static int VersionFlagId = -2;
		internal readonly static string ApplicationName = "Balsamic Solutions Read Unread Site Column (RemarQ)";
 
		 
		internal readonly static string InlineBMenuImageUrl = SPUtility.ContextImagesRoot + "/BalsamicSolutions.ReadUnreadSiteColumn/ReadUnreadLogo16.png";
		internal readonly static string LazyUpdateServiceUrl = SPUtility.ContextLayoutsFolder + "/BalsamicSolutions.ReadUnreadSiteColumn/LazyUpdate.aspx";
		internal readonly static string JSLinkServiceUrl = SPUtility.ContextLayoutsFolder + "/BalsamicSolutions.ReadUnreadSiteColumn/JSLink.aspx";
 
		internal readonly static string ReadUnreadJavaScriptPath = "BalsamicSolutions.ReadUnreadSiteColumn/ReadUnread.js";
		internal readonly static string ReadUnreadJavaScriptUrl = "/" + SPUtility.ContextLayoutsFolder + "/BalsamicSolutions.ReadUnreadSiteColumn/ReadUnread.js";
		internal readonly static string LicenseReminderUrl = SPUtility.ContextLayoutsFolder + "/BalsamicSolutions.ReadUnreadSiteColumn/LicenseReminder.aspx";
		internal readonly static string ReadUnreadReportUrl = SPUtility.ContextLayoutsFolder + "/BalsamicSolutions.ReadUnreadSiteColumn/ReadUnreadReport.aspx";
		internal readonly static string ReadUnreadQueryServiceUrl = SPUtility.ContextLayoutsFolder + "/BalsamicSolutions.ReadUnreadSiteColumn/ReadUnreadQuery.aspx";

		internal readonly static string ErrorImageUrl = SPUtility.ContextImagesRoot + "BalsamicSolutions.ReadUnreadSiteColumn/clscError.gif";
		internal readonly static string ConfigErrorImageUrl = SPUtility.ContextImagesRoot + "BalsamicSolutions.ReadUnreadSiteColumn/clscErrorGray.gif";
		internal readonly static string LoadingImageUrl = SPUtility.ContextImagesRoot + "BalsamicSolutions.ReadUnreadSiteColumn/clscLoading.gif";
		internal readonly static string WarningImageUrl = SPUtility.ContextImagesRoot + "BalsamicSolutions.ReadUnreadSiteColumn/clscWarning.gif";

		internal readonly static string LoadingImageTagTemplate = "<img src='" + LoadingImageUrl + "' alt='{0}' title='{0}' style='border:0px,none;height:16px;width:16px;' >";
		internal readonly static string ErrorImageTagTemplate = "<img src='" + ErrorImageUrl + "' alt='{0}' title='{0}' style='border:0px,none;height:16px;width:16px;' >";
		internal readonly static string ConfigErrorImageTagTemplate = "<img src='" + ConfigErrorImageUrl + "' alt='{0}' title='{0}' style='border:0px,none;height:16px;width:16px;' >";
		internal readonly static string WarningImageTagTemplate = "<img src='" + WarningImageUrl + "' alt='{0}' title='{0}' style='border:0px,none;height:16px;width:16px;' >";

		internal readonly static string BlankImageUrl = SPUtility.ContextImagesRoot + "BalsamicSolutions.ReadUnreadSiteColumn/clscBlank.gif";
		internal readonly static string AnonymousImageUrl = SPUtility.ContextImagesRoot + "BalsamicSolutions.ReadUnreadSiteColumn/clscBlank.gif";
		internal readonly static string BlankImageTag = "<img src='" + BlankImageUrl + "' alt='' style='border:0px,none;height:16px;width:16px;float:left' />";
		internal readonly static string AnonymousImageTag = "<img src='" + AnonymousImageUrl + "' alt='' style='border:0px,none;height:16px;width:16px;float:left' />";
		internal readonly static string EmptyDivTag = "<div></div>";

		internal readonly static string ImageTagTemplate = "<img id='{0}' src='{1}' alt='' style='border:0px,none;height:16px;width:16px;float:left' />";
		internal readonly static string FieldHtmlTemplate = "<div id='{0}' isUnread='{1}' isFolder='{2}' listId='{3}' itemId='{4}' >{5}</div>";
		internal readonly static string CloseSharePointDialogScript = "<script type=\"text/javascript\">window.frameElement.commonModalDialogClose(1, '');</script>";
		internal readonly static string ReadUnreadConfigurationTableName = "RemarQ.Config";
		internal readonly static string ReadUnreadResourceTableName = "RemarQ.Resources";
		internal readonly static string ReadUnreadVersionsTableName = "RemarQ.Versions";
		internal readonly static string ReadUnreadQueueTableName = "RemarQ.Queue";
		internal readonly static string ReadUnreadLargeQueueTableName = "RemarQ.LargeQueue";
		internal readonly static string ReadUnreadItemIdsTypeName = "ReadUnreadItemIds";
		internal readonly static int SqlReadMarkQueryRangePageSize = 30;
		internal readonly static uint CamlQueryPageSize = 2000;
		internal readonly static int SqlBatchUpdateSize = 25;

		internal readonly static string TextInitializing = "initializing";
		internal readonly static string TextError = "error";
		internal readonly static string TextRead = "read";
		internal readonly static string TextUnread = "unread";
		internal readonly static string TextAnonymous = "anonymous";

		internal readonly static string RootDiscussionCamlQuery = "<Where><IsNull><FieldRef Name='ParentItemID'/></IsNull></Where><OrderBy  Override='TRUE'><FieldRef Name='ID' /></OrderBy>";

		internal readonly static string DiscussionBoardViewFields = "<FieldRef Name='ID' /><FieldRef Name='Title'/><FieldRef Name='ItemChildCount'/><FieldRef Name='Created'/><FieldRef Name='Author'/><FieldRef Name='Body'/>";
		internal readonly static string DiscussionReplyCamlQueryTemplate = "<Where><Eq><FieldRef Name='ParentItemID'/><Value Type='Integer'>{0}</Value></Eq></Where><OrderBy Override='TRUE'><FieldRef Name='ID' /></OrderBy>"; 

		internal readonly static string CssTagTemplate = "<link href=\"{0}\" rel=\"stylesheet\" type=\"text/css\" />";
		internal readonly static string ScriptTagTemplate = "<script src=\"{0}\" type=\"text/javascript\" ></script>";

		internal readonly static string AssemblyFullName =  System.Reflection.Assembly.GetExecutingAssembly().FullName;
	}
}