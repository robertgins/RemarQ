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
		static bool SolutionIsDeployed()
		{
			bool returnValue = false;
			try
			{
				SPSolution remarQSolution = SPFarm.Local.Solutions["balsamicsolutions.readunreadsitecolumn.wsp"];
				if (null != remarQSolution)
				{
					returnValue = remarQSolution.Deployed;
				}
			}
			catch (IndexOutOfRangeException)
			{
			}
			return returnValue;
		}

		static bool SolutionExists()
		{
			bool returnValue = false;
			try
			{
				SPSolution remarQSolution = SPFarm.Local.Solutions["balsamicsolutions.readunreadsitecolumn.wsp"];
				if (null != remarQSolution)
				{
					returnValue = true;
				}
			}
			catch (IndexOutOfRangeException)
			{
			}
			return returnValue;
		}

		static bool Confirm(string promptText)
		{
			if (string.IsNullOrWhiteSpace(promptText))
			{
				promptText = "Are you sure (yes/no)";
			}
			string confirmText = ReadText(promptText);
			return confirmText.Equals("YES") || confirmText.Equals("Y");
		}

		static bool SettingsExist()
		{
			SPPersistedObject settingsObj = Microsoft.SharePoint.Administration.SPFarm.Local.GetObject(new Guid("{21DBC2A6-54F5-403C-AB77-1F1A32587A37}"));
			return (null != settingsObj );
		}

		static bool Configcheck()
		{
			if (SolutionIsDeployed() && SettingsExist())
			{
				FarmLicense.ExpireCachedObject();
				FarmSettings.ExpireCachedObject();
				if (FarmLicense.License.IsLicensed() && FarmSettings.Settings.TestedOk)
				{
					return true;
				}
				else
				{
					Console.WriteLine("Configuration is incomplete,");
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		static bool RemarQIsActive(SPSite testMe)
		{
			bool returnValue = true;
			try
			{
				SPField remarQField = testMe.RootWeb.AvailableFields["RemarQ"];
				returnValue = (null != remarQField);
			}
			catch (ArgumentException)
			{
				returnValue = false;
			}
			return returnValue;
		}

		static void RunJobs()
		{
			Console.WriteLine("Running Jobs and Cleanup");
			int smallJobs = SqlRemarQ.QueueCount(Constants.ReadUnreadQueueTableName);
			int largeJobs = SqlRemarQ.QueueCount(Constants.ReadUnreadLargeQueueTableName);
			Console.WriteLine(string.Format("Processing batchs for for {0} lists and {1} large lists", smallJobs, largeJobs));
			int batchNum = 1;
			while (SqlRemarQ.QueueCount(Constants.ReadUnreadQueueTableName) > 0)
			{
				Console.Write(string.Format("Starting standard list batch {0}", batchNum));
				StartProgress();
				ListInitializationProcessor listInit = new ListInitializationProcessor(Constants.ReadUnreadQueueTableName,"Utilities");
				listInit.Execute();
				EndProgress();
				Console.Write(string.Format("Completed standard list batch {0}", batchNum));
				batchNum++;
			}
			while (SqlRemarQ.QueueCount(Constants.ReadUnreadLargeQueueTableName) > 0)
			{
				Console.Write(string.Format("Starting large list batch {0}", batchNum));
				StartProgress();
				ListInitializationProcessor listInit = new ListInitializationProcessor(Constants.ReadUnreadLargeQueueTableName,"Utilities");
				listInit.Execute();
				EndProgress();
				Console.Write(string.Format("Completed large list batch {0}", batchNum));
				batchNum++;
			}
		}

		static void RemoveJobs()
		{
			if (SolutionIsDeployed())
			{
				DailyMaintenanceJob.UnInstall();
				ListInitializationJob.UnInstall();
				LargeListInitializationJob.UnInstall();
				InitializationQueueMonitorJob.UnInstall();
			}
		}

		static void AddJobs()
		{
			DailyMaintenanceJob.Install();
			ListInitializationJob.Install();
			LargeListInitializationJob.Install();
			InitializationQueueMonitorJob.Install();
		}

		static void SetUpgradeProperties()
		{
			Console.WriteLine("Setting Upgrade Flags");
			FarmSettings farmSettings = Microsoft.SharePoint.Administration.SPFarm.Local.GetObject(FarmSettings.SettingsId) as FarmSettings;
			if (farmSettings.TrackDocuments)
			{
				farmSettings.UpgradeTrackDocuments = true;
				farmSettings.TrackDocuments = false;
				farmSettings.Update();
			}
		}

		static void UninstallHttpModule()
		{
			if (SolutionIsDeployed())
			{
				Console.WriteLine("Unregistering HttpModules");
				ReadUnreadHttpModule.UnregisterModule(false);
			}
		}

		static void UninstallAllJobs()
		{
			if (SolutionIsDeployed())
			{
				StartProgress();
				Console.WriteLine("Waiting on InitializationQueueMonitorJob");
				while (InitializationQueueMonitorJob.IsRunning)
				{
					System.Threading.Thread.Sleep(1000);
				}
				InitializationQueueMonitorJob.UnInstall();
				Console.WriteLine("InitializationQueueMonitorJob Uninstalled");
				EndProgress();

				StartProgress();
				Console.WriteLine("Waiting on DailyMaintenanceJob");
				while (DailyMaintenanceJob.IsRunning)
				{
					System.Threading.Thread.Sleep(1000);
				}
				DailyMaintenanceJob.UnInstall();
				Console.WriteLine("DailyMaintenanceJob Uninstalled");
				EndProgress();

				StartProgress();
				Console.WriteLine("Waiting on ListInitializationJob");
				while (ListInitializationJob.IsRunning)
				{
					System.Threading.Thread.Sleep(1000);
				}
				ListInitializationJob.UnInstall();
				Console.WriteLine("ListInitializationJob Uninstalled");
				EndProgress();

				StartProgress();
				Console.WriteLine("Waiting on ListInitializationJob");
				while (ListInitializationJob.IsRunning)
				{
					System.Threading.Thread.Sleep(1000);
				}
				ListInitializationJob.UnInstall();
				Console.WriteLine("ListInitializationJob Uninstalled");
				EndProgress();
			}
		}

		static string ReadText(string promptText, bool caseSensitive = false, string passwordChar = null)
		{
			string returnValue = string.Empty;
			if (!string.IsNullOrWhiteSpace(promptText))
			{
				Console.Write(promptText);
				Console.Write(" ? ");
			}
			if (null != passwordChar)
			{
				passwordChar = passwordChar.Substring(0, 1);
				ConsoleKeyInfo consoleKey;
				do
				{
					consoleKey = Console.ReadKey(true);
					if (consoleKey.Key != ConsoleKey.Backspace && consoleKey.Key != ConsoleKey.Enter)
					{
						returnValue += consoleKey.KeyChar;
						Console.Write(passwordChar);
					}
					else
					{
						if (consoleKey.Key == ConsoleKey.Backspace && returnValue.Length > 0)
						{
							returnValue = returnValue.Substring(0, (returnValue.Length - 1));
							Console.Write("\b \b");
						}
					}
				}
				// Stops Receving Keys Once Enter is Pressed
				while (consoleKey.Key != ConsoleKey.Enter);
				Console.WriteLine();
			}
			else
			{
				returnValue = Console.ReadLine();
				if (!caseSensitive)
				{
					returnValue = returnValue.ToUpperInvariant().Trim();
				}
			}
			return returnValue;
		}

		static void ParseCommandLine(string cmdLine, out string cmdName, out string paramOne, out string paramTwo)
		{
			cmdName = string.Empty;
			paramOne = string.Empty;
			paramTwo = string.Empty;
			cmdLine = cmdLine.ToUpperInvariant().Trim().Replace("\"", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
			string[] cmdParts = cmdLine.Split(' ');
			cmdName = cmdParts[0];
			if (cmdParts.Length > 1)
			{
				paramOne = cmdParts[1];
			}
			if (cmdParts.Length > 2)
			{
				paramOne = cmdParts[2];
			}
		}

		static void StartProgress()
		{
			Console.WriteLine();
			_ProgressTimer.Enabled = true;
		}

		static void EndProgress()
		{
			_ProgressTimer.Enabled = false;
			Console.WriteLine();
		}

		static void ProgressTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Console.Write(".");
		}
	}
}