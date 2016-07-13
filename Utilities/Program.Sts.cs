// -----------------------------------------------------------------------------
//  Copyright 7/9/2014 (c) Balsamic Solutions, Inc. All rights reserved.
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
		static void ShellStsAdm(string commandLine)
		{
			string stsPath = Environment.GetEnvironmentVariable("CommonProgramFiles");
			stsPath += @"\microsoft shared\Web Server Extensions\15\bin\stsadm.exe";
			if (!System.IO.File.Exists(stsPath))
			{
				throw new System.IO.FileNotFoundException(stsPath);
			}
			Process shellProcess = new Process
									   {
										   StartInfo =
										   {
											   FileName = stsPath,
											   Arguments = commandLine,
											   UseShellExecute = false,
											   RedirectStandardOutput = true,
											   RedirectStandardError = true
										   }
									   };
			shellProcess.OutputDataReceived += ShellProcess_OutputDataReceived;
			shellProcess.ErrorDataReceived += ShellProcess_ErrorDataReceived;
			shellProcess.Start();
			shellProcess.BeginOutputReadLine();
			shellProcess.WaitForExit();
		}

		static void ShellProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			Console.WriteLine(e.Data);
		}

		static void ShellProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			throw new InvalidProgramException(e.Data);
		}
	}
}