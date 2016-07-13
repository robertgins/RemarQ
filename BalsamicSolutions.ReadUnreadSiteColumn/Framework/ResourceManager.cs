// -----------------------------------------------------------------------------
//  Copyright 8/8/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml;
using Microsoft.SharePoint;

namespace BalsamicSolutions.ReadUnreadSiteColumn.Framework
{
	/// <summary>
	/// gets a cachable resource manager for 
	/// language strings,derived from the
	/// ResourceManager class in BalsamicSolutions.Utilities
	/// </summary
	public class ResourceManager
	{
		const string DEFAULT_CULTURE = "EN";
		public const int DEFAULT_CACHE_LIFE_IN_MINUTES = 5;
		 
		DateTime _NextRefresh = DateTime.MinValue;
 
		readonly object _LockProxy = new object();
		readonly string _DefaultCulture = DEFAULT_CULTURE;
		int _CacheIntervalInMinutes = DEFAULT_CACHE_LIFE_IN_MINUTES;

		readonly Dictionary<string, string> _Cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public ResourceManager()
		{
			//Start with the default Resource File prompts in case the 
			//culture table is not initialized
			Assembly thisAssm = Assembly.GetExecutingAssembly();
			string[] resFileNames = thisAssm.GetManifestResourceNames();
			foreach (string resFileName in resFileNames)
			{
				if (resFileName.EndsWith("Resources.xml", StringComparison.OrdinalIgnoreCase))
				{
					XmlDocument xmldoc = new XmlDocument();
					string flatText = Utilities.GetEmbeddedResourceString(resFileName);
					xmldoc.LoadXml(flatText);
					XmlNode rootNode = xmldoc.SelectSingleNode("Resources");
					if (null != rootNode)
					{
						string cultureName = rootNode.Attributes["culture"].Value;

						foreach (XmlNode resNode in xmldoc.SelectNodes("Resources/Resource"))
						{
							string resName = resNode.Attributes["Name"].Value;
							string resValue = resNode.InnerText;
						 
							string keyName = cultureName.ToStandardCultureNameFormat() + ":" + resName.ToCamelCase();
							if (!this._Cache.ContainsKey(keyName))
							{
								this._Cache.Add(keyName, resValue);
							}
						}
					}
				}
			}
		}

		public int CacheIntervalInMinutes
		{
			get
			{
				lock (this._LockProxy)
				{
					return this._CacheIntervalInMinutes;
				}
			}
			set
			{
				lock (this._LockProxy)
				{
					if (value != this._CacheIntervalInMinutes)
					{
						this._NextRefresh = DateTime.MinValue;
					}
					this._CacheIntervalInMinutes = value;
				}
				this.UpdateCacheInternal();
			}
		}

		void ResetCacheInternal()
		{
			lock (this._LockProxy)
			{
				this._NextRefresh = DateTime.MinValue;
			}
			this.UpdateCacheInternal();
		}

		string[] CultureNamesInternal()
		{
			HashSet<string> returnValue = new HashSet<string>();
			this.UpdateCacheInternal();
			lock (this._LockProxy)
			{
				foreach (string keyValue in this._Cache.Keys)
				{
					string keyName = keyValue.Split(':')[0];
					returnValue.Add(keyName);
				}
			}
			return returnValue.ToArray();
		}

