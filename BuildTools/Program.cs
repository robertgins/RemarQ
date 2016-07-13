// -----------------------------------------------------------------------------
// Copyright 7/13/2016 (c) Balsamic Software, LLC. All rights reserved.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------- 
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BuildTools
{

	/// <summary>
	/// pre and post build support for the setup project,  This project should be set to build first 
	/// so that it will create the WSP before the setup is compiled. it can also
	/// auto increment the AssemblyInformationalVersion of the project
	/// which will provide you a reference to the build but will not
	/// blow up SharePoint with lots of version conflicts. To do that
	/// add this to the post build event of this project
	/// $(TargetPath) "$(SolutionDir)BalsamicSolutions.ReadUnreadSiteColumn"
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
 				string filePath = args[0].Trim('"');
				if(!filePath.EndsWith("\\")) filePath += "\\";
				 filePath += "Properties\\AssemblyInfo.cs";
				if(!File.Exists(filePath)) throw new FileNotFoundException(filePath);
				string[] fileLines = File.ReadAllLines(filePath);
				for(int idx =0; idx<fileLines.Length; idx++)
				{
					string fileLine = fileLines[idx];
					if(fileLine.IndexOf("AssemblyInformationalVersion") > -1)
					{
						int posIndexEnd = fileLine.LastIndexOf('.');
						int posQuoteEnd = fileLine.LastIndexOf('"');
						posIndexEnd++;
						string numVal = fileLine.Substring(posIndexEnd ,posQuoteEnd -posIndexEnd );
						int versionNum = int.Parse(numVal);
						versionNum ++;
						 
						numVal = versionNum.ToString();
						string filePart1 = fileLine.Substring(0,posIndexEnd);
						string filepart2 = fileLine.Substring(posQuoteEnd);
						string newLine = filePart1 + numVal + filepart2;
						fileLines[idx] = newLine;
						File.WriteAllLines(filePath,fileLines);
						break;
					}
				}
		}
	}
}
