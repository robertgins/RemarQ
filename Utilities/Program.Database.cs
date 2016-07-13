// -----------------------------------------------------------------------------
//  Copyright 8/19/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
 
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BalsamicSolutions.ReadUnreadSiteColumn;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Utilities
{
	partial class Program
	{
		static void CreateDatabase()
		{
			string sqlServerName = ReadText("Sql server");
			string sqlUser = ReadText("Sql user (enter for current login)");
			string sqlPassword = ReadText("Sql user password (enter for current login)", true, "*");
			string remarQDbName = ReadText("RemarQ database name (enter for RemarQ)");
			if (string.IsNullOrWhiteSpace(remarQDbName))
			{
				remarQDbName = "RemarQ";
			}
			if (!string.IsNullOrWhiteSpace(sqlServerName) && Confirm(null))
			{
				SqlConnectionStringBuilder csB = new SqlConnectionStringBuilder();
				csB.DataSource = sqlServerName;
				if (!string.IsNullOrWhiteSpace(sqlUser))
				{
					csB.UserID = sqlUser;
					csB.Password = sqlPassword;
				}
				else
				{
					csB.IntegratedSecurity = true;
				}
				csB.InitialCatalog = "Master";
				using (SqlConnection sqlConn = new SqlConnection(csB.ToString()))
				{
					sqlConn.Open();
					bool dbExists = false;
					string existsQuery = string.Format("SELECT 1 FROM sys.databases where name= '{0}'", remarQDbName);
					using (SqlCommand sqlCommand = new SqlCommand(existsQuery, sqlConn))
					{
						using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
						{
							if (sqlReader.Read())
							{
								dbExists = (1 == sqlReader.GetInt32(0));
							}
						}
					}
					if (dbExists)
					{
						Console.WriteLine(string.Format("Database {0} exists, use Admin web interfaces to connecto to it", remarQDbName));
					}
					else
					{
						RunCommand(sqlConn, string.Format("CREATE DATABASE [{0}]", remarQDbName));
						RunCommand(sqlConn, string.Format("ALTER DATABASE [{0}] SET RECOVERY SIMPLE", remarQDbName));
						RunCommand(sqlConn, string.Format("ALTER DATABASE [{0}] SET ANSI_NULL_DEFAULT OFF", remarQDbName));
						RunCommand(sqlConn, string.Format("ALTER DATABASE [{0}] SET ANSI_NULLS OFF", remarQDbName));
						RunCommand(sqlConn, string.Format("ALTER DATABASE [{0}] SET ANSI_PADDING OFF", remarQDbName));
						RunCommand(sqlConn, string.Format("ALTER DATABASE [{0}] SET ANSI_WARNINGS OFF", remarQDbName));
						RunCommand(sqlConn, string.Format("ALTER DATABASE [{0}] SET AUTO_CLOSE OFF", remarQDbName));
						RunCommand(sqlConn, string.Format("ALTER DATABASE [{0}] SET ARITHABORT OFF", remarQDbName));
						RunCommand(sqlConn, string.Format("ALTER DATABASE [{0}] SET PARAMETERIZATION SIMPLE", remarQDbName));
						RunCommand(sqlConn, string.Format("ALTER DATABASE [{0}] SET CURSOR_CLOSE_ON_COMMIT OFF", remarQDbName));
						RunCommand(sqlConn, string.Format("ALTER DATABASE [{0}] SET  MULTI_USER", remarQDbName));
						RunCommand(sqlConn, string.Format("ALTER DATABASE [{0}] SET  READ_WRITE", remarQDbName));

						Console.WriteLine(string.Format("Created database {0}", remarQDbName));
						string loginName = "_" + remarQDbName;
						string dropLoginAndIgnoreError = string.Format("DROP LOGIN [{0}]",loginName);
						using (SqlCommand sqlCommand = new SqlCommand(dropLoginAndIgnoreError, sqlConn))
						{
							try
							{
								sqlCommand.ExecuteNonQuery();
							}
							catch (SqlException)
							{
							}
						}

						string randomPassword = "p" + Guid.NewGuid().ToString("N").ToUpperInvariant() + "!";

						string createSqlUser = string.Format("CREATE LOGIN [{2}] WITH PASSWORD=N'{0}', DEFAULT_DATABASE=[{1}], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF", randomPassword, remarQDbName,loginName);
						RunCommand(sqlConn, createSqlUser);

						Console.WriteLine(string.Format("Created user {0}",loginName));
						RunCommand(sqlConn, string.Format("USE [{0}]", remarQDbName));
						RunCommand(sqlConn, string.Format("CREATE USER [{0}] FOR LOGIN [{0}] WITH DEFAULT_SCHEMA=[dbo]",loginName));
						RunCommand(sqlConn, string.Format("exec sp_addrolemember 'db_owner', '{0}'",loginName));

						if (Confirm("Install farm settings"))
						{
							FarmSettings settings = FarmSettings.Settings;
							csB.InitialCatalog = remarQDbName;
							csB.IntegratedSecurity = false;
							csB.UserID = loginName;
							csB.Password = randomPassword;
							settings.SqlConnectionString = csB.ToString();
							settings.TestedOk = true;
							settings.Update();
							FarmSettings.ExpireCachedObject();

							Console.WriteLine("Farm Settings updated");
							SqlRemarQ.ProvisionConfigurationTables();
						}
						else
						{
							Console.WriteLine("Password for " + loginName + "  is " + randomPassword);
						}
						Console.WriteLine("Database initialized");
					}
				}
			}
		}

		static void RunCommand(SqlConnection sqlConn, string cmdText)
		{
			using (SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConn))
			{
				sqlCommand.ExecuteNonQuery();
			}
		}
	}
}