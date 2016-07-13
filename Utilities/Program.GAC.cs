// -----------------------------------------------------------------------------
//  Copyright 8/23/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BalsamicSolutions.ReadUnreadSiteColumn;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace Utilities
{
	partial class Program
	{
		static void FixGac()
		{
			string filePath = Path.Combine(Environment.CurrentDirectory, "BalsamicSolutions.ReadUnreadSiteColumn.dll");
			if (File.Exists(filePath))
			{
				Console.WriteLine("Stopping services");
				_ProgressTimer.Enabled = true;
				bool startSPTimerV4 = StopService("SPTimerV4");
				bool startSPAdminV4 = StopService("SPAdminV4");
				bool startSPSearchHostController = StopService("SPSearchHostController");
				bool startOSearch15 = StopService("OSearch15");
				bool startSPTraceV4 = StopService("SPTraceV4");
				bool startSPUserCodeV4 = StopService("SPUserCodeV");
				bool startSPWriterV4 = StopService("SPWriterV4");
				bool startIISADMIN = StopService("IISADMIN");
				bool startW3SVC = StopService("W3SVC");
				_ProgressTimer.Enabled = false;
				Console.WriteLine("Publishing Assembly");
				Publish(filePath);
				Console.WriteLine("Starting services");
				_ProgressTimer.Enabled = true;
				if (startSPTimerV4)
				{
					StartService("SPTimerV4");
				}
				if (startSPAdminV4)
				{
					StartService("SPAdminV4");
				}
				if (startSPSearchHostController)
				{
					StartService("SPSearchHostController");
				}
				if (startOSearch15)
				{
					StartService("OSearch15");
				}
				if (startSPTraceV4)
				{
					StartService("SPTraceV4");
				}
				if (startSPUserCodeV4)
				{
					StartService("SPUserCodeV");
				}
				if (startSPWriterV4)
				{
					StartService("SPWriterV4");
				}
				if (startIISADMIN)
				{
					StartService("IISADMIN");
				}
				if (startW3SVC)
				{
					StartService("W3SVC");
				}
				_ProgressTimer.Enabled = false;
				Console.WriteLine("Assembly has been updated");
			}
			else
			{
				Console.WriteLine("Assembly is missing, cannot reinstall it");
			}
		}

		//https://powershellgac.codeplex.com/SourceControl/latest#PowerShellGac/PowerShellGac/GlobalAssemblyCache.cs
		//TODO fix this up a bit with ps file open checking
		static void Publish(string assemblyPath)
		{
			var pub = new System.EnterpriseServices.Internal.Publish();
			pub.GacRemove(assemblyPath);
			pub.GacInstall(assemblyPath);

		}

		static void StartService(string serviceName)
		{
			ServiceController svcControler = new ServiceController(serviceName);
			svcControler.Start();
			svcControler.WaitForStatus(ServiceControllerStatus.Running);
		}

		static bool StopService(string serviceName)
		{
			bool returnValue = false;
			try
			{
				ServiceController svcControler = new ServiceController(serviceName);
				if (svcControler.Status != ServiceControllerStatus.Stopped)
				{
					returnValue = svcControler.Status != ServiceControllerStatus.StopPending ;
					if (svcControler.Status != ServiceControllerStatus.StopPending)
					{
						svcControler.Stop();
					}
					svcControler.WaitForStatus(ServiceControllerStatus.Stopped);
				}
			}
			catch (ArgumentException)
			{
				//service does not exist
				returnValue = false;
			}
			return returnValue;
		}
	}
}