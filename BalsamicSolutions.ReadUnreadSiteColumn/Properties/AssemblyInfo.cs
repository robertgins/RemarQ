using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Resources;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("RemarQ ")]
[assembly: AssemblyDescription("Read Unread indicators for SharePoint 2013")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Balsamic Solutions (http://www.balsamicsolutions.com)")]
[assembly: AssemblyProduct("RemarQ")]
[assembly: AssemblyCopyright("Copyright © Balsamic Solutions 2014")]
[assembly: AssemblyTrademark("RemarQ")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(false)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("8151c497-ea34-481a-a5f9-46567b4e3ca2")]

//we cant auto increment assembly versions without fubaring  most of 
//sharepoint so the assembly version for compatbility is always going
//to be 1.1.2.0
[assembly: AssemblyVersion(BalsamicSolutions.ReadUnreadSiteColumn.Versions.ASSEMBLY )]
[assembly: AssemblyFileVersion(BalsamicSolutions.ReadUnreadSiteColumn.Versions.FILE)]

//if we need to make a change we will do it here, and we can
//look up that change in the about dialog
[assembly: AssemblyInformationalVersion("2.0.0.5")]

[assembly: NeutralResourcesLanguageAttribute("en")]

//this is the debugger console app used during development
[assembly:InternalsVisibleTo("DebugConsole,PublicKey=0024000004800000940000000602000000240000525341310004000001000100071C7DD1C4C3F0A82BF6301500F9CEE73865A45577B55B6071323F9D8CC78A05234CB1B22E631B6654AB56EEBEE6BBE289F1DFB275EB59D9119AB9D34E09D853A0A059A75F13BC7219BDF7A1607CCC19E15D6F19F06FFAC727EA798C66E25EDDF0F4A895EACAA0308A207180D5956CC6B594B73A126301EDAE48EC652252D3A6")]
[assembly:InternalsVisibleTo("Utilities,PublicKey=0024000004800000940000000602000000240000525341310004000001000100071C7DD1C4C3F0A82BF6301500F9CEE73865A45577B55B6071323F9D8CC78A05234CB1B22E631B6654AB56EEBEE6BBE289F1DFB275EB59D9119AB9D34E09D853A0A059A75F13BC7219BDF7A1607CCC19E15D6F19F06FFAC727EA798C66E25EDDF0F4A895EACAA0308A207180D5956CC6B594B73A126301EDAE48EC652252D3A6")]
[assembly:InternalsVisibleTo("RemarQListProcessor,PublicKey=0024000004800000940000000602000000240000525341310004000001000100071C7DD1C4C3F0A82BF6301500F9CEE73865A45577B55B6071323F9D8CC78A05234CB1B22E631B6654AB56EEBEE6BBE289F1DFB275EB59D9119AB9D34E09D853A0A059A75F13BC7219BDF7A1607CCC19E15D6F19F06FFAC727EA798C66E25EDDF0F4A895EACAA0308A207180D5956CC6B594B73A126301EDAE48EC652252D3A6")]
