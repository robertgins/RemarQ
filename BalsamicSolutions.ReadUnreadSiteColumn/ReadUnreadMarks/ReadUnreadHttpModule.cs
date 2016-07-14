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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// this httpmodule takes care of marking document library items Read
	/// when a user reads them from the server it has an async queue 
	/// that runs on a separate thread so that the actual server 
	/// performance is not impacted dramatically 
	/// </summary>
	public class ReadUnreadHttpModule : IHttpModule
	{ 
		readonly List<string> _RootPaths = new List<string>();
		readonly List<string> _ContextPaths = new List<string>();
		readonly object _LockProxy = new object();
		bool _Initialized = false;
		static readonly char[] _SemiColonDelimiter = new char[] { ';' };
 
		public void Init(HttpApplication httpApp)
		{
			//if (!System.Diagnostics.Debugger.IsAttached)
			//{
			//	System.Diagnostics.Debugger.Break();
			//}
			if (null == httpApp)
			{
				throw new ArgumentNullException("httpApp");
			}
			//These are our well known resource files and paths that
			//we should not even try to process
			this._RootPaths.Add(("/scriptresource.axd").ToUpperInvariant());
			this._RootPaths.Add(("/controltemplates.axd").ToUpperInvariant());
			this._RootPaths.Add(("/webresource.axd").ToUpperInvariant());
			this._RootPaths.Add(("/_").ToUpperInvariant()); // gets _app_bin, _layouts, _controltemplates, _login, _vti_bin , _windows , _wpresources, _catalogs
			this._RootPaths.Add(("/app_browsers").ToUpperInvariant());
			this._RootPaths.Add(("/app_globalresources").ToUpperInvariant());
			this._RootPaths.Add(("/bin").ToUpperInvariant());
			this._RootPaths.Add(("/clientaccesspolicy.xml").ToUpperInvariant());
			this._RootPaths.Add(("/crossdomain.xml").ToUpperInvariant());
			this._RootPaths.Add(("/defaultwsdlhelpgenerator.aspx").ToUpperInvariant());
			this._RootPaths.Add(("/wpresources").ToUpperInvariant());
			this._RootPaths.Add(("wpresources.axd").ToUpperInvariant());
			this._RootPaths.Add(("scriptresource.axd").ToUpperInvariant());
			this._RootPaths.Add(("controltemplates.axd").ToUpperInvariant());

			this._ContextPaths.Add(("/_app_bin/").ToUpperInvariant());
			this._ContextPaths.Add(("/_vti_bin/").ToUpperInvariant());
			this._ContextPaths.Add(("/_vti_pvt/").ToUpperInvariant());

			httpApp.PreSendRequestContent += this.OnPreSendRequestContentDebug;
			httpApp.BeginRequest += this.HttpApp_BeginRequest;
			httpApp.EndRequest += this.HttpApp_EndRequest;
		}

		void HttpApp_BeginRequest(object sender, EventArgs e)
		{
			this.InitializeContextPaths();	
			HttpApplication httpApp = sender as HttpApplication;
			if (null != httpApp)
			{
				HttpContext httpCtx = httpApp.Context;
				
				if (httpCtx.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
				{
					string soapAction = httpCtx.Request.Headers["SOAPAction"];
					if (!string.IsNullOrEmpty(soapAction))
					{
						soapAction = soapAction.ToLowerInvariant().Trim(new char[] { '"', '\'' });
						if (soapAction.Equals("http://schemas.microsoft.com/sharepoint/soap/icellstorages/executecellstoragerequest"))
						{
							string urlPath = httpCtx.Request.Url.AbsolutePath.ToLowerInvariant();
							if (urlPath.Equals("/_vti_bin/cellstorage.svc/cellstorageservice"))
							{
								if (httpCtx.Request.ContentType.ToLower().Contains("application/xop+xml") &&
									httpCtx.Request.ContentLength > 0 && httpCtx.Request.ContentLength < 4096)
								{
									//We only want the soap envelope for the first request to open
									//the file, not the subsequent requests to update or save
									//the file , (thats why we have the size restriction). 
									//For some reason a HttpContext.Request.Filter does not work
									//I assume its a SharePoint thing because the Request.InputStream
									//is seekable so I can copy what I want.
									SoapEnvelopeParser soapParser = new SoapEnvelopeParser(httpCtx.Request.InputStream);
									httpCtx.Items["RemarQ"] = soapParser;
								}
							}
						}
					}
				}
			}
		}

		void OnPreSendRequestContentDebug(object sender, EventArgs e)
		{
			try
			{
				this.OnPreSendRequestContent(sender, e);
			}
			catch (Exception couldBeAnything)
			{
				//there are lots of circumstances that could cause this,
				//mostly a request for content "outside" of SharePoint storage
				RemarQLog.LogError("Unexpected error accessing requst context in OnPreSendRequestContentDebug", couldBeAnything);
				throw;
			}
		}

		void InitializeContextPaths()
		{
			//We cant pick these up until the app has been hit at least once
			//so we will do it now
			if (!this._Initialized)
			{
				lock (this._LockProxy)
				{
					if (!this._Initialized)
					{
						this._Initialized = true;
						string contextPath = "/" + SPUtility.ContextControlTemplatesFolder;
						this._ContextPaths.Add(contextPath.ToUpperInvariant());
						contextPath = "/" + SPUtility.ContextLayoutsFolder;
						this._ContextPaths.Add(contextPath.ToUpperInvariant());
						contextPath = "/" + SPUtility.ContextImagesRoot;
						this._ContextPaths.Add(contextPath.ToUpperInvariant());
					}
				}
			}
		}
	 
		void OnPreSendRequestContent(object sender, EventArgs e)
		{
			//We only care if this is an authenticated GET on document library  
			//In theory this module is only active if the correct configuration
			//conditions exist so we wont check the WriteDirect or DocumentLibrary
			if (FarmSettings.Settings.IsOk)
			{
				HttpApplication httpApp = sender as HttpApplication;
				if (null != httpApp)
				{
					HttpContext httpCtx = httpApp.Context;
					if (null != httpCtx.User && null != httpCtx.User.Identity && httpCtx.User.Identity.IsAuthenticated)
					{
						if (null != httpCtx && null != httpCtx.Request && null != httpCtx.Response)
						{
							if (httpCtx.Response.StatusCode == 200)
							{
								if (httpCtx.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
								{
									//Ok we are authenticated and the response is a valid file GET request
									SPListItem fileItem = this.TryGetListItemForFileFromContext(httpCtx);
									if (null != fileItem)
									{
										ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(fileItem.ParentList.ID);
										if (null != listConfig)
										{
											//Ok its a valid read unread library list so if we can figure out who the user is we will 
											//try to mark the item read 
											SPUser currentUser = fileItem.ParentList.ParentWeb.CurrentUser;
											if (null != currentUser)
											{
												SharePointRemarQ.MarkRead(fileItem, currentUser);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		 
		/// <summary>
		/// this is where we pickup the end of an Office document
		/// SOAP call
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void HttpApp_EndRequest(object sender, EventArgs e)
		{ 
			HttpApplication httpApp = sender as HttpApplication;
			if (null != httpApp)
			{
				HttpContext httpCtx = httpApp.Context;
				if (null != httpCtx.Items["RemarQ"])
				{
					this.ProcessPossibleOfficeSoapClientRequest(httpCtx);
				}
			}
		}

		/// <summary>
		/// Office documents use SOAP (not rest) to get thier content
		/// if this is one of those we need to decode it
		/// </summary>
		/// <param name="httpCtx"></param>
		internal void ProcessPossibleOfficeSoapClientRequest(HttpContext httpCtx)
		{
			SoapEnvelopeParser soapParser = httpCtx.Items["RemarQ"] as SoapEnvelopeParser;
			if (null != soapParser)
			{
				string soapRequest = soapParser.SoapEnvelope;
				if (!string.IsNullOrEmpty(soapRequest))
				{
					try
					{
						XmlDocument xmlDoc = new XmlDocument();
						xmlDoc.LoadXml(soapRequest);
						XmlNamespaceManager soapNs = new XmlNamespaceManager(xmlDoc.NameTable);
						soapNs.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");
						soapNs.AddNamespace("m", "http://schemas.microsoft.com/sharepoint/soap/");
						XmlNode bodyNode = xmlDoc.SelectSingleNode("s:Envelope/s:Body", soapNs);
						if (null != bodyNode)
						{
							//only process if we have one request for one file to download (not a sync of multiple)
							XmlNodeList serviceRequests = bodyNode.SelectNodes("m:RequestCollection/m:Request", soapNs);
							if (null != serviceRequests && serviceRequests.Count == 1)
							{
								//check for Cell request in the sub request and a Success in the response
								XmlNode cellRequest = serviceRequests[0].SelectSingleNode("m:SubRequest[@Type='Cell']", soapNs);
								if (null != cellRequest)
								{
									//we really should check the response for ErrorCode="Success" HResult="0"
									XmlAttribute urlAttribute = serviceRequests[0].Attributes["Url"];
									if (null != urlAttribute)
									{
										SPUser currentUser = null;
										SPListItem fileItem = this.TryGetListItemForFileFromOfficeUrl(urlAttribute.Value, out currentUser);
										if (null != fileItem)
										{
											ListConfiguration listConfig = ListConfigurationCache.GetListConfiguration(fileItem.ParentList.ID);
											if (null != listConfig)
											{
												if (null != currentUser && !currentUser.LoginName.IsNullOrEmpty())
												{
													SharePointRemarQ.MarkRead(fileItem, currentUser);
												}
											}
										}
									}
								}
							}
						}
					}
					catch (XmlException)
					{
					}
				}
			}
		}

		bool IsExcludedPath(string pathToCheck)
		{
			//While this seems cheesy , there does not seem to be a solid api
			//to determine if the current path is in scope  SPContext.Current.ListItem
			//and SPContext.Current.File are usually null for direct paths 
			//to  documents or pictures but get file still seems to work 
			//which is what we want, so  we need to filter out all the
			//paths we should skip , for now a string check is the simplest
			string normalizedFilePath = pathToCheck.ToUpperInvariant();
			foreach (string publicPath in this._RootPaths)
			{
				if (normalizedFilePath.StartsWith(publicPath, StringComparison.Ordinal))
				{
					return true;
				}
			}
			foreach (string contextPath in this._ContextPaths)
			{
				if (normalizedFilePath.Contains(contextPath))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// we dont care about embeded CSS, html or javascript
		/// TODO : this may be a problem for HTML pages stored in a library
		/// but only if the ReadUnread field is on the list
		/// </summary>
		/// <param name="contentType"></param>
		/// <returns></returns>
		static bool IsIgnoredContentType(string contentType)
		{
			string[] contentTypeParts = contentType.Split(_SemiColonDelimiter, StringSplitOptions.RemoveEmptyEntries);
			foreach (string possibleType in contentTypeParts)
			{
				if (possibleType.Trim().StartsWith("text/", StringComparison.OrdinalIgnoreCase))
				{
					if (contentType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// attempt to translate the request url from SOAP to a 
		/// file item
		/// </summary>
		/// <param name="fileUrl"></param>
		/// <param name="webUser"></param>
		/// <returns></returns>
		SPListItem TryGetListItemForFileFromOfficeUrl(string fileUrl, out SPUser webUser)
		{
			webUser = null;
			SPListItem returnValue = null;
			try
			{
				using (SPSite docSite = new SPSite(fileUrl))
				{
					using (SPWeb docWeb = docSite.OpenWeb())
					{
						SPListItem mightBeAFile = docWeb.GetReadUnreadItemByUrl(fileUrl);
						if (null != mightBeAFile)
						{
							//even if we have a file item, we dont care unless it has 
							//our field on it
							if (mightBeAFile.Fields.ContainsField(Constants.ReadUnreadFieldName))
							{
								if (mightBeAFile.IsFile())
								{
									returnValue = mightBeAFile;
									webUser = docWeb.CurrentUser;
								}
							}
						}
					}
				}
			}
			catch (SPException couldBeAnything)
			{
				RemarQLog.LogError("Unexpected error accessing requst context in TryGetListItemForFileFromOfficeUrl", couldBeAnything);
				returnValue = null;
			}
			return returnValue;
		}

		/// <summary>
		/// try to get the list item from the url
		/// </summary>
		/// <param name="httpCtx"></param>
		/// <returns></returns>
		SPListItem TryGetListItemForFileFromContext(HttpContext httpCtx)
		{
			SPListItem returnValue = null;
			string requestPath = httpCtx.Request.Path;
			string contentType = httpCtx.Response.ContentType;
			if (!requestPath.IsNullOrEmpty() && !contentType.IsNullOrEmpty() && !this.IsExcludedPath(requestPath) && !IsIgnoredContentType(contentType))
			{
				try
				{
					SPContext currentContext = SPContext.GetContext(httpCtx);
					if (null != currentContext && null != currentContext.Web && null != currentContext.Web.CurrentUser)
					{
						//get this via Id with only our properties using getlistitem fields
						SPListItem mightBeAFile = currentContext.Web.GetReadUnreadItemByUrl(requestPath);
						if (null != mightBeAFile)
						{
							//even if we have a file item, we dont care unless it has 
							//our field on it
							if (mightBeAFile.Fields.ContainsField(Constants.ReadUnreadFieldName))
							{
								if (mightBeAFile.IsFile())
								{
									returnValue = mightBeAFile;
								}
							}
						}
					}
				}
				catch (SPException couldBeAnything)
				{
					//there are lots of circumstances that could cause this,
					//mostly a request for content "outside" of SharePoint storage
					RemarQLog.LogError("Unexpected error accessing requst context in TryGetListItemForFileFromContext", couldBeAnything);
					returnValue = null;
				}
			}
			return returnValue;
		}

		public void Dispose()
		{
			//Nothing to do
		}

		/// <summary>
		/// this is the actual webconfig entry we use
		/// </summary>
		readonly static SPWebConfigModification _HttpModuleModification = new SPWebConfigModification()
																			  {
																				  Owner = Constants.ProductName,
																				  Name = "add[@name='" + Constants.ProductName + "']",
																				  Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,
																				  Path = "configuration/system.webServer/modules",
																				  Sequence = 0,
																				  Value = "<add name=\"" + Constants.ProductName + "\"  type=\"BalsamicSolutions.ReadUnreadSiteColumn.ReadUnreadHttpModule," +
																						  Constants.AssemblyFullName + "\" />"
																			  };

		/// <summary>
		/// Unregister our module from the farm configuration
		/// </summary>
		public static void UnregisterModule(bool waitUntilDeployed)
		{
			if (IsRegistered())
			{
				SPWebService webService = SPWebService.ContentService;
				bool applyUpdate = false;
				System.Collections.ObjectModel.Collection<SPWebConfigModification> modificationCollection = webService.WebConfigModifications;
				int modificationCount = modificationCollection.Count;
				//walk backwards after preserving count because we will modify the
				//collection during this loop
				for (int countIndex = modificationCount - 1; countIndex > -1; countIndex--)
				{
					if ((modificationCollection[countIndex].Owner == Constants.ProductName))
					{
						SPWebConfigModification foundModification = modificationCollection[countIndex];
						modificationCollection.Remove(foundModification);
						applyUpdate = true;
					}
				}
				if (applyUpdate)
				{
					webService.Update();

					WaitForWebConfigJobsToComplete();
					webService.ApplyWebConfigModifications();
				}
				if(waitUntilDeployed)
				{
					WaitForWebConfigJobsToComplete();
				}
			}
		}

		/// <summary>
		/// wait for any job-webconfig-modification  jobs to complete and "delete" themselves
		/// this behaviour is unique to the job-webconfig-modification
		/// we could wait on the title but thats different based on language
		/// </summary>
		public static void WaitForWebConfigJobsToComplete()
		{
			string timerJobName = "job-webconfig-modification";
			//"Windows SharePoint Services Web.Config Update"
			//Utilities.WaitForOnetimeJobToFinish(SPWebService.ContentService,timerJobName,240);
			int ticCount = 240;
			while(ticCount>0 && Utilities.IsJobDefined(SPWebService.ContentService,timerJobName))
			{
				Thread.Sleep(500);
				ticCount --;
				Thread.Sleep(500);
			}
		}

		
		/// <summary>
		/// determine if the module is registered
		/// </summary>
		/// <returns></returns>
		public static bool IsRegistered()
		{
			bool returnValue = false;
			SPWebService webService = SPWebService.ContentService;
			System.Collections.ObjectModel.Collection<SPWebConfigModification> modificationCollection = webService.WebConfigModifications;
			int modificationCount = modificationCollection.Count;
			//walk backwards after preserving count because we will modify the
			//collection during this loop
			for (int countIndex = modificationCount - 1; countIndex > -1; countIndex--)
			{
				if ((modificationCollection[countIndex].Owner == Constants.ProductName))
				{
					returnValue = true;
					break;
				}
			}
			return returnValue;
		}

		/// <summary>
		/// register our web config registration
		/// </summary>
		public static void RegisterModule(bool waitUntilDeployed)
		{
			if (!IsRegistered())
			{
				if (FarmLicense.License.LicenseMode >= LicenseModeType.Professional && FarmLicense.License.IsLicensed())
				{
					SPWebService webService = SPWebService.ContentService;
					SPWebConfigModification configMod = new SPWebConfigModification("appSettings", "configuration");
					configMod.Owner = Constants.ProductName;
					configMod.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureSection;
					webService.WebConfigModifications.Add(configMod);
					webService.WebConfigModifications.Add(_HttpModuleModification);
					webService.Update();
					WaitForWebConfigJobsToComplete();
					webService.ApplyWebConfigModifications();
				}
				if(waitUntilDeployed)
				{
					WaitForWebConfigJobsToComplete();
				}
			}
		}
	}
}