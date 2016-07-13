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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BalsamicSolutions.ReadUnreadSiteColumn;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace Utilities
{
	//remove warning about redeclaring Versions class, we want to use
	//the local version so it wont load the solution assembly
	#pragma warning disable 436
	/// <summary>
	/// command processor for batch marking existing lists
	/// originally used for load test, to be made available
	/// to customers upon requst
	/// </summary>
	partial class Program
	{
		static System.Timers.Timer _ProgressTimer = new System.Timers.Timer(250);

		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolver);
			//AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
			if (null != args && args.Length > 0)
			{
				switch(args[0].ToUpperInvariant().Trim())
				{
					case "/REGISTER":
					case "REGISTER":
						UpdateSPTimerV4AssemblyRedirection(false);
						break;
					case "/UNREGISTER":
					case "UNREGISTER":
						UpdateSPTimerV4AssemblyRedirection(true);
						break;
					case "/RESET":
					case "RESET":
						ResetFarmSettings();
						break;
				}
			}
			else
			{
				Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
				{
					e.Cancel = true;
				};
				ServicePointManager.ServerCertificateValidationCallback = AllowAnyServerCertificate;
				Console.WriteLine("press any  key to initialize...");
				Console.ReadKey();
				RemoveJobs();
				if (Configcheck())
				{
					try
					{
						SqlRemarQ.ProvisionConfigurationTables();
						Console.WriteLine("Configuration has been validated");
					}
					catch
					{
						Console.WriteLine("Configuration is inoperable it will be purged");
						try
						{
							FarmSettings.Remove();
						}
						catch
						{
						}
					}
				}
				else
				{
					Console.WriteLine("Configuration is not valid");
				}
				_ProgressTimer = new System.Timers.Timer(250);
				_ProgressTimer.Enabled = false;
				_ProgressTimer.Elapsed += ProgressTimer_Elapsed;
				bool allDone = false;
				string paramOne = string.Empty;
				string paramTwo = string.Empty;
				string cmdName = string.Empty;
				try
				{
					while (!allDone)
					{
						Console.WriteLine();
						Console.WriteLine("Enter a command (help for a list of commands)");
						string cmdLine = Console.ReadLine();
						ParseCommandLine(cmdLine, out cmdName, out paramOne, out paramTwo);
						switch (cmdName)
						{
							case "VER":
							case "VERSION":
									{
										var infoAttribute = FarmSettings.InformationVersion;
										Console.WriteLine(BalsamicSolutions.ReadUnreadSiteColumn.Versions.ASSEMBLY + "/" + infoAttribute.InformationalVersion);
									}
								break;
							case "REMOVE":
									{
										if (SolutionIsDeployed())
										{
											if (Confirm("This will remove licensing and configuration ! Are you really sure "))
											{
												Remove();
											}
										}
										else
										{
											Console.WriteLine("RemarQ is not deployed");
										}
									}
								break;
							case "VERIFY":
									{
										if (SolutionIsDeployed())
										{
											if (Confirm(null))
											{
												Validate();
											}
										}
										else
										{
											Console.WriteLine("RemarQ is not deployed");
										}
									}
								break;
							case "INITIALIZE":
									{
										if (SolutionIsDeployed())
										{
											if (Configcheck())
											{
												if (Confirm(null))
												{
													RunJobs();
												}
											}
										}
										else
										{
											Console.WriteLine("RemarQ is not deployed");
										}
									}
								break;
							case "DISABLE":
									{
										if (SolutionIsDeployed())
										{
											if (Configcheck())
											{
												if (Confirm(null))
												{
													Disable();
												}
											}
										}
										else
										{
											Console.WriteLine("RemarQ is not deployed");
										}
									}
								break;
							case "ENABLE":
									{
										if (SolutionIsDeployed())
										{
											if (Configcheck())
											{
												if (Confirm(null))
												{
													Enable();
												}
											}
										}
										else
										{
											Console.WriteLine("RemarQ is not deployed");
										}
									}
								break;
							case "ACTIVATE":
									{
										if (SolutionIsDeployed())
										{
											if (Configcheck())
											{
												Activate();
											}
										}
										else
										{
											Console.WriteLine("RemarQ is not deployed");
										}
									}
								break;
							case "CREATE":
									{
										if (paramOne.Equals("DATABASE"))
										{
											try
											{
												CreateDatabase();
											}
											catch (SqlException sqlError)
											{
												Console.WriteLine(sqlError.Message);
												Console.WriteLine("CREATE DATABASE failed");
											}
										}
									}
								break;
							case "GAC":
							case "FIXGAC":
									{
										if (Confirm("This will force all SharePoint and IIS services locally offline and republish solution binary files to the global assembly cache! Are you really sure "))
										{
											FixGac();
										}
									}
								break;
							case "EXTERNALJOBMANAGER":
									{
										if (Confirm(null))
										{
											FarmSettings settings = Microsoft.SharePoint.Administration.SPFarm.Local.GetObject(FarmSettings.SettingsId) as FarmSettings;
											if (null != settings)
											{
												settings.ExternalJobManager = true;
												settings.Update();
											}
											else
											{
												Console.WriteLine("RemarQ is not configured");
											}
										}
									}
								break;
							case "SETTINGS":
									{
										if (paramOne.Equals("DELETE"))
										{
											FarmSettings.Remove();
											Console.WriteLine("Settings deleted");
											break;
										}
									}
								break;
						
							case "INTERNALJOBMANAGER":
									{
										FarmSettings settings = Microsoft.SharePoint.Administration.SPFarm.Local.GetObject(FarmSettings.SettingsId) as FarmSettings;
										if (null != settings)
										{
											settings.ExternalJobManager = false;
											settings.Update();
										}
										else
										{
											Console.WriteLine("RemarQ is not configured");
										}
									}
								break;
							case "REGISTER":
									{
										if (SolutionIsDeployed())
										{
											UpdateSPTimerV4AssemblyRedirection(false);
											if (paramOne.Equals("ALL"))
											{
												PropagateOwsTimerConfig();
											}
											Console.WriteLine("Registration complete");
										}
										else
										{
											Console.WriteLine("RemarQ is not installed use Install");
										}
									}
								break;
							case "UNREGISTER":
									{
										if (SolutionIsDeployed())
										{
											UpdateSPTimerV4AssemblyRedirection(true);
											if (paramOne.Equals("ALL"))
											{
												PropagateOwsTimerConfig();
											}
											Console.WriteLine("Deregistration complete");
										}
										else
										{
											Console.WriteLine("RemarQ is not installed use Install");
										}
									}
								break;
							case "FSACTIVATE":
									{
										FarmSettings farmSettings = SPFarm.Local.GetObject(FarmSettings.SettingsId) as  FarmSettings;
										if (null != farmSettings)
										{
											farmSettings.Activated = true;
											farmSettings.Update();
										}
									}
								break;
							case "FSDEACTIVATE":
									{
										FarmSettings farmSettings = SPFarm.Local.GetObject(FarmSettings.SettingsId) as  FarmSettings;
										if (null != farmSettings)
										{
											farmSettings.Activated = false;
											farmSettings.Update();
										}
									}
								break;
							case "RESET":
									{
										if (Confirm(null))
										{
											FarmSettings settings = Microsoft.SharePoint.Administration.SPFarm.Local.GetObject(FarmSettings.SettingsId) as FarmSettings;
											if (null != settings)
											{
												ResetFarmSettings();
											}
											else
											{
												Console.WriteLine("RemarQ is not configured");
											}
										}
									}
								break;
							case "EXIT":
							case "QUIT":
							case "BYE":
								allDone = true;
								break;
							case "HELP":
							case "?":
							default:
									{
										//Console.WriteLine();
										//Console.WriteLine("Install: starts a solution installation and deployment with the FORCE option");
										//Console.WriteLine();
										//Console.WriteLine("Upgrade: starts a solution ugprade");
										//Console.WriteLine();
										//Console.WriteLine("Reinstall: removes and reinstalls the solution");
										//Console.WriteLine();
										//Console.WriteLine("Uninstall: removes the solution");
										//Console.WriteLine();
										Console.WriteLine("License (export or import): Exports or Imports the license and displays the current license status");
										Console.WriteLine();
										Console.WriteLine("Register: updates the assembly version registration for the SharePoint timer service on this server");
										Console.WriteLine();
										Console.WriteLine("Unregister: removes the assembly version registration for the SharePoint timer service on this server");
										Console.WriteLine();
										Console.WriteLine("Enable all: starts a process to enable all possible lists");
										Console.WriteLine();
										Console.WriteLine("Disable all: starts a process to remove the field from all configured locations");
										Console.WriteLine();
										Console.WriteLine("Remove all: starts a process to scan for and remove all RemarQ artifacts");
										Console.WriteLine();
										Console.WriteLine("Initialize: runs list initialization jobs until queue is empty");
										Console.WriteLine();
										Console.WriteLine("Activate: activates the RemarQ feature on all SharePoint web sites");
										Console.WriteLine();
										Console.WriteLine("Create database: initializes the RemarQ database, if it does not exist");
										Console.WriteLine();
										Console.WriteLine("Verify: scans all the configured RemarQ indicator tables");
										Console.WriteLine();
										Console.WriteLine("Reset: reapplies settings after an upgrade");
										Console.WriteLine();
										Console.WriteLine("Exit : exits this application");
										Console.WriteLine();
									}
								break;
						}
					}
				}
				catch (Exception dangNabError)
				{
					if (System.IO.File.Exists("ErrorLog.txt"))
					{
						System.IO.File.Delete("ErrorLog.txt");
					}
					string messageLog = dangNabError.Message + "\r\n***************************\r\n" + dangNabError.StackTrace;
					System.IO.File.WriteAllText("ErrorLog.txt", messageLog);
					Console.WriteLine(dangNabError.Message + "(details are in he ErrorLog.txt file)");
					Console.WriteLine("press any key to exit");
					Console.ReadKey();
				}
				if (SolutionIsDeployed())
				{
					if (Confirm("Re-enable jobs"))
					{
						AddJobs();
					}
				}
			}
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
			}
			return null;
		}

		static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			if (args.LoadedAssembly.FullName.StartsWith("BalsamicSolutions.ReadUnreadSiteColumn", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("ASSEMBLY LOADED FROM: " + args.LoadedAssembly.CodeBase);
			}
		}

		public static bool AllowAnyServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}
}


