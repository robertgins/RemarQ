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
		static void InstallSolution()
		{
			UpdateSPTimerV4AssemblyRedirection(false);
			string filePath = Path.Combine(Environment.CurrentDirectory, "BalsamicSolutions.ReadUnreadSiteColumn.wsp");
			SPSolution remarQSolution = SPFarm.Local.Solutions.Add(filePath);
			Collection<SPWebApplication> allWebApps = new Collection<SPWebApplication>();
			SPWebService webSvc = SPFarm.Local.Services.GetValue<SPWebService>(string.Empty);
			foreach (SPWebApplication webApp in webSvc.WebApplications)
			{
				allWebApps.Add(webApp);
			}
			remarQSolution.Deploy(DateTime.Now, true, allWebApps, true);
		 
			Console.WriteLine("The deployment  has been submitted");
			bool solutionIsDeployed = false;
			while (!solutionIsDeployed)
			{
				System.Threading.Thread.Sleep(1000);
				solutionIsDeployed = !remarQSolution.JobExists && SolutionIsDeployed();
				Console.Write(".");
			}
			Console.WriteLine(".");
			Console.WriteLine("RemarQ has been deployed");
		}

		static void UninstallSolution()
		{
			Console.WriteLine("minimizing dependancies");
			SPSolution remarQSolution = SPFarm.Local.Solutions["balsamicsolutions.readunreadsitecolumn.wsp"];
			if (null != remarQSolution)
			{
				UninstallAllJobs();
				UninstallHttpModule();
				bool processCheck = remarQSolution.Deployed;
				if (processCheck)
				{
					remarQSolution.Retract(DateTime.Now, remarQSolution.DeployedWebApplications); 
					Console.WriteLine("Waiting on solution to retract");
					while (processCheck)
					{
						System.Threading.Thread.Sleep(1000);
						processCheck = SolutionIsDeployed() || remarQSolution.JobExists;
						Console.Write(".");
					}
				}
				Console.WriteLine(".");
				Console.WriteLine("Waiting on solution to delete");
				remarQSolution.Delete();
				processCheck = true;
				while (processCheck)
				{
					System.Threading.Thread.Sleep(1000);
					processCheck = SolutionExists();
					Console.Write(".");
				}
				System.Threading.Thread.Sleep(1000);
				Console.WriteLine("Soulution deleted");
			}
			else
			{
				Console.WriteLine("Solution does not exist");
			}
		}

		static void ReInstallSolution()
		{
			bool httpIsInstalled = ReadUnreadHttpModule.IsRegistered();
			UninstallSolution();
			InstallSolution();
			if (httpIsInstalled)
			{
				ReadUnreadHttpModule.RegisterModule(false);
			}
		}

		static void Upgrade()
		{
			//bool httpIsInstalled = ReadUnreadHttpModule.IsRegistered();
			Console.WriteLine("minimizing dependancies");
			UninstallAllJobs();
			SetUpgradeProperties();
			//UninstallHttpModule();
			UpdateSPTimerV4AssemblyRedirection(false);
			string filePath = Path.Combine(Environment.CurrentDirectory, "BalsamicSolutions.ReadUnreadSiteColumn.wsp");
			SPSolution remarQSolution = SPFarm.Local.Solutions["balsamicsolutions.readunreadsitecolumn.wsp"];
			remarQSolution.Upgrade(filePath);
			Console.WriteLine("The upgrade has been submitted");
			System.Threading.Thread.Sleep(2000);
			while (remarQSolution.JobExists)
			{
				remarQSolution = SPFarm.Local.Solutions["balsamicsolutions.readunreadsitecolumn.wsp"];
				System.Threading.Thread.Sleep(1000);
				Console.Write(".");
			}
			
			//if (httpIsInstalled)
			//{
			//	ReadUnreadHttpModule.RegisterModule();
			//}
			Console.WriteLine("The upgrade has been processed");
		}

		/// <summary>
		/// stop the SPTimerV4
		/// </summary>
		static void StopOwsTimer()
		{
			ServiceController owsControler = new ServiceController("SPTimerV4");
			Console.WriteLine("Stopping SPTimerV4");
			_ProgressTimer.Enabled = true;
			try
			{
				if (!owsControler.Status.Equals(ServiceControllerStatus.Stopped))
				{
					if (! owsControler.Status.Equals(ServiceControllerStatus.StopPending))
					{
						owsControler.Stop();
					}
					owsControler.WaitForStatus(ServiceControllerStatus.Stopped);
				}
			}
			finally
			{
				_ProgressTimer.Enabled = false;
			}
			Console.WriteLine("SPTimerV4 stopped");
		}

		static void PropagateOwsTimerConfig()
		{
			string configFile = SPUtility.GetVersionedGenericSetupPath("TEMPLATE", 15).ToLower().Replace("\\template", "\\bin") + "\\OWSTIMER.EXE.CONFIG";
			if (File.Exists(configFile))
			{
				HashSet<string> serverNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				foreach (var timerSvc in  SPFarm.Local.TimerService.Instances)
				{
					serverNames.Add(timerSvc.Server.Name);
					
				}
			}
		}

		#pragma warning disable 436
		/// <summary>
		/// add asssembly redirection for the current assembly
		/// kkudos to http://kwizcom.blogspot.ca/2011/02/assembly-redirection-in-owstimer.html
		/// </summary>
		static void UpdateSPTimerV4AssemblyRedirection(bool removeOnly)
		{
			//StopOwsTimer();
			Console.WriteLine("Updating assembly redirection");
			string configFile = SPUtility.GetVersionedGenericSetupPath("TEMPLATE", 15).ToLower().Replace("\\template", "\\bin") + "\\OWSTIMER.EXE.CONFIG";
			if (File.Exists(configFile))
			{
				//string XMLData = System.IO.File.ReadAllText(configFile, Encoding.UTF8);
				XmlDocument config = new XmlDocument();
				config.Load(configFile);
				//ensure assemblyBinding exists
				XmlNode assemblyBinding = config.SelectSingleNode("configuration/runtime/*[local-name()='assemblyBinding' and namespace-uri()='urn:schemas-microsoft-com:asm.v1']");

				if (assemblyBinding == null)
				{
					assemblyBinding = config.CreateNode(XmlNodeType.Element, "assemblyBinding", "urn:schemas-microsoft-com:asm.v1");
					config.SelectSingleNode("configuration/runtime").AppendChild(assemblyBinding);
				}
				//Delete old entrees if exist
				XmlElement current = assemblyBinding.FirstChild as XmlElement;
				while (current != null)
				{
					XmlElement elmToRemove = null;
					if (current.FirstChild != null)
					{
						var asmIdn = (current.FirstChild as XmlElement);
						if (asmIdn.GetAttribute("name").ToLower().Equals("balsamicsolutions.readunreadsitecolumn"))
						{
							elmToRemove = current;
						}
					}

					current = current.NextSibling as XmlElement;

					if (elmToRemove != null)
					{
						assemblyBinding.RemoveChild(elmToRemove);
					}
				}
				if (!removeOnly)
				{
					XmlElement dependentAssembly = null;
					if (dependentAssembly == null)//create it
					{
						dependentAssembly = config.CreateElement("dependentAssembly");
						dependentAssembly.InnerXml = "<assemblyIdentity name=\"BalsamicSolutions.ReadUnreadSiteColumn\" publicKeyToken=\"4000e3255b4ebc93\" culture=\"neutral\" />" +
													 "<bindingRedirect oldVersion=\""+BalsamicSolutions.ReadUnreadSiteColumn.Versions.REDIRECT+"\" newVersion=\"" + BalsamicSolutions.ReadUnreadSiteColumn.Versions.ASSEMBLY + "\" />";
						assemblyBinding.AppendChild(dependentAssembly);
					}
				}
				config.LoadXml(config.OuterXml.Replace("xmlns=\"\"", ""));
				config.Save(configFile);
				Console.WriteLine("Assembly version updated");

			}
			//StartOwsTimer();
		}
		#pragma warning restore 436
		/// <summary>
		/// start the SPTimerV4
		/// </summary>
		static void StartOwsTimer()
		{
			ServiceController owsControler = new ServiceController("SPTimerV4");
			Console.WriteLine("Starting SPTimerV4");
			_ProgressTimer.Enabled = true;
			try
			{
				if (owsControler.Status.Equals(ServiceControllerStatus.Stopped))
				{
					owsControler.Start();
					owsControler.WaitForStatus(ServiceControllerStatus.Running);
				}
			}
			finally
			{
				_ProgressTimer.Enabled = false;
			}
			
			Console.WriteLine("SPTimerV4 started");
		}
	}
}