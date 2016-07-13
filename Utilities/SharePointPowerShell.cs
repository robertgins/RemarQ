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
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
	public class SharePointPowerShell : IDisposable
	{
		PowerShell _PowerShell = null;
		Runspace _Runspace = null;

		public SharePointPowerShell()
		{
			RunspaceConfiguration rsConfig = RunspaceConfiguration.Create();
			PSSnapInException snapInError = null;
			PSSnapInInfo sharePointSnapIn = rsConfig.AddPSSnapIn("Microsoft.SharePoint.PowerShell", out snapInError);
			System.Diagnostics.Debug.WriteLine(sharePointSnapIn.AssemblyName);
			this._Runspace = RunspaceFactory.CreateRunspace(rsConfig);
			this._PowerShell = PowerShell.Create();
			this._PowerShell.Runspace = this._Runspace;
			this._Runspace.Open();
		}

		/// <summary>
		/// Validate that the connection is up and heathy
		/// </summary>
		public bool IsOpen
		{
			get { return (this._Runspace != null && this._Runspace.RunspaceStateInfo.State == RunspaceState.Opened); }
		}

		public bool AddSolution(string wspFilePath)
		{
			bool returnValue = false;
		 
			if (!this.IsOpen)
			{
				throw new InvalidOperationException("Runspace is not open");
			}
			Command psCmd = new Command("Add-SPSolution");
			psCmd.Parameters.Add("LiteralPath", wspFilePath);

			string cmdText = CommandText(psCmd);
			System.Diagnostics.Trace.Write(cmdText);
			using (Pipeline pipeLine = this._Runspace.CreatePipeline())
			{
				pipeLine.Commands.Add(psCmd);
				Collection<PSObject> cmdRes = pipeLine.Invoke();
				if (cmdRes.Count == 1)
				{
					if (cmdRes[0].TypeNames.Contains("Microsoft.SharePoint.Administration.SPSolution"))
					{
						returnValue = true;
					}
				}
			}
			return returnValue;
		}

		public void DeploySolution(string solutionName, bool allowGac)
		{
			this.InstallSolution(solutionName, allowGac);
		}

		/// <summary>
		/// Install solution 
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="allowGac"></param>
		/// <returns></returns>
		public void InstallSolution(string solutionName, bool allowGac)
		{
			//Install-SPSolution -Identity contoso_solution.wsp -GACDeployment
			if (!this.IsOpen)
			{
				throw new InvalidOperationException("Runspace is not open");
			}
			Command psCmd = new Command("Install-SPSolution");
			psCmd.Parameters.Add("Identity", solutionName);
			psCmd.Parameters.Add("AllWebApplications");
			psCmd.Parameters.Add("Force");
			if (allowGac)
			{
				psCmd.Parameters.Add("GACDeployment");
			}

			string cmdText = CommandText(psCmd);
			System.Diagnostics.Trace.Write(cmdText);
			using (Pipeline pipeLine = this._Runspace.CreatePipeline())
			{
				pipeLine.Commands.Add(psCmd);
				Collection<PSObject> cmdRes = pipeLine.Invoke();
			}
		}

		public void UpdateSolution(string wspFilePath, string solutionName, bool allowGac)
		{
			if (!this.IsOpen)
			{
				throw new InvalidOperationException("Runspace is not open");
			}
 
			Command psCmd = new Command("Update-SPSolution");
			psCmd.Parameters.Add("Identity", solutionName);
			psCmd.Parameters.Add("LiteralPath", wspFilePath);
			psCmd.Parameters.Add("Force");
			if (allowGac)
			{
				psCmd.Parameters.Add("GACDeployment");
			}
			string cmdText = CommandText(psCmd);
			System.Diagnostics.Trace.Write(cmdText);
			using (Pipeline pipeLine = this._Runspace.CreatePipeline())
			{
				pipeLine.Commands.Add(psCmd);
				Collection<PSObject> cmdRes = pipeLine.Invoke();
			}
		}
	 
		public void RetractSolution(string solutionName)
		{
			this.UninstallSolution(solutionName);
		}

		public void UninstallSolution(string solutionName)
		{
			if (!this.IsOpen)
			{
				throw new InvalidOperationException("Runspace is not open");
			}
			Command psCmd = new Command("Uninstall-SPSolution");
			psCmd.Parameters.Add("Identity", solutionName);
			psCmd.Parameters.Add("AllWebApplications");
			string cmdText = CommandText(psCmd);
			System.Diagnostics.Trace.Write(cmdText);
			using (Pipeline pipeLine = this._Runspace.CreatePipeline())
			{
				pipeLine.Commands.Add(psCmd);
				Collection<PSObject> cmdRes = pipeLine.Invoke();
			}
		}

		public void DeleteSolution(string solutionName)
		{
			if (!this.IsOpen)
			{
				throw new InvalidOperationException("Runspace is not open");
			}
			Command psCmd = new Command("Remove-SPSolution");
			psCmd.Parameters.Add("Identity", solutionName);
			psCmd.Parameters.Add("Force");
			string cmdText = CommandText(psCmd);
			System.Diagnostics.Trace.Write(cmdText);
			using (Pipeline pipeLine = this._Runspace.CreatePipeline())
			{
				pipeLine.Commands.Add(psCmd);
				Collection<PSObject> cmdRes = pipeLine.Invoke();
			}
		}

		#region Diagnostic tools
		
		static string CommandText(Command psCmd)
		{
			var returnValue = new System.Text.StringBuilder();
			returnValue.Append(psCmd.CommandText);
			returnValue.Append(" ");
			foreach (var cmdParameter in psCmd.Parameters)
			{
				string cmdCmd = cmdParameter.Name;
				string cmdTxt = cmdParameter.Value == null ? "" : cmdParameter.Value.ToString();
				if (cmdCmd.Equals("confirm", StringComparison.OrdinalIgnoreCase))
				{
					returnValue.Append("-Confirm:$" + cmdTxt + " ");
				}
				else
				{
					if (string.IsNullOrWhiteSpace(cmdTxt))
					{
						cmdTxt = cmdTxt = "''";
					}
					else
					{
						if (cmdTxt.IndexOf(" ") > -1)
						{
							cmdTxt = "'" + cmdTxt + "'";
						}
					}
					returnValue.AppendFormat("-{0} {1} ", cmdCmd, cmdTxt);
				}
			}
			return returnValue.ToString();
		}
		
		static string DumpPSObjectCollection(Collection<PSObject> coll)
		{
			if (coll.Count == 0)
			{
				return "No results found\r\n";
			}
			var returnValue = new System.Text.StringBuilder();
			foreach (var psObj in coll)
			{
				returnValue.Append(DumpPSObject(psObj));
			}
			return returnValue.ToString();
		}
		
		static void DumpPSObject(PSObject psObj, System.Text.StringBuilder stringBuilder, string indentText)
		{
			stringBuilder.AppendFormat("{0}.NET Type {1}\r\n", indentText, psObj.GetType());
			stringBuilder.AppendFormat("{0}NamedType:\r\n", indentText);
			
			foreach (var name in psObj.TypeNames)
			{
				stringBuilder.AppendFormat("{0}\t{1}\r\n", indentText, name);
			}
			
			stringBuilder.AppendFormat("{0}Properties:\r\n", indentText);
			
			foreach (var prop in psObj.Properties.OrderBy((prop) => prop.Name))
			{
				stringBuilder.AppendFormat("{0}{1}: {2} ({3})({4})\r\n",
					indentText,
					prop.Name,
					prop.Value ?? "<null>",
					prop.Value == null ? "<null>" : prop.Value.GetType().ToString(),
					prop.TypeNameOfValue);
				
				if ((prop.Value != null) && (prop.Value.GetType() == typeof(PSObject)))
				{
					DumpPSObject((PSObject)prop.Value, stringBuilder, indentText + "\t");
				}
			}
		}
		
		static string DumpPSObject(PSObject psObj)
		{
			var returnValue = new System.Text.StringBuilder(2000);
			DumpPSObject(psObj, returnValue, string.Empty);
			return returnValue.ToString();
		}
		
		#endregion
		
		#region IDisposable
		
		public virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (null != this._PowerShell)
				{
					try
					{
						this._PowerShell.Dispose();
					}
					catch (ObjectDisposedException)
					{
					}
					this._PowerShell = null;
				}
				if (null != this._Runspace)
				{
					try
					{
						this._Runspace.Dispose();
					}
					catch (ObjectDisposedException)
					{
					}
					this._Runspace = null;
				}
			}
		}
		
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		#endregion
	}
}