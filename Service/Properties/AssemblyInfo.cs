using System.Reflection;
using System.Runtime.InteropServices;
using log4net.Config;

[assembly: XmlConfigurator(ConfigFile = "FDSService.log4Net.cfg.xml", Watch = true)]

//Company shipping the assembly

[assembly: AssemblyCompany("OSIsoft")]

//Friendly name for the assembly

[assembly: AssemblyTitle("FitBit Data Service Prototype Service")]

//Short description of the assembly

[assembly: AssemblyDescription("FitBit Data Service Prototype Windows Service")]
[assembly: AssemblyConfiguration("")]

//Product Name

[assembly: AssemblyProduct("FitBit Data Service Prototype")]

//Copyright information

[assembly: AssemblyCopyright("Copyright OSIsoft © 2016")]

//Enumeration indicating the target culture for the assembly

[assembly: AssemblyCulture("")]

//

[assembly: ComVisible(false)]



//Version number expressed as a string

[assembly: AssemblyVersion("1.0.*")]
//[assembly: AssemblyFileVersion("1.0.0.0")]
