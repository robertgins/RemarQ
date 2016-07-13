// -----------------------------------------------------------------------------
//  Copyright 9/4/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Administration.Claims;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	/// <summary>
	/// misc utilities
	/// </summary>
	internal static class Utilities
	{
		public static void InstallJobsIfValid()
		{
			FarmLicense.ExpireCachedObject();
			FarmSettings.ExpireCachedObject();
			if (FarmLicense.License.IsLicensed() && FarmSettings.Settings.TestedOk)
			{
				DailyMaintenanceJob.Install();
				LargeListInitializationJob.Install();
				ListInitializationJob.Install();
				InitializationQueueMonitorJob.Install();
			}
		}

		public static void UninstallAllJobs()
		{
			
			InitializationQueueMonitorJob.UnInstall();
			DailyMaintenanceJob.UnInstall();
			LargeListInitializationJob.UnInstall();
			ListInitializationJob.UnInstall();
			RemoveRemarQJob.UnInstall();
		}

		/// <summary>
		/// create a CSS link tag
		/// </summary>
		/// <param name="cssUrl"></param>
		/// <returns></returns>
		internal static string CssLink(string cssUrl)
		{
			if (null == cssUrl)
			{
				throw new ArgumentNullException("cssUrl");
			}
			return string.Format(CultureInfo.InvariantCulture, "<link rel='stylesheet' href='{0}' type='text/css' />", cssUrl.Trim());
		}

		/// <summary>
		/// create a javascript link tag
		/// </summary>
		/// <param name="javaScriptUrl"></param>
		/// <returns></returns>
		internal static string JavaScriptLink(string javaScriptUrl)
		{
			if (null == javaScriptUrl)
			{
				throw new ArgumentNullException("javaScriptUrl");
			}
			return string.Format(CultureInfo.InvariantCulture, "<script type='text/javascript' src='{0}'></script>", javaScriptUrl.Trim());
		}

		/// <summary>
		/// remove all the junk from a sharepoint path name
		/// </summary>
		/// <param name="folderPath"></param>
		/// <returns></returns>
		internal static string CleanUpSharePointPathName(string folderPath)
		{
			if (null == folderPath)
			{
				throw new ArgumentNullException("folderPath");
			}
			string returnValue = folderPath;
			if (-1 != returnValue.IndexOf(";#", StringComparison.OrdinalIgnoreCase))
			{
				returnValue = returnValue.Substring(returnValue.IndexOf(";#", StringComparison.OrdinalIgnoreCase) + 2);
			}
			return returnValue;
		}
       
		/// <summary>
		/// find all read unread fields on a list
		/// </summary>
		/// <param name="sourceList"></param>
		/// <returns></returns>
		internal static List<ReadUnreadField> FindFieldsOnList(SPList sourceList)
		{
			List<ReadUnreadField> returnValue = new List<ReadUnreadField>();
			if (null != sourceList)
			{
				foreach (SPField tempField in sourceList.Fields)
				{
					ReadUnreadField tempCast = tempField as ReadUnreadField;
					if (null != tempCast)
					{
						returnValue.Add(tempCast);
					}
				}
			}
			return returnValue;
		}

		/// <summary>
		/// find the first read / unread field on the list
		/// </summary>
		/// <param name="sourceList"></param>
		/// <returns></returns>
		internal static ReadUnreadField FindFirstFieldOnList(SPList sourceList)
		{
			ReadUnreadField returnValue = null;
			if (null != sourceList)
			{
				foreach (SPField tempField in sourceList.Fields)
				{
					returnValue = tempField as ReadUnreadField;
					if (null != returnValue)
					{
						break;
					}
				}
			}
			return returnValue;
		}
		
		/// <summary>
		/// get an embedded resource as a string
		/// </summary>
		/// <param name="resourceName"></param>
		/// <returns></returns>
		internal static string GetEmbeddedResourceString(string resourceName)
		{
			Assembly thisAssm = Assembly.GetExecutingAssembly();
			using (System.IO.StreamReader templateStream = new System.IO.StreamReader(thisAssm.GetManifestResourceStream(resourceName)))
			{
				return templateStream.ReadToEnd();
			}
		}
		
		/// <summary>
		/// get an embedded resource as a byte array
		/// </summary>
		/// <param name="resourceName"></param>
		/// <returns></returns>
		internal static byte[] GetEmbeddedResource(string resourceName)
		{
			Assembly thisAssm = Assembly.GetExecutingAssembly();
			using (Stream templateStream = thisAssm.GetManifestResourceStream(resourceName))
			{
				using (MemoryStream returnValue = new MemoryStream())
				{
					templateStream.CopyTo(returnValue);
					returnValue.Seek(0, SeekOrigin.Begin);
					return returnValue.ToArray();
				}
			}
		}

		/// <summary>
		/// create a des decryption engine
		/// </summary>
		/// <param name="passwordKey"></param>
		/// <returns></returns>
		static ICryptoTransform GetDecryptor(string passwordKey)
		{
			using (MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider())
			{
				using (TripleDESCryptoServiceProvider desEncryption = new TripleDESCryptoServiceProvider())
				{
					desEncryption.Mode = CipherMode.ECB;
					desEncryption.Key = md5Provider.ComputeHash(Encoding.Unicode.GetBytes(passwordKey));
					return desEncryption.CreateDecryptor();
				}
			}
		}
    
		/// <summary>
		/// create a DES encryption engine
		/// </summary>
		/// <param name="passwordKey"></param>
		/// <returns></returns>
		static ICryptoTransform GetEncryptor(string passwordKey)
		{
			using (MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider())
			{
				using (TripleDESCryptoServiceProvider desEncryption = new TripleDESCryptoServiceProvider())
				{
					desEncryption.Mode = CipherMode.ECB;
					desEncryption.Key = md5Provider.ComputeHash(Encoding.Unicode.GetBytes(passwordKey));
					return desEncryption.CreateEncryptor();
				}
			}
		}
        
		/// <summary>
		/// encrypt bytes
		/// </summary>
		/// <param name="encryptMe"></param>
		/// <param name="encryptionKey"></param>
		/// <returns></returns>
		internal static byte[] EncryptBytes(byte[] encryptMe, string encryptionKey)
		{
			using (ICryptoTransform cryptoTransform = GetEncryptor(encryptionKey))
			{
				return cryptoTransform.TransformFinalBlock(encryptMe, 0, encryptMe.Length);
			}
		}

		/// <summary>
		/// decrypt bytes
		/// </summary>
		/// <param name="decryptMe"></param>
		/// <param name="encryptionKey"></param>
		/// <returns></returns>
		internal static byte[] DecryptBytes(byte[] decryptMe, string encryptionKey)
		{
			using (ICryptoTransform cryptoTransform = GetDecryptor(encryptionKey))
			{
				return cryptoTransform.TransformFinalBlock(decryptMe, 0, decryptMe.Length);
			}
		}

		/// <summary>
		/// encrypt a string
		/// </summary>
		/// <param name="encryptMe"></param>
		/// <param name="encryptionKey"></param>
		/// <returns></returns>
		internal static string EncryptString(string encryptMe, string encryptionKey)
		{
			return Convert.ToBase64String(EncryptBytes(Encoding.Unicode.GetBytes(encryptMe), encryptionKey));
		}

		/// <summary>
		/// decrypt a string
		/// </summary>
		/// <param name="decryptMe"></param>
		/// <param name="encryptionKey"></param>
		/// <returns></returns>
		internal static string DecryptString(string decryptMe, string encryptionKey)
		{
			return Encoding.Unicode.GetString(DecryptBytes(Convert.FromBase64String(decryptMe), encryptionKey));
		}

		/// <summary>
		/// checks to see if a job is defined in the scope
		/// </summary>
		/// <param name="farm"></param>
		/// <param name="jobName"></param>
		/// <returns></returns>
		internal static bool IsJobDefined(SPService hostService, string jobName)
		{
			foreach (SPJobDefinition definedJob in hostService.JobDefinitions)
			{
				if (jobName.Equals(definedJob.Name, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		///// <summary>
		///// Determines whether the specified timer job is currently running (or
		///// scheduled to run).
		///// </summary>
		///// <param name="farm">The farm to check if the job is running on.</param>
		///// <param name="jobName">The title of the timer job.</param>
		///// <returns><c>true</c> if the specified timer job is currently running
		///// (or scheduled to run); otherwise <c>false</c>.</returns>
		//internal static bool IsJobRunning(SPService hostService, string jobName)
		//{
			 
		//		foreach (SPRunningJob runningJob in hostService.RunningJobs)
		//		{
		//			System.Diagnostics.Trace.WriteLine(runningJob.JobDefinitionTitle);
		//			if (null != runningJob.JobDefinition && jobName.Equals(runningJob.JobDefinition.Name, StringComparison.OrdinalIgnoreCase))
		//			{
		//				return true;
		//			}
		//		}
			 

		//	return false;
		//}
		
		///// <summary>
		///// Waits for a one-time SharePoint timer job to finish.
		///// </summary>
		///// <param name="farm">The farm on which the timer job runs.</param>
		///// <param name="jobName">The name of the timer job (e.g. job-webconfig-modifications </param>
		///// <param name="maximumWaitTime">The maximum time (in seconds) to wait
		///// for the timer job to finish.</param>
		//internal static void WaitForOnetimeJobToFinish(SPService hostService, string jobName, byte maximumWaitTime)
		//{
		//	float waitTime = 0;
		//	bool isJobDefined = IsJobDefined(hostService, jobName);
		//	bool isJobRunning = IsJobRunning(hostService, jobName);
			 
		//	while ((isJobDefined || isJobRunning) && waitTime < maximumWaitTime)
		//	{
		//		int sleepTime = 500; // milliseconds

		//		Thread.Sleep(sleepTime);
		//		waitTime += (sleepTime / 1000.0F); // seconds
		//		isJobDefined = IsJobDefined(hostService, jobName);
		//		isJobRunning = IsJobRunning(hostService, jobName);
		//	}
			 
		//}
	}
}