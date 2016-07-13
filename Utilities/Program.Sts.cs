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