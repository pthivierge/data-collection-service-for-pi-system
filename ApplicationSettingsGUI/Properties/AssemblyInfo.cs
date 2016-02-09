using System.Reflection;
using System.Runtime.InteropServices;
using log4net.Config;

[assembly: XmlConfigurator(ConfigFile = "FDSUI.log4net.cfg.xml", Watch = true)]
//Company shipping the assembly

[assembly: AssemblyCompany("OSIsoft")]

//Friendly name for the assembly

[assembly: AssemblyTitle("FitBit Data Service Prototype Service Manager User Interface")]

//Short description of the assembly

[assembly:
    AssemblyDescription(
        "FitBit Data Service Prototype Service Manager.  This is a user interface help starting or stopping the Service and manage service settings."
        )]
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
