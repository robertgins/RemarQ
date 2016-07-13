// -----------------------------------------------------------------------------
//  Copyright 8/31/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//TODO : Add logging, test, configuration checks ? 
namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolver);
			bool runService = false;
			bool showConfirmation = true;
			if (args.Length > 0)
			{
				if (args.Length > 1)
				{
					string temp = args[1].ToLower().Trim();
					if ((temp == "/quiet" || temp == "quiet" || temp == "/q" || temp == "q"))
					{
						showConfirmation = false;
					}
				}
				switch ( args[0].ToLower().Trim() )
				{
					case "/installservice":
						try
						{
							System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
							if (showConfirmation)
							{
								MessageBox.Show("Service installation succeeded.", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
							}
						}
						catch (Exception ex)
						{
							if (showConfirmation)
							{
								MessageBox.Show("Unable to install service due to the following error:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
								MessageBox.Show(GetFullException(ex, true), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
								Environment.Exit(0x000001f);
							}
						}
						break;
					case "/uninstallservice":
						try
						{
							System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
							if (showConfirmation)
							{
								MessageBox.Show("Service uninstallation succeeded.", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
							}
						}
						catch (Exception ex)
						{
							if (showConfirmation)
							{
								MessageBox.Show("Unable to uninstall service due to the following error:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
								Environment.Exit(0x000001f);
							}
						}
						break;
					case "/runonce":
						RunOnce();
						break;
					default:
						runService = true;
						break;
				}
			}

			if (runService)
			{
				ServiceBase[] servicesToRun;
				 
				RemarQListProcessor	remarQService = new RemarQListProcessor(Application.ExecutablePath);
				servicesToRun = new ServiceBase[] { remarQService };
				ServiceBase.Run(servicesToRun);
			}
		}
	 
		internal static void RunOnce()
		{
			try
			{
				RemarQListProcessor	remarQService = new RemarQListProcessor(Application.ExecutablePath);
				remarQService.RunOnce();
			}
			catch (Exception ex)
			{
				string errorMessage = GetFullException(ex, true);
				string errorLogTextFile = Path.Combine(Application.ExecutablePath, "ErrorLog.txt");
				if (File.Exists(errorLogTextFile))
				{
					File.AppendAllText(errorLogTextFile, errorMessage);
				}
				else
				{
					File.WriteAllText(errorLogTextFile, errorMessage);
				}
				MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		internal static string GetFullException(Exception exception, bool showStacktrace)
		{
			System.Text.StringBuilder fullError = new System.Text.StringBuilder();
			fullError.AppendLine("Exception Type:" + exception.GetType().ToString());
			fullError.AppendLine("Message:" + exception.Message);
			if (showStacktrace)
			{
				fullError.Append("StackTrace:" + exception.StackTrace + "\r\n");
			}
			Exception inner = exception.InnerException;
			while (inner != null)
			{
				fullError.AppendLine("Inner Type:" + inner.GetType().ToString());
				fullError.AppendLine("Inner Message:" + inner.Message);
				if (showStacktrace)
				{
					fullError.Append("Inner Stacktrace:" + inner.StackTrace + "\r\n");
				}
				inner = inner.InnerException;
			}
			return fullError.ToString();
		}

		/// <summary>
		/// force this utility to load here instead of gac
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private static Assembly CurrentDomain_AssemblyResolver(object sender, ResolveEventArgs args)
		{
			if (args.Name.StartsWith("BalsamicSolutions.ReadUnreadSiteColumn", StringComparison.OrdinalIgnoreCase))
			{
				string filePath = Path.Combine(Environment.CurrentDirectory, "BalsamicSolutions.ReadUnreadSiteColumn.dll");
				if (File.Exists(filePath))
				{
					return Assembly.LoadFile(filePath);				
				}
				else
				{
					throw new InvalidProgramException("Cannot locate local copy of BalsamicSolutions.ReadUnreadSiteColumn.dll");
				}
			}
			return null;
		}
	}
}