		void UpdateCacheInternal()
		{
			if (this._NextRefresh < DateTime.Now)
			{
				if (this._NextRefresh < DateTime.Now)
				{
					string connectionString = FarmSettings.Settings.SqlConnectionString;
					if (!string.IsNullOrWhiteSpace(connectionString) && SqlRemarQ.TableExists(connectionString, Constants.ReadUnreadResourceTableName))
					{
						bool needsImpersonation = BalsamicSolutions.ReadUnreadSiteColumn.SqlRemarQ.NeedsImpersonation(connectionString);
						Dictionary<string, string> tempCollection = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
						IDisposable securityContext = needsImpersonation ? HostingEnvironment.Impersonate() : null;
						try
						{
							using (SqlConnection sqlConn = new SqlConnection(connectionString))
							{
								sqlConn.Open();
								string sqlQuery = "SELECT Culture,Name,Value  FROM [" + Constants.ReadUnreadResourceTableName + "]";
								using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn))
								{
									using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
									{
										while (sqlReader.Read())
										{
											string resCulture = sqlReader.GetString(0);
											string resName = sqlReader.GetString(1);
											string resValue = sqlReader.GetString(2);
											string keyName = resCulture.ToStandardCultureNameFormat() + ":" + resName.ToCamelCase();
											if (!tempCollection.ContainsKey(keyName))
											{
												tempCollection.Add(keyName, resValue);
											}
										}
									}
								}
							}

							lock (this._LockProxy)
							{
								if (tempCollection.Count > 0)
								{
									this._Cache.Clear();
									foreach (string keyName in tempCollection.Keys)
									{ 
										if (!this._Cache.ContainsKey(keyName))
										{
											this._Cache.Add(keyName, tempCollection[keyName]);
										}
									}
								}
							}
							this._NextRefresh = DateTime.Now.AddMinutes(this._CacheIntervalInMinutes);
						}
						catch (SqlException sqlError)
						{
							//Unprovisiond language
							RemarQLog.LogError("Unprovisioned language", sqlError);
						}
						finally
						{
							if (null != securityContext)
							{
								securityContext.Dispose();
							}
						}
					}
				}
			}
		}
			
		string GetResourceString(string resName)
		{
			return this.GetResourceString(CultureInfo.CurrentUICulture, resName);
		}
	
		string GetResourceString(System.Globalization.CultureInfo cultureInfo, string resName)
		{
			string cultureName = null == cultureInfo ? this._DefaultCulture : cultureInfo.Name;
			return this.GetResourceString(cultureName, resName);
		}

		string GetResourceString(string cultureName, string resName)
		{
			string returnValue = null;
			if (null == cultureName)
			{
				cultureName = DEFAULT_CULTURE;
			}
			string keyName = cultureName + ":" + resName;
			this.UpdateCacheInternal();
			lock (this._LockProxy)
			{
				if (!this._Cache.TryGetValue(keyName, out returnValue))
				{
					//try two char base name
					string baseName = cultureName.Split('-')[0] ;
					keyName = baseName + ":" + resName;
					if (!this._Cache.TryGetValue(keyName, out returnValue))
					{
						//try standard four char base name
						string standardName = baseName + "-" + baseName;
						keyName = standardName + ":" + resName;
						if (!this._Cache.TryGetValue(keyName, out returnValue))
						{
							//get default culture
							keyName = this._DefaultCulture + ":" + resName;
							if (!this._Cache.TryGetValue(keyName, out returnValue))
							{
								System.Diagnostics.Trace.WriteLine("Missing Resource for " + keyName);
							}
						}
					}
				}
			}
			return returnValue;
		}

		#region Statics
		
		/// <summary>
		/// return a list of cultures installed in cluture table
		/// </summary>
		/// <returns></returns>
		public static string[] InstalledCultures()
		{
			List<string> returnValue = new List<string>();
			string connectionString = FarmSettings.Settings.SqlConnectionString;
			if (!connectionString.IsNullOrWhiteSpace())
			{
				bool needsImpersonation = BalsamicSolutions.ReadUnreadSiteColumn.SqlRemarQ.NeedsImpersonation(connectionString);
				IDisposable securityContext = needsImpersonation ? HostingEnvironment.Impersonate() : null;
				try
				{
					string sqlQuery = "SELECT DISTINCT [Culture] FROM [" + Constants.ReadUnreadResourceTableName + "]";
					using (SqlConnection sqlConn = new SqlConnection(connectionString))
					{
						sqlConn.Open();
						using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn))
						{
							using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
							{
								while (sqlReader.Read())
								{
									returnValue.Add(sqlReader.GetString(0));
								}
							}
						}
					}
				}
				finally
				{
					if (null != securityContext)
					{
						securityContext.Dispose();
					}
				}
			}

			return returnValue.ToArray();
		}

		/// <summary>
		/// removes a culture from the culture table
		/// </summary>
		/// <param name="cultureName"></param>
		public static void RemoveCulture(string cultureName)
		{
			string connectionString = FarmSettings.Settings.SqlConnectionString;
			if (!connectionString.IsNullOrWhiteSpace())
			{
				if (cultureName.IsNullOrWhiteSpace())
				{
					cultureName = DEFAULT_CULTURE;
				}
				string cmdText = "DELETE FROM [" + Constants.ReadUnreadResourceTableName + "] WHERE [Culture]='" + cultureName.Trim() + "'";
				bool needsImpersonation = BalsamicSolutions.ReadUnreadSiteColumn.SqlRemarQ.NeedsImpersonation(connectionString);
				IDisposable securityContext = needsImpersonation ? HostingEnvironment.Impersonate() : null;
				try
				{
					using (SqlConnection sqlConn = new SqlConnection(connectionString))
					{
						sqlConn.Open();
						using (SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConn))
						{
							sqlCommand.ExecuteNonQuery();
						}
					}
				}
				finally
				{
					if (null != securityContext)
					{
						securityContext.Dispose();
					}
				}
			}
		}

		/// <summary>
		/// returns an array of all the resource files that are part 
		/// of the embeded package
		/// </summary>
		/// <returns></returns>
		public static string[] GetEmbededCultures()
		{
			List<string> returnValue = new List<string>();
			Assembly thisAssm = Assembly.GetExecutingAssembly();
			string[] resFileNames = thisAssm.GetManifestResourceNames();
			foreach (string resFileName in resFileNames)
			{
				if (resFileName.EndsWith("Resources.xml", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						XmlDocument xmldoc = new XmlDocument();
						string flatText = Utilities.GetEmbeddedResourceString(resFileName);
						xmldoc.LoadXml(flatText);
						XmlNode rootNode = xmldoc.SelectSingleNode("Resources");
						if (null != rootNode)
						{
							string cultureName = rootNode.Attributes["culture"].Value;
							returnValue.Add(cultureName);
						}
					}
					catch (XmlException)
					{
					}
				}
			}
			return returnValue.ToArray();
		}

		/// <summary>
		/// installs a culture from the culture table
		/// </summary>
		/// <param name="cultureName"></param>
		/// <param name="cultureValues"></param>
		public static void InstallCulture(string cultureName, Dictionary<string, string> cultureValues)
		{
			if (null == cultureValues)
			{
				throw new ArgumentNullException("cultureValues");
			}
			string connectionString = FarmSettings.Settings.SqlConnectionString;
			if (!connectionString.IsNullOrWhiteSpace())
			{
				if (cultureName.IsNullOrWhiteSpace())
				{
					cultureName = DEFAULT_CULTURE;
				}
				RemoveCulture(cultureName);
				bool needsImpersonation = BalsamicSolutions.ReadUnreadSiteColumn.SqlRemarQ.NeedsImpersonation(connectionString);
				IDisposable securityContext = needsImpersonation ? HostingEnvironment.Impersonate() : null;
				
				string cmdTemplate = string.Format(CultureInfo.InvariantCulture, "INSERT INTO [{0}] ([Culture],[Name],[Value]) VALUES (@Culture,@Name,@Value)", Constants.ReadUnreadResourceTableName);
				try
				{
					using (SqlConnection sqlConn = new SqlConnection(connectionString))
					{
						sqlConn.Open();
						foreach (string keyName in cultureValues.Keys)
						{
							string resValue = cultureValues[keyName];
							using (SqlCommand sqlCommand = new SqlCommand(cmdTemplate, sqlConn))
							{
								SqlParameter cultureParam = sqlCommand.Parameters.Add("@Culture", SqlDbType.NVarChar, 32);
								cultureParam.Value = cultureName;
							
								SqlParameter nameParam = sqlCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 64);
								nameParam.Value = keyName;
							
								SqlParameter valueParam = sqlCommand.Parameters.Add("@Value", SqlDbType.NVarChar);
								valueParam.Value = resValue;
								sqlCommand.ExecuteNonQuery();
							}
						}
					}
				}
				finally
				{
					if (null != securityContext)
					{
						securityContext.Dispose();
					}
				}
			}
		}
		
		/// <summary>
		/// load all the default values into the language table for this project
		/// </summary>
		public static void Initialize(bool removeAll)
		{
			lock (_StaticLock)
			{
				string connectionString = FarmSettings.Settings.SqlConnectionString;
				if (!string.IsNullOrWhiteSpace(connectionString))
				{
					if (removeAll)
					{
						foreach (string cultureName in InstalledCultures())
						{
							RemoveCulture(cultureName);
						}
					}
					///install all embeded resource files
					Assembly thisAssm = Assembly.GetExecutingAssembly();
					string[] resFileNames = thisAssm.GetManifestResourceNames();
					foreach (string resFileName in resFileNames)
					{ 
						if (resFileName.EndsWith("Resources.xml", StringComparison.OrdinalIgnoreCase))
						{
							XmlDocument xmldoc = new XmlDocument();
							string xmlText = Utilities.GetEmbeddedResourceString(resFileName);
							try
							{
								xmldoc.LoadXml(xmlText);
								XmlNode rootNode = xmldoc.SelectSingleNode("Resources");
								if (null != rootNode)
								{
									string cultureName = rootNode.Attributes["culture"].Value.ToUpperInvariant().Trim();
									string otherCultureName = string.Empty;
									if (null != rootNode.Attributes["altculture"])
									{
										otherCultureName = rootNode.Attributes["altculture"].Value.ToUpperInvariant().Trim();
									}
									Dictionary<string, string> cultureValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
									 
									foreach (XmlNode resNode in xmldoc.SelectNodes("Resources/Resource"))
									{
										string resName = resNode.Attributes["Name"].Value;
										string resValue = resNode.InnerText;
										if (!cultureValues.ContainsKey(resName))
										{
											cultureValues.Add(resName, resValue);
										}
										else
										{
											System.Diagnostics.Trace.Write("duplicate key " + resName);
										}
									}
					
									InstallCulture(cultureName, cultureValues);
									if (!string.IsNullOrEmpty(otherCultureName))
									{
										InstallCulture(otherCultureName, cultureValues);
									}
								}
							}
							catch (XmlException xmlError)
							{
								RemarQLog.LogError("invalid res file language", xmlError);
							}
						}
					}
				}
			}
		}
		
		static ResourceManager _ResManager = null;
		static readonly object _StaticLock = new object();
		
		public static ResourceManager Instance
		{
			get
			{
				if (null == _ResManager)
				{
					lock (_StaticLock)
					{
						if (null == _ResManager)
						{
							_ResManager = new ResourceManager();
						}
					}
				}
				return _ResManager;
			}
		}
		
		public static void ResetCache()
		{
			lock (_StaticLock)
			{
				if (null != _ResManager)
				{
					_ResManager.ResetCacheInternal();
				}
			}
		}
	 
		public static string GetString(CultureInfo uiCulture, string resName)
		{
			string returnValue = Instance.GetResourceString(uiCulture, resName);
			if (null == returnValue)
			{
				returnValue = "RES:ERROR";
			}
			return returnValue;
		}

		public static string GetString(CultureInfo uiCulture, CultureInfo spWebCulture, string resName)
		{
			string returnValue = Instance.GetResourceString(uiCulture, resName);
			if (null == returnValue && null != spWebCulture)
			{
				returnValue = Instance.GetResourceString(spWebCulture, resName);
			}
			if (null == returnValue)
			{
				returnValue = "RES:ERROR";
			}
			return returnValue;
		}
		
		public static IEnumerable<CultureInfo> GetInstalledCultures()
		{
			List<CultureInfo> returnValue = new List<CultureInfo>();
			foreach (string cultureName in InstalledCultures())
			{
				CultureInfo installedCulture = new CultureInfo(cultureName);
				returnValue.Add(installedCulture);
			}
			return returnValue.AsReadOnly();
		}
		
		#endregion
	}
}