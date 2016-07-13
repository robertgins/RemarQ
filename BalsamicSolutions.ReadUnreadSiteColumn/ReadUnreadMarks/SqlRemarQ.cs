// -----------------------------------------------------------------------------
//  Copyright 8/3/2014 (c) Balsamic Solutions, Inc. All rights reserved.
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Hosting;
using BalsamicSolutions.ReadUnreadSiteColumn.Framework;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Administration.Claims;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// SQL manager for RemqrQ tables, there are three common tables
	/// for list configuation, version storage, and language text, and one table each
	/// for configured lists. Each list table keeps a copy of the entire list
	/// hierarchy under the user name {SYSTYEM} and read marks for each user.
	/// Table interaction is by direct query, view query, and stored procedures.
	/// Stored procedrues are different, depending on which features are enabled.
	/// </summary>
	internal static class SqlRemarQ
	{
		#region SQL connection string cache
		
		static string _CachedSqlCS = null;
		static readonly object _LockProxy = new object();
		
		/// <summary>
		/// its very cpu expensive to get the connection string
		/// so we will cache it, 
		/// This is our only real "leak" of a plain text
		/// connection string but for performance we need
		/// to cache it, 
		/// </summary>
		static string SqlConnectionString
		{
			get
			{
				if (null == _CachedSqlCS)
				{
					lock (_LockProxy)
					{
						if (null == _CachedSqlCS)
						{
							_CachedSqlCS = FarmSettings.Settings.SqlConnectionString;
						}
					}
				}
				return _CachedSqlCS;
			}
		}
		
		/// <summary>
		/// whack the connection string cache, only called
		/// from the farm settings object
		/// </summary>
		internal static void DropConnectionStringCache()
		{
			lock (_LockProxy)
			{
				_CachedSqlCS = null;
			}
		}
		
		internal static int SqlAutoRetry
		{
			get { return 2; }
		}
		
		static void RetrySleep()
		{
			if (!System.Threading.Thread.Yield())
			{
				System.Threading.Thread.Sleep(SqlAutoRetry);
			}
		}
		
		#endregion
		
		#region List Configuration
		
		/// <summary>
		/// update the list configuration for a specific list in our config table
		/// instead of simple dynamic text we use the command paramater
		/// collection.
		/// </summary>
		internal static int UpdateListConfiguration(Guid listId, Guid webId, Guid siteId, Guid fieldId, ColumnRenderMode colRenderMode, string readImagePath, string unReadImagePath,
			string unReadHtmlColor, string thredUnReadHtmlColor, ListConfiguration.ContextMenuType contextMenu, ListConfiguration.VersionUpdateType versionUpdateFlags,
			string publicName, uint listLcid, string layoutsPath, int refreshInterval)
		{
			int returnValue = 0;
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				// Even though this is not directly exposed to users
				// becauase MSCAF thinks that it is, so to satisfly the false
				// postive we use a paramaratized update
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					
					using (SqlCommand sqlCommand = new SqlCommand("dbo.UpdateListConfiguration", sqlConn))
					{
						sqlCommand.CommandType = CommandType.StoredProcedure;							
						SqlParameter listIdParam = sqlCommand.Parameters.Add("@ListId", SqlDbType.UniqueIdentifier);
						listIdParam.Value = listId;
						
						SqlParameter webIdParam = sqlCommand.Parameters.Add("@WebId", SqlDbType.UniqueIdentifier);
						webIdParam.Value = webId;
						
						SqlParameter siteIdParam = sqlCommand.Parameters.Add("@SiteId", SqlDbType.UniqueIdentifier);
						siteIdParam.Value = siteId;
						
						SqlParameter fieldIdParam = sqlCommand.Parameters.Add("@FieldId", SqlDbType.UniqueIdentifier);
						fieldIdParam.Value = fieldId;
						
						SqlParameter publicNameParam = sqlCommand.Parameters.Add("@PublicName", SqlDbType.NVarChar, 50);
						publicNameParam.Value = publicName.TrimTo(50);
						
						CultureInfo defaultCulture = new CultureInfo((int)listLcid);
						SqlParameter cultureNameParam = sqlCommand.Parameters.Add("@CultureName", SqlDbType.NVarChar, 50);
						cultureNameParam.Value = defaultCulture.Name.TrimTo(50);
						
						SqlParameter colRenderModeParam = sqlCommand.Parameters.Add("@ColumnRenderMode", SqlDbType.Int);
						colRenderModeParam.Value = (int)colRenderMode;
						
						SqlParameter colversionUpdateFlagsParam = sqlCommand.Parameters.Add("@VersionUpdateFlags", SqlDbType.Int);
						colversionUpdateFlagsParam.Value = (int)versionUpdateFlags;
						
						SqlParameter unreadImageUrlParam = sqlCommand.Parameters.Add("@UnreadImageUrl", SqlDbType.NVarChar, 512);
						unreadImageUrlParam.Value = unReadImagePath.TrimTo(512);
						
						SqlParameter readImageUrlParam = sqlCommand.Parameters.Add("@ReadImageUrl", SqlDbType.NVarChar, 512);
						readImageUrlParam.Value = readImagePath.TrimTo(512);
						
						SqlParameter unreadHtmlColorParam = sqlCommand.Parameters.Add("@UnreadhHtmlColor", SqlDbType.NVarChar, 50);
						unreadHtmlColorParam.Value = unReadHtmlColor.TrimTo(50);
						
						SqlParameter threadUnReadHtmlColorParam = sqlCommand.Parameters.Add("@ThreadUnReadHtmlColor", SqlDbType.NVarChar, 50);
						threadUnReadHtmlColorParam.Value = thredUnReadHtmlColor.TrimTo(50);
						
						SqlParameter showToolsParam = sqlCommand.Parameters.Add("@ShowEditingTools", SqlDbType.Int);
						showToolsParam.Value = (int)contextMenu;
						
						SqlParameter layoutsUrlParam = sqlCommand.Parameters.Add("@LayoutsUrl", SqlDbType.NVarChar, 512);
						layoutsUrlParam.Value = layoutsPath.TrimTo(512);
						
						SqlParameter refreshIntervalParam = sqlCommand.Parameters.Add("@RefreshInterval", SqlDbType.Int);
						refreshIntervalParam.Value = refreshInterval;
						
						returnValue = sqlCommand.ExecuteNonQuery();
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
			ListConfigurationCache.UpdateCache(listId);
			return returnValue;
		}
		
		/// <summary>
		/// update the list configuration for a specific list in our config table
		/// instead of simple dynamic text we use the command paramater
		/// collection.
		/// </summary>
		internal static void CreateListConfiguration(Guid listId, Guid webId, Guid siteId, Guid fieldId, ColumnRenderMode colRenderMode, string readImagePath, string unReadImagePath,
			string unReadHtmlColor, string thredUnReadHtmlColor, ListConfiguration.ContextMenuType contextMenu, ListConfiguration.VersionUpdateType versionUpdateFlags, string publicName, uint listLcid, string layoutsPath)
		{
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				// Even though this is not directly exposed to users
				// becauase MSCAF thinks that it is, so to satisfly the false
				// postive we use a paramaratized update
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					
					using (SqlCommand sqlCommand = new SqlCommand("dbo.CreateListConfiguration", sqlConn))
					{
						sqlCommand.CommandType = CommandType.StoredProcedure;
						
						SqlParameter listIdParam = sqlCommand.Parameters.Add("@ListId", SqlDbType.UniqueIdentifier);
						listIdParam.Value = listId;
						
						SqlParameter webIdParam = sqlCommand.Parameters.Add("@WebId", SqlDbType.UniqueIdentifier);
						webIdParam.Value = webId;
						
						SqlParameter siteIdParam = sqlCommand.Parameters.Add("@SiteId", SqlDbType.UniqueIdentifier);
						siteIdParam.Value = siteId;
						
						SqlParameter fieldIdParam = sqlCommand.Parameters.Add("@FieldId", SqlDbType.UniqueIdentifier);
						fieldIdParam.Value = fieldId;
						
						SqlParameter publicNameParam = sqlCommand.Parameters.Add("@PublicName", SqlDbType.NVarChar, 50);
						publicNameParam.Value = publicName.TrimTo(50);
						
						CultureInfo defaultCulture = new CultureInfo((int)listLcid);
						SqlParameter cultureNameParam = sqlCommand.Parameters.Add("@CultureName", SqlDbType.NVarChar, 50);
						cultureNameParam.Value = defaultCulture.Name.TrimTo(50);
						
						SqlParameter colRenderModeParam = sqlCommand.Parameters.Add("@ColumnRenderMode", SqlDbType.Int);
						colRenderModeParam.Value = (int)colRenderMode;
						
						SqlParameter unreadImageUrlParam = sqlCommand.Parameters.Add("@UnreadImageUrl", SqlDbType.NVarChar, 512);
						unreadImageUrlParam.Value = unReadImagePath.TrimTo(512);
						
						SqlParameter readImageUrlParam = sqlCommand.Parameters.Add("@ReadImageUrl", SqlDbType.NVarChar, 512);
						readImageUrlParam.Value = readImagePath.TrimTo(512);
						
						SqlParameter unreadHtmlColorParam = sqlCommand.Parameters.Add("@UnreadhHtmlColor", SqlDbType.NVarChar, 50);
						unreadHtmlColorParam.Value = unReadHtmlColor.TrimTo(50);
						
						SqlParameter threadUnReadHtmlColorParam = sqlCommand.Parameters.Add("@ThreadUnReadHtmlColor", SqlDbType.NVarChar, 50);
						threadUnReadHtmlColorParam.Value = thredUnReadHtmlColor.TrimTo(50);
						
						SqlParameter showToolsParam = sqlCommand.Parameters.Add("@ShowEditingTools", SqlDbType.Int);
						showToolsParam.Value = (int)contextMenu;
						
						SqlParameter colversionUpdateFlagsParam = sqlCommand.Parameters.Add("@VersionUpdateFlags", SqlDbType.Int);
						colversionUpdateFlagsParam.Value = (int)versionUpdateFlags;
						
						SqlParameter layoutsUrlParam = sqlCommand.Parameters.Add("@LayoutsUrl", SqlDbType.NVarChar, 512);
						layoutsUrlParam.Value = layoutsPath.TrimTo(512);
						
						SqlParameter refreshIntervalParam = sqlCommand.Parameters.Add("@RefreshInterval", SqlDbType.Int);
						refreshIntervalParam.Value = 0;
						
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
			ListConfigurationCache.UpdateCache(listId);
		}
		
		/// <summary>
		/// get the configuration for a specific list
		/// </summary>
		/// <param name="listId"></param>
		/// <returns></returns>
		internal static ListConfiguration GetListConfiguration(Guid listId)
		{
			ListConfiguration returnValue = null;
			int retryCount = 0;
			while (retryCount < SqlAutoRetry)
			{
				try
				{
					returnValue = GetListConfigurationEx(listId);
					retryCount = SqlAutoRetry + 1;
				}
				catch (System.Data.SqlClient.SqlException sqlError)
				{
					if (sqlError.Number == 1205)// Deadlock                         
					{
						retryCount++;
						RetrySleep();
					}
					else
					{
						throw;
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// get the configuration for a specific list
		/// </summary>
		/// <param name="listId"></param>
		/// <returns></returns>
		static ListConfiguration GetListConfigurationEx(Guid listId)
		{
			ListConfiguration returnValue = null;
			
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = new SqlCommand("dbo.GetListConfiguration", sqlConn))
					{
						sqlCommand.CommandType = CommandType.StoredProcedure;
						SqlParameter listIdParam = sqlCommand.Parameters.Add("@ListId", SqlDbType.UniqueIdentifier);
						listIdParam.Value = listId;
						using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
						{
							if (sqlReader.Read())
							{
								returnValue = new ListConfiguration(sqlReader);
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
			return returnValue;
		}
		
		/// <summary>
		/// get all configured lists
		/// </summary>
		/// <returns></returns>
		internal static List<ListConfiguration> GetListConfigurations()
		{
			List<ListConfiguration> returnValue = new List<ListConfiguration>();
			
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = new SqlCommand("dbo.GetListConfigurations", sqlConn))
					{
						sqlCommand.CommandType = CommandType.StoredProcedure;
						using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
						{
							while (sqlReader.Read())
							{
								ListConfiguration listConfig = new ListConfiguration(sqlReader);
								returnValue.Add(listConfig);
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
			return returnValue;
		}
		
		#endregion
		
		#region Queue commands
		
		internal static void QueueListCommand(SPList queueMe, ListCommand listCmd)
		{
			Guid listId = queueMe.ID;
			Guid webId = queueMe.ParentWeb.ID;
			Guid siteId = queueMe.ParentWeb.Site.ID;
			string tableName = Constants.ReadUnreadQueueTableName;
			if (queueMe.ItemCount > FarmSettings.Settings.MinSizeOfLargeList)
			{
				tableName = Constants.ReadUnreadLargeQueueTableName;
			}
			QueueListCommand(siteId, webId, listId, listCmd, tableName);
		}
		
		/// <summary>
		/// adds a command request for the list initialization processor
		/// </summary>
		/// <param name="listId"></param>
		/// <param name="listCmd"></param>
		internal static void QueueListCommand(Guid siteId, Guid webId, Guid listId, ListCommand listCmd, string queueName)
		{
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					string cmdText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO [{0}] ([SiteId],[WebId],[ListId],[QueueCommand],[Batch],[PercentComplete],[LockName],[LastUpdate]) Values(@SiteId,@WebId,@ListId,@QueueCommand,@Batch,0,'',GETUTCDATE())", queueName);
					using (SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConn))
					{
						SqlParameter siteIdParam = sqlCommand.Parameters.Add("@SiteId", SqlDbType.UniqueIdentifier);
						siteIdParam.Value = siteId;
						
						SqlParameter webIdParam = sqlCommand.Parameters.Add("@WebId", SqlDbType.UniqueIdentifier);
						webIdParam.Value = webId;
						
						SqlParameter listIdParam = sqlCommand.Parameters.Add("@ListId", SqlDbType.UniqueIdentifier);
						listIdParam.Value = listId;
						
						SqlParameter cmdParam = sqlCommand.Parameters.Add("@QueueCommand", SqlDbType.Int);
						cmdParam.Value = (int)listCmd;
						
						SqlParameter batchParam = sqlCommand.Parameters.Add("@Batch", SqlDbType.NVarChar, 50);
						batchParam.Value = "NONE";
						
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
		
		internal static void TickleQueueRecord(int jobId, int percentComplete, string queueName)
		{
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					string cmdText = string.Format(CultureInfo.InvariantCulture, "UPDATE [{0}] SET [PercentComplete] = @PercentComplete, [LastUpdate]=GETUTCDATE() WHERE [Id]= @JobId", queueName);
					using (SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConn))
					{
						SqlParameter jobIdParam = sqlCommand.Parameters.Add("@JobId", SqlDbType.Int);
						jobIdParam.Value = jobId;
						
						SqlParameter webIdParam = sqlCommand.Parameters.Add("@PercentComplete", SqlDbType.Int);
						webIdParam.Value = percentComplete;
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
		
		internal static int QueueCount(string queueName)
		{
			string sqlQuery = string.Format(CultureInfo.InvariantCulture, "SELECT COUNT([ListId]) FROM [{0}]", queueName);
			List<object[]> queryResults = SqlRemarQ.ExecuteQuery(sqlQuery);
			return (int)queryResults[0][0] ;
		}
		
		/// <summary>
		/// creates a batch out of existing entries in the queue table
		/// also provides three lisits of listId's to work on
		/// This batch is not "multi instance safe" as only 
		/// one of job runs each time, but we need
		///to know which ones to delete at the end of our process
		///so we will create a batch
		/// </summary>
		/// <returns></returns>
		internal static string CreateBatch(string queueName, out List<Guid> provisionList, out List<Guid> verifyList, out List<Guid> deprovisionList, out List<Guid> reinitializeList, out Dictionary<Guid, int> jobMap)
		{
			provisionList = new List<Guid>();
			deprovisionList = new List<Guid>();
			verifyList = new List<Guid>();
			reinitializeList = new List<Guid>();
			jobMap = new Dictionary<Guid, int>();
			string returnValue = CreateBatch(queueName);
			if (!string.IsNullOrEmpty(returnValue))
			{
				string sqlQuery = string.Format(CultureInfo.InvariantCulture, "SELECT [QueueCommand],[ListId],[Id] FROM [{0}] WHERE [Batch]=@Batch ORDER BY ID ASC", queueName);
				IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
				try
				{
					using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
					{
						sqlConn.Open();
						using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn))
						{
							SqlParameter batchParam = sqlCommand.Parameters.Add("@Batch", SqlDbType.NVarChar, 50);
							batchParam.Value = returnValue;
							
							using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
							{
								while (sqlReader.Read())
								{
									//they are in request order so we need to sort them by
									//whichever came last
									ListCommand listCmd = (ListCommand)sqlReader.GetInt32(0);
									Guid listId = sqlReader.GetGuid(1);
									int jobId = sqlReader.GetInt32(2);
									
									switch(listCmd)
									{
										case ListCommand.None:
											break;
										case ListCommand.Deprovision:
											if (!deprovisionList.Contains(listId))
											{
												deprovisionList.Add(listId);
												jobMap[listId] = jobId;
											}
											verifyList.Remove(listId);
											provisionList.Remove(listId);
											reinitializeList.Remove(listId);
											break;
										case ListCommand.Provision:
											if (!provisionList.Contains(listId))
											{
												provisionList.Add(listId);
												jobMap[listId] = jobId;
											}
											verifyList.Remove(listId);
											deprovisionList.Remove(listId);
											reinitializeList.Remove(listId);
											break;
										case ListCommand.ReInitialize:
											
											if (!deprovisionList.Contains(listId))
											{
												if (!reinitializeList.Contains(listId))
												{
													reinitializeList.Add(listId);
													jobMap[listId] = jobId;
												}
												
												verifyList.Remove(listId);
												provisionList.Remove(listId);
											}
											break;
										case ListCommand.Verify:
											//Special case, if we have a deprovision or provision
											//we cant verify because something else is there
											if (!provisionList.Contains(listId) && !deprovisionList.Contains(listId))
											{
												if (!verifyList.Contains(listId))
												{
													verifyList.Add(listId);
													jobMap[listId] = jobId;
												}
											}
											break;
									}
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
			return returnValue;
		}
		
		/// <summary>
		/// This batch is not "multi instance safe" as only 
		/// one of job runs each time, but we need
		///to know which ones to delete at the end of our process
		///so we will create a batch
		/// </summary>
		/// <returns></returns>
		internal static string CreateBatch(string tableName)
		{
			string returnValue = GetServerUtcDateTime().ToString("dd-MM-yyyy HH:mm:ss.fff");
			int batchSize = FarmSettings.Settings.BatchSize;
			if (tableName.Equals(Constants.ReadUnreadLargeQueueTableName, StringComparison.OrdinalIgnoreCase))
			{
				batchSize = FarmSettings.Settings.LargeBatchSize;
			}
			//we dont care if an existing batch is assigned because that means that the system reset while it was processing , 
			string sqlQuery = string.Format(CultureInfo.InvariantCulture, "UPDATE TOP ({0}) [{1}] SET [Batch]=@Batch", batchSize, tableName);
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn))
					{
						SqlParameter batchParam = sqlCommand.Parameters.Add("@Batch", SqlDbType.NVarChar, 50);
						batchParam.Value = returnValue;
						
						int numUpdated = sqlCommand.ExecuteNonQuery();
						if (numUpdated == 0)
						{
							returnValue = string.Empty;
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
			
			return returnValue;
		}
		
		/// <summary>
		/// removes a batch from the queue table
		/// </summary>
		/// <param name="batchId"></param>
		internal static void DeleteBatch(string queueName, string batchId)
		{
			string sqlQuery = string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}] WHERE [Batch]=@Batch", queueName);
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn))
					{
						SqlParameter batchParam = sqlCommand.Parameters.Add("@Batch", SqlDbType.NVarChar, 50);
						batchParam.Value = batchId;
						
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
		
		/// <summary>
		/// query the config table for any lists that are considered
		/// "large" by the throttling rules and initialize it from
		/// an SP job instead of from a thread in the UI
		/// </summary>
		/// <returns></returns>
		internal static List<Guid> GetListsInCommandBatch(string tableName, ListCommand listCmd, string batchId)
		{
			List<Guid> returnValue = new List<Guid>();
			//We return them all, this is just for debugging, we could use an IN clause but
			//some of the debugging needed us to see what was happening per web applications
			//so I left the query that way
			string sqlQuery = string.Format(CultureInfo.InvariantCulture, "SELECT [ListId],[Id] FROM [{0}] WHERE [QueueCommand]={1} AND [Batch]=@Batch", Constants.ReadUnreadQueueTableName, (int)listCmd);
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn))
					{
						SqlParameter batchParam = sqlCommand.Parameters.Add("@Batch", SqlDbType.NVarChar, 50);
						batchParam.Value = batchId;
						
						using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
						{
							while (sqlReader.Read())
							{
								Guid listId = sqlReader.GetGuid(0);
								returnValue.Add(listId);
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
			return returnValue;
		}
		
		#endregion
		
		#region System and Table provisioning
		
		internal static void ProvisionConfigurationTables()
		{
			if (!TypeExists(Constants.ReadUnreadItemIdsTypeName))
			{
				string commandText = string.Format(CultureInfo.InvariantCulture, "CREATE TYPE {0} AS Table (Id INT);", Constants.ReadUnreadItemIdsTypeName);
				ExecuteNonQuery(commandText);
			}
			if (!TableExists(Constants.ReadUnreadConfigurationTableName))
			{
				string commandText = Constants.SqlConfigTableTemplate.Replace("_TABLENAME_", Constants.ReadUnreadConfigurationTableName);
				ExecuteNonQuery(commandText);
			}
			if (!TableExists(Constants.ReadUnreadQueueTableName))
			{
				string commandText = Constants.SqlQueueTableTemplate.Replace("_TABLENAME_", Constants.ReadUnreadQueueTableName);
				ExecuteNonQuery(commandText);
			}
			if (!TableExists(Constants.ReadUnreadLargeQueueTableName))
			{
				string commandText = Constants.SqlQueueTableTemplate.Replace("_TABLENAME_", Constants.ReadUnreadLargeQueueTableName);
				ExecuteNonQuery(commandText);
			}
			if (!TableExists(Constants.ReadUnreadResourceTableName))
			{
				string commandText = Constants.SqlResourceTableTemplate.Replace("_TABLENAME_", Constants.ReadUnreadResourceTableName);
				ExecuteNonQuery(commandText);
				ResourceManager.Initialize(false);
			}
			
			if (!TableExists(Constants.ReadUnreadVersionsTableName))
			{
				string commandText = Constants.SqlVersionsTableTemplate.Replace("_TABLENAME_", Constants.ReadUnreadVersionsTableName);
				ExecuteNonQuery(commandText);
			}
		}
		
		/// <summary>
		/// create a read unread table for a specific list
		/// </summary>
		/// <param name="listId"></param>
		internal static void ProvisionReadUnreadTable(Guid listId)
		{
			string tableName = TableName(listId);
			string listName = listId.ToString("N");
			string commandText = string.Empty;
			if (!TableExists(tableName))
			{
				//From Scratch
				commandText = Constants.SqlReadUnreadTableTemplate.Replace("_LISTID_", listName);
				ExecuteNonQuery(commandText);
				if (FarmLicense.License.LicenseMode >= LicenseModeType.Enterprise)
				{
					commandText = string.Format(CultureInfo.InvariantCulture, "ALTER TABLE [dbo].[{0}] ADD [Version] bigint NOT NULL", tableName);
					ExecuteNonQuery(commandText);
					commandText = Constants.SqlReadUnreadTableIndicesWithVersionTemplate.Replace("_LISTID_", listId.ToString("N"));
					ExecuteNonQuery(commandText);
				}
				else
				{
					commandText = Constants.SqlReadUnreadTableIndicesTemplate.Replace("_LISTID_", listId.ToString("N"));
					ExecuteNonQuery(commandText);
				}
			}
			
			//Check that the schema conforms to the license
			VerifyReadUnreadTableSchema(tableName, listName, FarmLicense.License.LicenseMode, false);
		}
		
		/// <summary>
		/// force a rebuild of the views and stored procedures for
		/// a read unread table
		/// </summary>
		/// <param name="listId"></param>
		internal static void RebuildReadUnreadTableObjects(Guid listId)
		{
			FarmLicense farmLicense = FarmLicense.License;
			string tableName = TableName(listId);
			string listName = listId.ToString("N");
			VerifyReadUnreadTableSchema(tableName, listName, farmLicense.LicenseMode, true);
		}
		
		/// <summary>
		/// check the views and stored procedures for
		/// a read unread table
		/// </summary>
		/// <param name="listId"></param>
		internal static void VerifyReadUnreadTableSchema(Guid listId, bool forceRebuild)
		{
			FarmLicense farmLicense = FarmLicense.License;
			string tableName = TableName(listId);
			string listName = listId.ToString("N");
			VerifyReadUnreadTableSchema(tableName, listName, farmLicense.LicenseMode, forceRebuild);
		}
		
		/// <summary>
		/// checks to see if the Version column has been provisioned on a table
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		static bool HasVersionColumn(string tableName)
		{
			string versionColumnQuery = string.Format(CultureInfo.InvariantCulture, "IF COL_LENGTH('{0}','Version') IS NULL SELECT 0 ELSE SELECT 1", tableName);
			var colQueryResults = ExecuteQuery(versionColumnQuery);
			bool versionColumnExists = colQueryResults.Count == 1 && colQueryResults[0][0].ToString() == "1";
			return versionColumnExists;
		}
		
		/// <summary>
		/// verify the schema and objects are correct for the current operating mode
		/// optionaly force a rebuild of the objects
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="listName"></param>
		/// <param name="licenseMode"></param>
		/// <param name="forceRebuild"></param>
		static void VerifyReadUnreadTableSchema(string tableName, string listName, LicenseModeType licenseMode, bool forceRebuild)
		{
			bool versionColumnExists = HasVersionColumn(tableName);
			string commandText = string.Empty;
			if (licenseMode >= LicenseModeType.Enterprise)
			{
				//check schema, if its wrong then add the version column
				//and update the stored procedures
				if (forceRebuild || !versionColumnExists)
				{
					commandText = Constants.SqlReadUnreadDropTableIndicesTemplate.Replace("_LISTID_", listName);
					ExecuteNonQuery(commandText, true);
					if (!versionColumnExists)
					{
						commandText = string.Format(CultureInfo.InvariantCulture, "ALTER TABLE [dbo].[{0}] ADD [Version] bigint NOT NULL CONSTRAINT Init{1} DEFAULT 0 WITH VALUES", tableName, listName);
						ExecuteNonQuery(commandText);
						commandText = string.Format(CultureInfo.InvariantCulture, "ALTER TABLE [dbo].[{0}] DROP CONSTRAINT [Init{1}]", tableName, listName);
						ExecuteNonQuery(commandText);
					}
					commandText = Constants.SqlReadUnreadTableIndicesWithVersionTemplate.Replace("_LISTID_", listName);
					ExecuteNonQuery(commandText);
				}
			}
			else
			{
				//check the schema if its wrong, drop the version column
				//and update the stored procedures we also have to drop any
				//version history first because it may cause index collisions
				//after the update
				if (forceRebuild || versionColumnExists)
				{
					commandText = Constants.SqlReadUnreadDropTableIndicesTemplate.Replace("_LISTID_", listName);
					ExecuteNonQuery(commandText, true);
					if (versionColumnExists)
					{
						commandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}] WHERE [Version]>0", tableName);
						ExecuteNonQuery(commandText);
						commandText = string.Format(CultureInfo.InvariantCulture, "ALTER TABLE [dbo].[{0}]  DROP COLUMN [Version]", tableName);
						ExecuteNonQuery(commandText);
					}
					commandText = Constants.SqlReadUnreadTableIndicesTemplate.Replace("_LISTID_", listName);
					ExecuteNonQuery(commandText);
				}
			}
		}
		
		/// <summary>
		/// remove the read unread table for a specific list
		/// and its configuration
		/// </summary>
		/// <param name="listId"></param>
		internal static void DeProvisionReadUnreadTableAndRemoveConfigurationEntry(Guid listId)
		{
			string tableName = TableName(listId);
			//remove the table from configuration
			string commandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}] WHERE ListId='{1}'", Constants.ReadUnreadConfigurationTableName, listId.ToString("D"));
			//drop stored procedures
			ExecuteNonQuery(commandText);
			if (TableExists(tableName))
			{
				commandText = Constants.SqlReadUnreadDropTableIndicesTemplate.Replace("_LISTID_", listId.ToString("N"));
				ExecuteNonQuery(commandText, true);
				commandText = "DROP TABLE [dbo].[" + tableName + "]";
				ExecuteNonQuery(commandText, true);
			}
			ListConfigurationCache.UpdateCache(listId);
		}
		
		#endregion
		
		#region Data operations
		
		/// <summary>
		/// update the item in the hierarchy section of the 
		/// read unread marks table
		/// </summary>
		/// <param name="listItem"></param>
		internal static void UpdateItemPath(SPListItem listItem)
		{
			if (null == listItem)
			{
				throw new ArgumentNullException("listItem");
			}
			
			string itemPath = listItem.ReadUnreadParentPath();
			string itemLeaf = listItem.ReadUnreadLeafName();
			string storedProcName = "dbo.rq_UpdateItemPath" + listItem.ParentList.ID.ToString("N");
			List<object[]> queryResults = SqlRemarQ.ExecRqStoredQuery(storedProcName, listItem.ID, -1, Constants.HierarchySystemUserId, itemPath, itemLeaf);
			System.Diagnostics.Trace.Write(queryResults.Count);
		}
		
		/// <summary>
		/// purge an item from the read unread table
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="listId"></param>
		internal static void RemoveAllItemReferences(int itemId, Guid listId)
		{
			string tableName = TableName(listId);
			string commandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}] WHERE [ItemId]={1}", tableName, itemId);
			ExecuteNonQuery(SqlTransact(commandText));
		}
		
		/// <summary>
		/// purge  read unread table
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="listId"></param>
		internal static void RemoveAllItemReferences(Guid listId)
		{
			string tableName = TableName(listId);
			string commandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}] WHERE [ItemId]>-1", tableName);
			ExecuteNonQuery(SqlTransact(commandText));
		}
		
		/// <summary>
		/// list of all child items with a matching path (parentPath*) for 
		/// a particular user
		/// </summary>
		/// <param name="listId"></param>
		/// <param name="parentPath"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		internal static List<int> ChildItems(Guid listId, string parentPath)
		{
			string viewName = HierarchyViewName(listId);
			string sqlQuery = string.Format(CultureInfo.InvariantCulture, "SELECT [ItemId] FROM [{0}] WHERE [Path] LIKE '{1}%'", viewName, parentPath);
			return ItemIDQuery(sqlQuery);
		}
		
		/// <summary>
		/// returns the count of items in the
		/// hierarchy view
		/// </summary>
		/// <param name="listId"></param>
		/// <param name="parentPath"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		internal static int HierarchyItemcount(Guid listId)
		{
			int returnValue = 0;
			string viewName = HierarchyViewName(listId);
			string sqlQuery = string.Format(CultureInfo.InvariantCulture, "SELECT Count([ItemId]) FROM [{0}]", viewName);
			var colQueryResults = ExecuteQuery(sqlQuery);
			if (colQueryResults.Count == 1)
			{
				returnValue = (int)colQueryResults[0][0];
			}
			return returnValue;
		}
		
		/// <summary>
		/// gets the path map for the listed items
		/// </summary>
		/// <param name="listId"></param>
		/// <param name="idFloor"></param>
		/// <param name="idCeiling"></param>
		/// <returns></returns>
		internal static Dictionary<int, string> GetChildPathMap(Guid listId, int idFloor, int idCeiling, bool isDiscussionBoard)
		{
			string storedProcName = "dbo.rq_GetChildPathMap" + listId.ToString("N");
			List<object[]> queryResults = SqlRemarQ.ExecRqStoredQuery(storedProcName, idFloor, idCeiling, int.MinValue, null, null);
			Dictionary<int, string> returnValue = new Dictionary<int, string>();
			if (queryResults.Count > 0)
			{
				foreach (object[] rowResults in queryResults)
				{
					int itemId = (int)rowResults[0];
					string itemPath = (string)rowResults[1];
					string itemLeaf = (string)rowResults[2]; 
					if (isDiscussionBoard)
					{
						//parsing the path is mode specific
						//in a discussionboard we have to add back
						//the hex prefix to make the string LIKE predicates
						//used later work correctly
						if (itemPath.Equals("/", StringComparison.Ordinal))
						{
							itemPath = "0";
						}
						string childPath = itemPath + itemLeaf;
						returnValue[itemId] = childPath;
					}
					else
					{
						//this is just a simple web path so just clean up dupes 
						//and add it together
						if (itemPath.Equals("/", StringComparison.Ordinal))
						{
							itemPath = string.Empty;
						}
						string childPath = itemPath + "/" + itemLeaf;
						returnValue[itemId] = childPath;
					}
				}
			}
			
			return returnValue;
		}
		
		/// <summary>
		/// get all the read marks for a user between two
		/// ranging watermarks
		/// </summary>
		/// <param name="listId">id of list</param>
		/// <param name="loginName">user login name</param>
		/// <param name="idFloor">low watermark</param>
		/// <param name="idCeiling">high watermark</param>
		/// <returns></returns>
		internal static List<int> GetReadMarksFromAListForAUser(Guid listId, SPUser spUser, int idFloor, int idCeiling)
		{
			List<int> returnValue = new List<int>();
			
			string storedProcName = "dbo.rq_GetReadMarksFromAListForAUser" + listId.ToString("N");
			List<object[]> queryResults = SqlRemarQ.ExecRqStoredQuery(storedProcName, idFloor, idCeiling, spUser.ID, null, null);
			
			if (queryResults.Count > 0)
			{
				foreach (object[] rowResults in queryResults)
				{
					foreach (object columValue in rowResults)
					{
						returnValue.Add((int)columValue);
					}
				}
			}
			
			return returnValue;
		}
		
		/// <summary>
		/// shrink the Versions db to just a few of the recent entries
		/// we dont need it except as a counter anyway
		/// </summary>
		internal static void CleanUpVersionsDb()
		{
			long maxVersion = GenerateNewVersionId() - 10;
			if (maxVersion > 0)
			{
				string commandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}] WHERE [Version]<{1}", Constants.ReadUnreadVersionsTableName, maxVersion);
				ExecuteNonQuery(commandText);
			}
		}
		
		/// <summary>
		/// returns a new version id in the version Id Table
		/// </summary>
		/// <param name="sqlConnectionString"></param>
		/// <returns></returns>
		static long GenerateNewVersionId()
		{
			long returnValue = 0;
			if (TableExists(Constants.ReadUnreadVersionsTableName))
			{
				string sqlQuery = string.Format(CultureInfo.InvariantCulture, "INSERT INTO [{0}] DEFAULT VALUES SELECT SCOPE_IDENTITY()", Constants.ReadUnreadVersionsTableName);
				var colQueryResults = ExecuteQuery(sqlQuery);
				if (colQueryResults.Count == 1)
				{
					decimal scopeIdentity = (decimal)colQueryResults[0][0];
					returnValue = (long)scopeIdentity;
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// remove all references in a table for a specific user id
		/// including all version information
		/// </summary>
		/// <param name="userId"></param>
		internal static void DeleteItemsForSpecificUser(Guid listId, SPUser spUser)
		{
			string tableName = TableName(listId);
			string commandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}] WHERE [UserId]={1}", tableName, spUser.ID);
			ExecuteNonQuery(SqlTransact(commandText));
		}
		
		#endregion
		
		#region Mark Read
		
		internal static void MarkRead(int[] itemIds, SPItemEventProperties eventProperties)
		{
			if (null != itemIds && itemIds.Length > 0)
			{
				string storedProcName = "dbo.rq_MarkItemsRead" + eventProperties.ListId.ToString("N");
				SPWeb eventWeb = eventProperties.Web;
				try
				{
					SPUser spUser = eventWeb.AllUsers[eventProperties.UserLoginName];
					ExecRqStoredProc(storedProcName, int.MinValue, itemIds, spUser);
				}
				catch (SPException)
				{
					//user deleted before we found them
				}
				catch (IndexOutOfRangeException)
				{
					//bad user name
				}
			}
		}
		
		/// <summary>
		/// marks multiple items read for a user SPItemEventProperties eventProperties
		/// </summary>
		/// <param name="itemIds"></param>
		/// <param name="listId"></param>
		/// <param name="loginName"></param>
		internal static void MarkRead(int[] itemIds, Guid listId, SPUser spUser)
		{
			if (null != itemIds && itemIds.Length > 0)
			{
				string storedProcName = "dbo.rq_MarkItemsRead" + listId.ToString("N");
				ExecRqStoredProc(storedProcName, int.MinValue, itemIds, spUser);
			}
		}
		
		#endregion
		
		#region Mark Unread
		
		/// <summary>
		/// marks multiples unread for a single user
		/// </summary>
		/// <param name="itemIds"></param>
		/// <param name="listId"></param>
		/// <param name="loginName"></param>
		internal static void MarkUnRead(int[] itemIds, Guid listId, SPUser spUser)
		{
			if (null != itemIds && itemIds.Length > 0)
			{
				string storedProcName = "dbo.rq_MarkItemsUnread" + listId.ToString("N");
				ExecRqStoredProc(storedProcName, int.MinValue, itemIds, spUser);
			}
		}
		
		/// <summary>
		/// marks an entire list as unread for a particualr user
		/// </summary>
		/// <param name="listId"></param>
		/// <param name="spUser"></param>
		internal static void MarkUnRead(Guid listId, SPUser spUser)
		{
			string storedProcName = "dbo.rq_MarkListUnread" + listId.ToString("N");
			ExecRqStoredProc(storedProcName, int.MinValue, null, spUser);
		}
		
		#endregion
		
		#region Reset marks
		
		/// <summary>
		/// Resets the read marks for several items, regardless of the user
		/// </summary>
		/// <param name="associatedIds"></param>
		/// <param name="listId"></param>
		/// <param name="retainHistory"></param>
		internal static void ResetReadMarks(int[] associatedIds, Guid listId)
		{
			if (null != associatedIds && associatedIds.Length > 0)
			{
				string storedProcName = "dbo.rq_ResetItemsReadMark" + listId.ToString("N");
				ExecRqStoredProc(storedProcName, int.MinValue, associatedIds, null);
			}
		}
		
		#endregion
		
		#region Object names
		
		/// <summary>
		/// calculate the table name for a read unread table
		/// for a specific list
		/// </summary>
		/// <param name="listId"></param>
		/// <returns></returns>
		internal static string TableName(Guid listId)
		{
			return "ReadUnread" + listId.ToString("N");
		}
		
		/// <summary>
		/// return the name of the HierarchyView
		/// </summary>
		/// <param name="listId"></param>
		/// <returns></returns>
		internal static string HierarchyViewName(Guid listId)
		{
			return "Hierarchy" + listId.ToString("N");
		}
		
		#endregion
		
		#region Stored procedure calls
		
		/// <summary>
		/// execute a stored procedure that returns values
		/// </summary>
		/// <param name="storedProcedureName"></param>
		/// <param name="itemId"></param>
		/// <param name="folderId"></param>
		/// <param name="userId"></param>
		/// <param name="referencePath"></param>
		/// <param name="referencePath2"></param>
		/// <returns></returns>
		internal static List<object[]> ExecRqStoredQuery(string storedProcedureName, int itemId, int folderId, int userId, string referencePath, string referencePath2)
		{
			List<object[]> returnValue = null;
			int retryCount = 0;
			while (retryCount < SqlAutoRetry)
			{
				try
				{
					returnValue = ExecRqStoredQueryEx(storedProcedureName, itemId, folderId, userId, referencePath, referencePath2);
					retryCount = SqlAutoRetry + 1;
				}
				catch (System.Data.SqlClient.SqlException sqlError)
				{
					if (sqlError.Number == 1205)// Deadlock                         
					{
						retryCount++;
						RetrySleep();
					}
					else
					{
						throw;
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// execute a stored procedure that returns values
		/// </summary>
		/// <param name="storedProcedureName"></param>
		/// <param name="itemId"></param>
		/// <param name="folderId"></param>
		/// <param name="userId"></param>
		/// <param name="referencePath"></param>
		/// <param name="referencePath2"></param>
		/// <returns></returns>
		static List<object[]> ExecRqStoredQueryEx(string storedProcedureName, int itemId, int folderId, int userId, string referencePath, string referencePath2)
		{
			List<object[]> returnValue = new List<object[]>();
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = sqlConn.CreateCommand())
					{
						sqlCommand.CommandText = storedProcedureName;
						sqlCommand.CommandType = CommandType.StoredProcedure;
						
						if (itemId > -1)
						{
							SqlParameter itemIdParam = sqlCommand.Parameters.Add("@itemId", SqlDbType.Int);
							itemIdParam.Value = itemId;
						}
						if (folderId > -1)
						{
							SqlParameter folderIdParam = sqlCommand.Parameters.Add("@folderId", SqlDbType.Int);
							folderIdParam.Value = folderId;
						}
						if (userId >= -1)
						{
							SqlParameter userIdParam = sqlCommand.Parameters.Add("@userId", SqlDbType.Int);
							userIdParam.Value = userId;
						}
						if (null != referencePath)
						{
							SqlParameter pathParam = sqlCommand.Parameters.Add("@referencePath", SqlDbType.NVarChar, 256);
							pathParam.Value = referencePath;
						}
						if (null != referencePath2)
						{
							SqlParameter path2param = sqlCommand.Parameters.Add("@referencePath2", SqlDbType.NVarChar, 256);
							path2param.Value = referencePath2;
						}
						using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
						{
							while (sqlReader.Read())
							{
								object[] oneRow = new object[sqlReader.FieldCount];
								sqlReader.GetValues(oneRow);
								returnValue.Add(oneRow);
							}
						}
					}
				}
			}
			catch (SqlException sqlError)
			{
				System.Diagnostics.Trace.Write(sqlError.Message);
				throw;
			}
			finally
			{
				if (null != securityContext)
				{
					securityContext.Dispose();
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// call an rq structured stored procedure
		/// </summary>
		/// <param name="storedProcedureName"></param>
		/// <param name="itemIds"></param>
		/// <param name="loginName"></param>
		internal static void ExecRqStoredProc(string storedProcedureName, int itemId, int[] itemIds, SPUser spUser)
		{
			int retryCount = 0;
			while (retryCount < SqlAutoRetry)
			{
				try
				{
					ExecRqStoredProcEx(storedProcedureName, itemId, itemIds, spUser);
					retryCount = SqlAutoRetry + 1;
				}
				catch (System.Data.SqlClient.SqlException sqlError)
				{
					if (sqlError.Number == 1205)// Deadlock                         
					{
						retryCount++;
						RetrySleep();
					}
					else
					{
						throw;
					}
				}
			}
		}
		
		/// <summary>
		/// call an rq structured stored procedure
		/// </summary>
		/// <param name="storedProcedureName"></param>
		/// <param name="itemIds"></param>
		/// <param name="loginName"></param>
		static void ExecRqStoredProcEx(string storedProcedureName, int itemId, int[] itemIds, SPUser spUser)
		{
			List<int[]> itemBatchs = ConvertToBatchs(itemIds, 500);
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					foreach (int[] itemBatch in itemBatchs)
					{
						using (SqlCommand sqlCommand = sqlConn.CreateCommand())
						{
							sqlCommand.CommandText = storedProcedureName;
							sqlCommand.CommandType = CommandType.StoredProcedure;
							
							if (null != itemBatch)
							{
								SqlParameter itemIdsParam = sqlCommand.Parameters.Add("@itemIds", SqlDbType.Structured);
								itemIdsParam.Value = CreateItemIdsTableParamater(itemBatch);
							}
							if (itemId > int.MinValue)
							{
								SqlParameter itemIdParam = sqlCommand.Parameters.Add("@itemId", SqlDbType.Int);		
								itemIdParam.Value = itemId;					
							}
							
							if (null != spUser)
							{
								SqlParameter userIdParam = sqlCommand.Parameters.Add("@userId", SqlDbType.Int);		
								userIdParam.Value = spUser.ID;					
							}
							sqlCommand.ExecuteNonQuery();
						}
					}
				}
			}
			catch (SqlException sqlError)
			{
				System.Diagnostics.Trace.Write(sqlError.Message);
				throw;
			}
			finally
			{
				if (null != securityContext)
				{
					securityContext.Dispose();
				}
			}
		}
		
		/// <summary>
		/// get the UTC time at the SQL Server
		/// </summary>
		/// <returns></returns>
		internal static DateTime GetServerUtcDateTime()
		{
			DateTime returnValue = DateTime.MinValue;
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				string utcQuery = "SELECT GETUTCDATE()";
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = new SqlCommand(utcQuery, sqlConn))
					{
						using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
						{
							if (sqlReader.Read())
							{
								returnValue = Convert.ToDateTime(sqlReader[0]);
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
			return new DateTime(returnValue.Ticks,DateTimeKind.Utc);
		}
		
		#endregion
		
		#region Specialized queries
		
		/// <summary>
		/// returns a list of hierarchy items matching the query
		/// </summary>
		/// <param name="queryText"></param>
		/// <returns></returns>
		internal static List<HierarchyItem> HierarchyItemQuery(string queryText)
		{
			List<HierarchyItem> returnValue = null;
			int retryCount = 0;
			while (retryCount < SqlAutoRetry)
			{
				try
				{
					returnValue = HierarchyItemQueryEx(queryText);
					retryCount = SqlAutoRetry + 1;
				}
				catch (System.Data.SqlClient.SqlException sqlError)
				{
					if (sqlError.Number == 1205)// Deadlock                         
					{
						retryCount++;
						RetrySleep();
					}
					else
					{
						throw;
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// returns a list of hierarchy items matching the query
		/// </summary>
		/// <param name="queryText"></param>
		/// <returns></returns>
		static List<HierarchyItem> HierarchyItemQueryEx(string queryText)
		{
			List<HierarchyItem> returnValue = new List<HierarchyItem>();
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = new SqlCommand(queryText, sqlConn))
					{
						using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
						{
							while (sqlReader.Read())
							{
								returnValue.Add(new HierarchyItem(sqlReader));
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
			return returnValue;
		}
		
		/// <summary>
		/// execute a Sql query , one that expects only to return the ItemId's in the reader
		/// </summary>
		/// <param name="queryText"></param>
		/// <returns></returns>
		static public List<int> ItemIDQuery(string queryText)
		{
			List<int> returnValue = null;
			int retryCount = 0;
			while (retryCount < SqlAutoRetry)
			{
				try
				{
					returnValue = ItemIDQueryEx(queryText);
					retryCount = SqlAutoRetry + 1;
				}
				catch (System.Data.SqlClient.SqlException sqlError)
				{
					if (sqlError.Number == 1205)// Deadlock                         
					{
						retryCount++;
						RetrySleep();
					}
					else
					{
						throw;
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// execute a Sql query , one that expects only to return the ItemId's in the reader
		/// </summary>
		/// <param name="queryText"></param>
		/// <returns></returns>
		static List<int> ItemIDQueryEx(string queryText)
		{
			List<int> returnValue = new List<int>();
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = new SqlCommand(queryText, sqlConn))
					{
						using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
						{
							if (sqlReader.Read())
							{
								int colIdx = 0;
								try
								{
									colIdx = sqlReader.GetOrdinal("ItemId");
								}
								catch (IndexOutOfRangeException)
								{
									RemarQLog.LogWarning("Unexpected Query results for " + queryText);
									colIdx = -1;
								}
								if (colIdx > -1)
								{
									returnValue.Add(sqlReader.GetInt32(colIdx));
									while (sqlReader.Read())
									{
										returnValue.Add(sqlReader.GetInt32(colIdx));
									}
								}
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
			return returnValue;
		}
		
		/// <summary>
		/// execute a command statement or a series of command statements 
		/// seperated by GO
		/// </summary>
		/// <param name="commandText"></param>
		/// <returns>total rows affected</returns>
		static int ExecuteNonQuery(string commandText, bool ignoreErrors = false)
		{
			int returnValue = -1;
			int retryCount = 0;
			while (retryCount < SqlAutoRetry)
			{
				try
				{
					returnValue = ExecuteNonQueryEx(commandText, ignoreErrors);
					retryCount = SqlAutoRetry + 1;
				}
				catch (System.Data.SqlClient.SqlException sqlError)
				{
					if (sqlError.Number == 1205)// Deadlock                         
					{
						retryCount++;
						RetrySleep();
					}
					else
					{
						throw;
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// execute a command statement or a series of command statements 
		/// seperated by GO
		/// </summary>
		/// <param name="commandText"></param>
		/// <returns>total rows affected</returns>
		static int ExecuteNonQueryEx(string commandText, bool ignoreErrors)
		{
			Regex sqlSplit = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			string[] sqlLines = sqlSplit.Split(commandText);
			int returnValue = 0;
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				for (int lineIdx = 0; lineIdx < sqlLines.Length; lineIdx++)
				{
					string cmdText = sqlLines[lineIdx].Trim(new char[] { '\r', '\n' });
					if (cmdText.Length > 5)
					{
						using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
						{
							sqlConn.Open();
							using (SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConn))
							{
								try
								{
									returnValue += sqlCommand.ExecuteNonQuery();
								}
								catch (SqlException)
								{
									if (!ignoreErrors)
									{
										throw;
									}
								}
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
			return returnValue;
		}
		
		/// <summary>
		/// execute a query and return the values , but close the 
		/// reader to release the resources
		/// </summary>
		/// <param name="sqlQuery"></param>
		/// <returns></returns>
		internal static List<object[]> ExecuteQuery(string sqlQuery)
		{
			List<object[]> returnValue = null;
			int retryCount = 0;
			while (retryCount < SqlAutoRetry)
			{
				try
				{
					returnValue = ExecuteQueryEx(sqlQuery);
					retryCount = SqlAutoRetry + 1;
				}
				catch (System.Data.SqlClient.SqlException sqlError)
				{
					if (sqlError.Number == 1205)// Deadlock                         
					{
						retryCount++;
						RetrySleep();
					}
					else
					{
						throw;
					}
				}
			}
			return returnValue;
		}

		/// <summary>
		/// returns the result set with column names suitable for serialization
		/// </summary>
		/// <param name="sqlQuery"></param>
		/// <returns></returns>
		internal static List<Dictionary<string, object>> ExecuteQueryWithColumnsEx(string sqlQuery)
		{
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn))
					{
						DataTable dataTbl = new DataTable();
						SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
						da.Fill(dataTbl);

						foreach (DataRow dataRow in dataTbl.Rows)
						{
							Dictionary<string, object> oneRow = new Dictionary<string, object>();
							foreach (DataColumn col in dataTbl.Columns)
							{
								oneRow.Add(col.ColumnName, dataRow[col]);
							}
							rows.Add(oneRow);
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
			return rows;
		}

		/// <summary>
		/// execute a query and return the values , but close the 
		/// reader to release the resources
		/// </summary>
		/// <param name="sqlQuery"></param>
		/// <returns></returns>
		static List<object[]> ExecuteQueryEx(string sqlQuery)
		{
			IDisposable securityContext = FarmSettings.Settings.NeedsImpersonation ? HostingEnvironment.Impersonate() : null;
			List<object[]> returnValue = new List<object[]>();
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(SqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn))
					{
						using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
						{
							while (sqlReader.Read())
							{
								object[] oneRow = new object[sqlReader.FieldCount];
								sqlReader.GetValues(oneRow);
								returnValue.Add(oneRow);
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
			return returnValue;
		}
		
		/// <summary>
		/// check and see if a table exists in the corresponding database
		/// </summary>
		/// <param name="sqlConnectionString"></param>
		/// <param name="tableName"></param>
		/// <returns></returns>
		internal static bool SqlObjectExists(string sqlConnectionString, string sqlQuery)
		{
			bool returnValue = false;
			bool needsImpersonation = NeedsImpersonation(sqlConnectionString);
			IDisposable securityContext = needsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				using (SqlConnection sqlConn = new SqlConnection(sqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConn))
					{
						using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
						{
							if (sqlReader.Read())
							{
								returnValue = (1 == sqlReader.GetInt32(0));
							}
						}
					}
				}
			}
			catch (System.Data.SqlClient.SqlException eatMe)
			{
				System.Diagnostics.Trace.WriteLine(eatMe.Message);
				returnValue = false;
			}
			finally
			{
				if (null != securityContext)
				{
					securityContext.Dispose();
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// test a connection string to see if it can access a server
		/// and if it has the correct permissions on the database
		/// </summary>
		/// <param name="sqlConnectionString"></param>
		/// <returns></returns>
		internal static bool TestConnectionString(string sqlConnectionString)
		{
			bool returnValue = false;
			
			bool needsImpersonation = NeedsImpersonation(sqlConnectionString);
			IDisposable securityContext = needsImpersonation ? HostingEnvironment.Impersonate() : null;
			try
			{
				//these will blow chunks if we dont have permission or 
				//the identity is wrong
				string tableName = "testTbl" + Guid.NewGuid().ToString("N");
				string commandText = string.Format(CultureInfo.InvariantCulture, "CREATE TABLE [dbo].[{0}]([ItemId] [int] NOT NULL) ON [PRIMARY]", tableName);
				using (SqlConnection sqlConn = new SqlConnection(sqlConnectionString))
				{
					sqlConn.Open();
					using (SqlCommand sqlCommand = new SqlCommand(commandText, sqlConn))
					{
						sqlCommand.ExecuteNonQuery();
					}
					commandText = "DROP TABLE [dbo].[" + tableName + "]";
					using (SqlCommand sqlCommand = new SqlCommand(commandText, sqlConn))
					{
						sqlCommand.ExecuteNonQuery();
					}
				}
				returnValue = true;
			}
			catch (System.Data.SqlClient.SqlException eatMe)
			{
				System.Diagnostics.Trace.WriteLine(eatMe.Message);
				returnValue = false;
			}
			finally
			{
				if (null != securityContext)
				{
					securityContext.Dispose();
				}
			}
			return returnValue;
		}
		
		#endregion
		
		#region Sql Utilities
		
		/// <summary>
		/// creates a ReadUnreadItemIds table value paramater
		/// which can be passed to a storedprocedure  from a list of ItemIds
		/// the Type was created during provisioning as
		/// CREATE TYPE ReadUnreadItemIds AS Table (Id INT);
		/// </summary>
		/// <param name="itemIds"></param>
		/// <returns></returns>
		internal static DataTable CreateItemIdsTableParamater(IEnumerable<int> itemIds)
		{
			if (null == itemIds)
			{
				throw new ArgumentNullException("itemIds");
			}
			DataTable returnValue = new DataTable();
			returnValue.Columns.Add("Id", typeof(int));
			foreach (int id in itemIds)
			{
				returnValue.Rows.Add(id);
			}
			return returnValue;
		}
		
		/// <summary>
		/// convert a large list of items to a list of batchs of items
		/// </summary>
		/// <param name="intArray"></param>
		/// <param name="batchSize"></param>
		/// <returns></returns>
		static List<int[]> ConvertToBatchs(int[] intArray, int batchSize)
		{
			List<int[]> returnValue = new List<int[]>();
			List<int> currentBatch = new List<int>();
			if (null == intArray)
			{
				returnValue.Add(null);
			}
			else
			{
				for (int arryIdx = 0; arryIdx < intArray.Length; arryIdx ++)
				{
					currentBatch.Add(intArray[arryIdx]);
					if (currentBatch.Count >= batchSize)
					{
						returnValue.Add(currentBatch.ToArray());
						currentBatch.Clear();
					}
				}
				if (currentBatch.Count > 0)
				{
					returnValue.Add(currentBatch.ToArray());
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// make a statement into a simple transaction
		/// </summary>
		/// <param name="commandText"></param>
		/// <returns></returns>
		static string SqlTransact(string commandText)
		{
			return "BEGIN TRANSACTION  " + commandText + " COMMIT TRANSACTION ";
		}
		
		/// <summary>
		/// check and see if a table exists in the corresponding database
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		internal static bool TableExists(string tableName)
		{
			string sqlQuery = string.Format(CultureInfo.InvariantCulture, "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME = '{0}'", tableName);
			return SqlObjectExists(SqlConnectionString, sqlQuery);
		}
		
		internal static bool TableExists(string sqlConnectionString, string tableName)
		{
			string sqlQuery = string.Format(CultureInfo.InvariantCulture, "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME = '{0}'", tableName);
			return SqlObjectExists(sqlConnectionString, sqlQuery);
		}
		
		/// <summary>
		/// check and see if a type exists in the corresponding database
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		internal static bool TypeExists(string typeName)
		{
			string sqlQuery = string.Format(CultureInfo.InvariantCulture, "SELECT 1 FROM sys.types where is_user_defined =1 and name= '{0}'", typeName);
			return SqlObjectExists(SqlConnectionString, sqlQuery);
		}
		
		/// <summary>
		/// test/detect a connection string security settings
		/// </summary>
		/// <param name="sqlConnectionString"></param>
		/// <returns></returns>
		internal static string VerifyConnectionStringSecuritySettings(string sqlConnectionString)
		{
			if (string.IsNullOrEmpty(sqlConnectionString))
			{
				return sqlConnectionString;
			}
			SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder(sqlConnectionString);
			if (string.IsNullOrEmpty(connBuilder.UserID))
			{
				connBuilder.IntegratedSecurity = true;
			}
			return connBuilder.ToString();
		}
		
		/// <summary>
		/// check a connection string to see if it has an explicit credential
		/// </summary>
		/// <param name="sqlConnectionString"></param>
		/// <returns></returns>
		internal static bool NeedsImpersonation(string sqlConnectionString)
		{
			SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder(sqlConnectionString);
			return connBuilder.IntegratedSecurity;
		}
		
		#endregion
	}
}