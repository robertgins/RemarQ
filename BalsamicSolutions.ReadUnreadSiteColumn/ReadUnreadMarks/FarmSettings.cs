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
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// the shared configuration settings for the read unread marking system
	/// </summary>
	public class FarmSettings : SPPersistedObject
	{
		#region Fields & Declarations
		
		internal readonly static Guid SettingsId = new Guid("{21DBC2A6-54F5-403C-AB77-1F1A32587A37}");
		
		static bool _RetrieveRunning = false;
		static FarmSettings _Settings = null;
		readonly static object _LockProxy = new object();
		const int MAX_LIFE_IN_MINUTES = 5;
		
		DateTime LastRefresh { get; set; }
		
		bool _IsOk = false;
		
		bool _NeedsImpersonation = false;
		
		[Persisted]
		private string _SqlConnectionString = string.Empty;
		
		[Persisted]
		private string _FarmAdmin = string.Empty;
		
		[Persisted]
		private bool _TestedOk = false;
		
		[Persisted]
		private bool _ExternalJobManager = false;
		
		[Persisted]
		private bool _TrackDocuments = false;
		
		[Persisted]
		private bool _UpgradeTrackDocuments = false;
		
		[Persisted]
		private string _JQueryPath = "http://kendo.cdn.telerik.com/2014.2.716/js/jquery.min.js";//"/" + SPUtility.ContextLayoutsFolder + "/BalsamicSolutions.ReadUnreadSiteColumn/kendo/scripts/jquery.js";
		
		[Persisted]
		private string _KendoJavasScriptPath="http://kendo.cdn.telerik.com/2014.2.716/js/kendo.ui.core.min.js";// "/" + SPUtility.ContextLayoutsFolder + "/BalsamicSolutions.ReadUnreadSiteColumn/kendo/scripts/kendo.core.js";
		
		[Persisted]
		private string _KendoStyleCommonPath ="http://kendo.cdn.telerik.com/2014.2.716/styles/kendo.common.min.css";// "/" + SPUtility.ContextLayoutsFolder + "/BalsamicSolutions.ReadUnreadSiteColumn/kendo/css/kendo.common.core.css";
		
		[Persisted]
		private string _KendoStyleThemePath ="http://kendo.cdn.telerik.com/2014.2.716/styles/kendo.default.min.css";// "/" + SPUtility.ContextLayoutsFolder + "/BalsamicSolutions.ReadUnreadSiteColumn/kendo/css/kendo.default.css";
		
		[Persisted]
		private int _MinJavascriptClientRefreshInterval = 0;
		
		[Persisted]
		bool _Activated = false;
		
		[Persisted]
		int _MinSizeOfLargeList = 2000;
		
		[Persisted]
		int _BatchSize = 10;
		[Persisted]
		int _LargeBatchSize = 2;
		
		private string _EncodeKey = null;
		
		#endregion
		
		#region CTORs
		
		public FarmSettings(string name, SPPersistedObject parent)
			: base(name, parent)
		{
			this.LastRefresh = DateTime.MinValue;
		}
		
		public FarmSettings(string name, SPPersistedObject parent, Guid id)
			: base(name, parent, id)
		{
			this.LastRefresh = DateTime.MinValue;
		}
		
		public FarmSettings()
			: this("ReadUnreadGlobalSettings", SPFarm.Local, FarmSettings.SettingsId)
		{
			this.LastRefresh = DateTime.MinValue;
		}
		
		#endregion
		
		#region Properties
		
		internal bool ExternalJobManager 
		{
			get { return this._ExternalJobManager; }
			set { this._ExternalJobManager = value; }
		}
		
		internal int BatchSize
		{
			get { return this._BatchSize; }
			set { this._BatchSize = value; }
		}
		
		internal int LargeBatchSize
		{
			get { return this._LargeBatchSize; }
			set { this._LargeBatchSize = value; }
		}
		
		internal int MinSizeOfLargeList
		{
			get { return this._MinSizeOfLargeList; }
			set { this._MinSizeOfLargeList = value; }
		}
		
		internal string FarmAdmin
		{
			get { return this._FarmAdmin; }
			set { this._FarmAdmin = value; }
		}
		
		internal bool Activated
		{
			get { return this._Activated; }
			set { this._Activated = value; }
		}
		
		/// <summary>
		/// path to JQuery for Specialty support
		/// </summary>
		internal string JQueryPath
		{
			get { return this._JQueryPath; }
			set { this._JQueryPath = value; }
		}
		
		/// <summary>
		/// path to Kendo  UI library
		/// </summary>
		internal string KendoJavasScriptPath
		{
			get { return this._KendoJavasScriptPath; }
			set { this._KendoJavasScriptPath = value; }
		}
		
		/// <summary>
		/// path to Kendo  UI Style sheet
		/// </summary>
		internal string KendoStyleCommonPath
		{
			get { return this._KendoStyleCommonPath; }
			set { this._KendoStyleCommonPath = value; }
		}
		
		/// <summary>
		/// path to Kendo  UI Theme Sheet
		/// </summary>
		internal string KendoStyleThemePath
		{
			get { return this._KendoStyleThemePath; }
			set { this._KendoStyleThemePath = value; }
		}
		
		/// <summary>
		/// global value for client side refresh polling
		/// interval in seconds
		/// </summary>
		internal int MinJavascriptClientRefreshInterval
		{
			get { return this._MinJavascriptClientRefreshInterval; }
			set { this._MinJavascriptClientRefreshInterval = value; }
		}
		
		internal string KendoHeader(CultureInfo cultureInfo)
		{
			string cultureName = string.Empty;
			if (null != cultureInfo && null != cultureInfo.TextInfo)
			{
				cultureName = cultureInfo.TextInfo.CultureName;
			}
			return this.KendoHeader(cultureName);
		}
		
		internal string KendoHeader(string cultureName)
		{
			string[] pathParts = this.KendoJavasScriptPath.Split('/');
			int lastPart = pathParts.Length - 1;
			pathParts[lastPart] = "kendo.aspnetmvc.min.js";
			string kendoMvcPath = string.Join("/", pathParts);
			
			StringBuilder returnValue = new StringBuilder();
			returnValue.AppendFormat(Constants.CssTagTemplate, FarmSettings.Settings.KendoStyleCommonPath);
			returnValue.AppendLine();
			returnValue.AppendFormat(Constants.CssTagTemplate, FarmSettings.Settings.KendoStyleThemePath);
			returnValue.AppendLine();
			returnValue.AppendFormat(Constants.ScriptTagTemplate, FarmSettings.Settings.JQueryPath);
			returnValue.AppendLine();
			returnValue.AppendFormat(Constants.ScriptTagTemplate, FarmSettings.Settings.KendoJavasScriptPath);
			returnValue.AppendLine();
			returnValue.AppendFormat(Constants.ScriptTagTemplate, kendoMvcPath);
			returnValue.AppendLine();
			if (!string.IsNullOrWhiteSpace(cultureName))
			{
				pathParts[lastPart] = "cultures";
				string culturesPath = string.Join("/", pathParts);
				string cultureScriptPath = culturesPath + "/kendo.culture." + cultureName + ".min.js";
				returnValue.AppendFormat(Constants.ScriptTagTemplate, cultureScriptPath);
			}
			return returnValue.ToString();
		}
		
		/// <summary>
		/// indicates that the http module for inline document tracking is installed
		/// </summary>
		internal bool TrackDocuments
		{
			get { return this._TrackDocuments; }
			set { this._TrackDocuments = value; }
		}
		
		/// <summary>
		/// indicates that the http module for inline document tracking is installed
		/// </summary>
		internal bool UpgradeTrackDocuments
		{
			get { return this._UpgradeTrackDocuments; }
			set { this._UpgradeTrackDocuments = value; }
		}
		
		#region Connection string encryption
		
		static string HexEncode(byte[] encodeTheseBytes)
		{
			StringBuilder returnValue = new StringBuilder(encodeTheseBytes.Length * 2);
			for (int byteIdx = 0; byteIdx < encodeTheseBytes.Length; byteIdx++)
			{
				returnValue.Append(encodeTheseBytes[byteIdx].ToString("X2", CultureInfo.InvariantCulture));
			}
			return returnValue.ToString();
		}
		
		string EncodeKey
		{
			get
			{
				if (null == this._EncodeKey)
				{
					byte[] keyBytes = Assembly.GetExecutingAssembly().GetName().GetPublicKey();
					this._EncodeKey = HexEncode(keyBytes);
				}
				return this._EncodeKey;
			}
		}
		
		string KeyFromAssembly(Assembly keySource)
		{
			byte[] keyBytes = keySource.GetName().GetPublicKey();
			return HexEncode(keyBytes);
		}
		
		string DecodeString(System.Reflection.Assembly callingAssembly, string rawText)
		{
			if (!string.IsNullOrWhiteSpace(rawText))
			{
				try
				{
					rawText = Utilities.DecryptString(rawText, this.KeyFromAssembly(callingAssembly));
				}
				catch (System.FormatException fmtError)
				{
					//all of this logging is just for MSOCAF
					RemarQLog.TraceError(fmtError);
					rawText = null;
				}
				catch (System.Text.DecoderFallbackException decError)
				{
					//all of this logging is just for MSOCAF
					RemarQLog.TraceError(decError);
					rawText = null;
				}
				catch (System.ArgumentOutOfRangeException argEx)
				{
					//all of this logging is just for MSOCAF
					RemarQLog.TraceError(argEx);
					rawText = null;
				}
				catch (System.ArgumentException argError)
				{
					//all of this logging is just for MSOCAF
					RemarQLog.TraceError(argError);
					rawText = null;
				}
				catch (System.Security.SecurityException secError)
				{
					//all of this logging is just for MSOCAF
					RemarQLog.TraceError(secError);
					rawText = null;
				}
				catch (System.Security.Cryptography.CryptographicException cryptoError)
				{
					//all of this logging is just for MSOCAF
					RemarQLog.TraceError(cryptoError);
					rawText = null;
				}
			}
			return rawText;
		}
		
		private string EncodeString(string encodeThis)
		{
			string returnValue = string.Empty;
			if (!string.IsNullOrWhiteSpace(encodeThis))
			{
				returnValue = Utilities.EncryptString(encodeThis, this.EncodeKey);
			}
			return returnValue;
		}
		
		#endregion
		
		/// <summary>
		/// the sql connection string used to connect to the read/unread marking database
		/// </summary>
		internal string SqlConnectionString
		{
			get { return this.DecodeString(Assembly.GetCallingAssembly(), this._SqlConnectionString); }
			set 
			{
				if (value != null)
				{
					value = value.Replace("\r", string.Empty).Replace("\n", string.Empty);
					if (string.IsNullOrWhiteSpace(value))
					{
						this._SqlConnectionString = string.Empty;
					}
					else
					{
						this._SqlConnectionString = this.EncodeString(value);
					}
				}
				else
				{
					this._SqlConnectionString = string.Empty;
				}
				SqlRemarQ.DropConnectionStringCache();
			}
		}
		
		/// <summary>
		/// indicates that the connection string was tested at least once
		/// </summary>
		internal bool TestedOk
		{
			get { return this._TestedOk; }
			set { this._TestedOk = value; }
		}
		
		/// <summary>
		/// indicates that the global table exits and that the current connection string is valid
		/// </summary>
		internal bool IsOk
		{
			get { return this._IsOk; }
			set { this._IsOk = value; }
		}
		
		/// <summary>
		/// indicates that the connection string does not have explicit credentials and that the
		/// app domain account must be impersonated during SQL calls to our read unread database
		/// </summary>
		internal bool NeedsImpersonation
		{
			get { return this._NeedsImpersonation; }
			set { this._NeedsImpersonation = value; }
		}
		
		#endregion
		
		#region Static methods to retrieve settings
		
		internal static AssemblyInformationalVersionAttribute InformationVersion
		{
			get
			{
				AssemblyInformationalVersionAttribute infoAttribute = (AssemblyInformationalVersionAttribute)Assembly
																													 .GetExecutingAssembly()
																													 .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
																													 .FirstOrDefault();
				return infoAttribute;
			}
		}
		
		/// <summary>
		/// called from the Settings property to update the cached
		/// copy of the ReadUnreadFarmSettings. 
		/// </summary>
		static void BackgroundRetrieve()
		{
			FarmSettings settings = null;
			SPSecurity.CodeToRunElevated runMe = new SPSecurity.CodeToRunElevated(delegate()
			{
				settings = Microsoft.SharePoint.Administration.SPFarm.Local.GetObject(FarmSettings.SettingsId) as FarmSettings;
				if (null == settings)
				{
					settings = new FarmSettings();
				}
				if (!string.IsNullOrEmpty(settings.SqlConnectionString))
				{
					settings.NeedsImpersonation = SqlRemarQ.NeedsImpersonation(settings.SqlConnectionString);
					settings.IsOk = SqlRemarQ.TableExists(settings.SqlConnectionString, Constants.ReadUnreadConfigurationTableName);
				}
				settings.LastRefresh = DateTime.UtcNow;
			});
			SPSecurity.RunWithElevatedPrivileges(runMe);
			lock (_LockProxy)
			{
				_Settings = settings;
				_RetrieveRunning = false;
				SqlRemarQ.DropConnectionStringCache();
			}
		}
		
		/// <summary>
		/// invalidate the current cached license
		/// </summary>
		internal static void ExpireCachedObject()
		{
			lock (_LockProxy)
			{
				SPPersistedObject settingsObj = Microsoft.SharePoint.Administration.SPFarm.Local.GetObject(FarmSettings.SettingsId);
				if (null != settingsObj)
				{
					settingsObj.Uncache();
				}
				if (null != _Settings)
				{
					_Settings = null;
				}
				SqlRemarQ.DropConnectionStringCache();
			}
		}
		
		#if DEBUG
		internal static FarmSettings DebugSettings(string sqlConnectionString)
		{
			_Settings = new FarmSettings();
			_Settings.SqlConnectionString = sqlConnectionString;
			_Settings.NeedsImpersonation = SqlRemarQ.NeedsImpersonation(_Settings.SqlConnectionString);
			_Settings.IsOk = SqlRemarQ.TableExists(_Settings.SqlConnectionString, Constants.ReadUnreadConfigurationTableName);
			_Settings.LastRefresh = DateTime.UtcNow.AddMinutes(MAX_LIFE_IN_MINUTES * 2);
			SqlRemarQ.DropConnectionStringCache();
			return _Settings;
		}
		
		#endif
		
		/// <summary>
		/// delete the farm settings object
		/// </summary>
		public static void Remove()
		{
			SPPersistedObject settingsObj = Microsoft.SharePoint.Administration.SPFarm.Local.GetObject(FarmSettings.SettingsId);
			if (null != settingsObj)
			{
				settingsObj.Delete();
				settingsObj.Unprovision();
				settingsObj.Uncache();
			}
		}
		
		/// <summary>
		/// get the Settings information object
		/// </summary>
		internal static FarmSettings Settings
		{
			get
			{
				lock (_LockProxy)
				{
					if (null == _Settings || _Settings.LastRefresh.AddMinutes(MAX_LIFE_IN_MINUTES) < DateTime.UtcNow)
					{
						if (null == _Settings)
						{
							SPSecurity.CodeToRunElevated runMe = new SPSecurity.CodeToRunElevated(delegate()
							{
								_Settings = Microsoft.SharePoint.Administration.SPFarm.Local.GetObject(FarmSettings.SettingsId) as FarmSettings;
								if (null == _Settings)
								{
									_Settings = new FarmSettings();
								}
								if (!string.IsNullOrEmpty(_Settings.SqlConnectionString))
								{
									_Settings.NeedsImpersonation = SqlRemarQ.NeedsImpersonation(_Settings.SqlConnectionString);
									_Settings.IsOk = SqlRemarQ.TableExists(_Settings.SqlConnectionString, Constants.ReadUnreadConfigurationTableName);
								}
								_Settings.LastRefresh = DateTime.Now;
							});
							SPSecurity.RunWithElevatedPrivileges(runMe);
							SqlRemarQ.DropConnectionStringCache();
						}
						else
						{
							if (!_RetrieveRunning)
							{
								_RetrieveRunning = true;
								Task.Factory.StartNew(() =>
								{
									BackgroundRetrieve();
								});
							}
						}
					}
					return _Settings;
				}
			}
		}
		
		#endregion
	}
}