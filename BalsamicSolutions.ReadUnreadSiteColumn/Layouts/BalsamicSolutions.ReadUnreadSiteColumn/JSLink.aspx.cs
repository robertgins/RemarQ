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
using System.IO;
using System.Reflection;
using System.Web;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace BalsamicSolutions.ReadUnreadSiteColumn.Layouts.BalsamicSolutions.ReadUnreadSiteColumn
{
	public partial class JSLink : UnsecuredLayoutsPageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			this.Response.Clear();
			this.Response.ClearContent();
			this.Response.ClearHeaders();
			this.Response.ContentType = "text/javascript";
		 
			bool isAnonymous = this.IsAnonymous;
			if (isAnonymous)
			{
				this.Response.Expires = -1;
				this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
				this.Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
			}
			 
			//check for a remarQId request first which is from
			//a threaded discussion
			string threadIdQS = this.Request.QueryString["remarQId"];
			if (!string.IsNullOrEmpty(threadIdQS) && threadIdQS.Length > 77)
			{ //{2E913A2D-1E43-4784-B7B3-BE1DE61978D4}-{69385851-9CCC-43C3-987E-2EBD6E9B511C}-1
				Guid listId = Guid.Parse(threadIdQS.Substring(0, 38));
				Guid viewId = Guid.Parse(threadIdQS.Substring(39, 38));
				int userId = int.Parse(threadIdQS.Substring(78));
 
				if (!isAnonymous)
				{
					this.ProcessThreadedDiscussionViewRequest(listId, viewId, userId);
				}
			}
			else
			{
				//All this decoding is because someone on the SharePoint team thought that they
				//needed to HTML encode the JSLink paramater of a custom field.
 				string base64Qs = this.Request.Url.Query;
 				if (base64Qs.StartsWith("?", StringComparison.OrdinalIgnoreCase))
				{
					base64Qs = base64Qs.Substring(1);
				}
				if (!base64Qs.IsNullOrWhiteSpace())
				{
					this.ProcessRemarQViewRequest(base64Qs, isAnonymous);
				}
			}
			this.Response.StatusCode = 200;
			HttpContext.Current.ApplicationInstance.CompleteRequest();
		}

		/// <summary>
		/// process a request from a standard JsLink generated Url
		/// </summary>
		/// <param name="base64Qs"></param>
		/// <param name="isAnonymous"></param>
		void ProcessRemarQViewRequest(string base64Qs, bool isAnonymous)
		{
			//RegisterModuleInit needs a path relative to _layouts
			string registerPath = MakeLayoutsRelativePath(this.Request.Url.PathAndQuery);
			string queryData = System.Text.UnicodeEncoding.Unicode.GetString(Convert.FromBase64String(base64Qs));
			string[] queryParts = queryData.Split('\t');
			if (queryParts.Length >= 4)
			{
				string listIdQS = queryParts[0];
				string viewTypeQS = queryParts[1];
				string cacheControlQS = queryParts[2];
				string isDiscussionQS = queryParts[3];
				bool isDiscussion = bool.Parse(isDiscussionQS);

				if (viewTypeQS.ToUpperInvariant() == "HTML" || viewTypeQS.ToUpperInvariant() == "GRID")
				{
					if (!listIdQS.IsNullOrEmpty() && !cacheControlQS.IsNullOrEmpty())
					{
						Guid listId = new Guid(listIdQS);
						if (Guid.Empty != listId)
						{
							ListConfiguration listInfo = ListConfigurationCache.GetListConfiguration(listId);
							if (null != listInfo)
							{
								if (isDiscussion)
								{
									if (isAnonymous)
									{
										this.Response.Write(string.Empty);
									}
									else
									{
										string templateName = string.Empty;
										if (queryParts.Length > 4)
										{
											templateName = queryParts[4];
										}

										this.EmitJavaScript(listInfo.JSLinkDiscussionViewJavaScript(registerPath, templateName));
									}
								}
								else
								{
									if (isAnonymous)
									{
										this.EmitJavaScript(listInfo.JSLinkJavaScriptAnonymous(registerPath));
									}
									else
									{
										this.EmitJavaScript(listInfo.JSLinkJavaScript(registerPath));
									}
								}
							}
						}
					}
				}
			}
		}
 
		/// <summary>
		/// since we are writing javascript we cant pass an AntiXss analysis
		/// unless we write binary, so we will do the converation and write it
		/// out that way, its ok thats what happens underthe covers anyway
		/// </summary>
		/// <param name="javaScript"></param>
		void EmitJavaScript(string javaScript)
		{
			char[] resultCharArray = new char[javaScript.Length];
			javaScript.CopyTo(0, resultCharArray, 0, javaScript.Length);
			this.Response.Write(resultCharArray,0,resultCharArray.Length);
		}

		/// <summary>
		/// process a request from an Xsl embeded JSLink 
		/// </summary>
		/// <param name="listId"></param>
		void ProcessThreadedDiscussionViewRequest(Guid listId, Guid viewId, int userId)
		{
			if (Guid.Empty != listId)
			{
				ListConfiguration listInfo = ListConfigurationCache.GetListConfiguration(listId);
				if (null != listInfo)
				{
					string registerPath = MakeLayoutsRelativePath(this.Request.Url.PathAndQuery);
					this.EmitJavaScript(listInfo.JSLinkThreadedDiscussionViewJavaScript(registerPath, viewId, userId));
				}
			}
		}

		bool IsAnonymous
		{
			get
			{
				HttpContext httpCtx = HttpContext.Current;
				if (null == httpCtx)
				{
					return true;
				}
				try
				{
					SPContext currentContext = SPContext.GetContext(httpCtx);
					if (null == currentContext)
					{
						return true;
					}
					if (null == currentContext.Web)
					{
						return true;
					}
					if (null == currentContext.Web.CurrentUser)
					{
						return true;
					}
				}
				catch (SPException)
				{
					return true;
				}
				return false;   
			}
		}

		protected override bool AllowAnonymousAccess
		{
			get { return true; }
		}

		protected override bool AllowNullWeb
		{
			get { return false; }
		}

		static string MakeLayoutsRelativePath(string registerPath)
		{
			string layoutsRoot = "/" + SPUtility.ContextLayoutsFolder;
			if (registerPath.StartsWith(layoutsRoot, StringComparison.OrdinalIgnoreCase))
			{
				int posWhack = registerPath.IndexOf("/", 10, StringComparison.OrdinalIgnoreCase);
				if (posWhack > -1)
				{
					posWhack++;
					if (posWhack < registerPath.Length)
					{
						registerPath = registerPath.Substring(posWhack);
					}
				}
			}
			else if (registerPath.StartsWith("/_layouts", StringComparison.OrdinalIgnoreCase))
			{
				int posWhack = registerPath.IndexOf("/", 2, StringComparison.OrdinalIgnoreCase);
				if (posWhack > -1)
				{
					posWhack++;
					if (posWhack < registerPath.Length)
					{
						registerPath = registerPath.Substring(posWhack);
					}
				}
			}
			return registerPath;
		}
	}
}