//case "UPGRADE":
//		{
//			if (SolutionExists())
//			{
//				if (Configcheck())
//				{
//					if (Confirm(null))
//					{
//						if (RemoveRemarQJob.IsRunning)
//						{
//							Console.WriteLine("Cannot process an upgrade while a RemoveEverything job is running");
//						}
//						else
//						{
//							Upgrade();
//						}
//					}
//				}
//				else
//				{
//					Console.WriteLine("RemarQ is not configured, use Reinstall");
//				}
//			}
//			else
//			{
//				Console.WriteLine("RemarQ is not installed");
//			}
//		}
//	break;
//case "INSTALL":
//		{
//			if (!SolutionExists())
//			{
//				if (Confirm(null))
//				{
//					InstallSolution();
//				}
//			}
//			else
//			{
//				Console.WriteLine("RemarQ is already installed use Upgrade or Reinstall");	
//			}
//		}
								 
//	break;
//case "REINSTALL":
//		{
//			if (SolutionExists())
//			{
//				if (Confirm(null))
//				{
//					ReInstallSolution();
//				}
//			}
//			else
//			{
//				Console.WriteLine("RemarQ is not installed use Install");
//			}
//		}
								 
//	break;
//case "UNINSTALL":
//		{
//			if (SolutionExists())
//			{
//				if (Confirm(null))
//				{
//					UninstallSolution();
//				}
//			}
//			else
//			{
//				Console.WriteLine("RemarQ is not installed");
//			}
//		}
								 
//	break;