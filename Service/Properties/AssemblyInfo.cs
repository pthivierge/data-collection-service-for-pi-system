using System.Reflection;
using System.Runtime.InteropServices;
using log4net.Config;

[assembly: XmlConfigurator(ConfigFile = "WSRService.log4Net.cfg.xml", Watch = true)]

//Company shipping the assembly

[assembly: AssemblyCompany("Patrice Thivierge F.")]

//Friendly name for the assembly

[assembly: AssemblyTitle("Web Service Data Reader Service")]

//Short description of the assembly

[assembly: AssemblyDescription("Web Service Data Reader Windows Service")]
[assembly: AssemblyConfiguration("")]

//Product Name

[assembly: AssemblyProduct("Web Service Data Reader")]

//Copyright information

[assembly: AssemblyCopyright("Copyright Patrice Thivierge F. © 2016")]

//Enumeration indicating the target culture for the assembly

[assembly: AssemblyCulture("")]

//

[assembly: ComVisible(false)]



//Version number expressed as a string

[assembly: AssemblyVersion("1.0.*")]
//[assembly: AssemblyFileVersion("1.0.0.0")]
