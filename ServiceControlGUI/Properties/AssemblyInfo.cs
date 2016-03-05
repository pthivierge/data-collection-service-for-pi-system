using System;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net.Config;

[assembly: XmlConfigurator(ConfigFile = "WSRServiceConfigurator.log4net.cfg.xml", Watch = true)]
//Company shipping the assembly

[assembly: AssemblyCompany("Patrice Thivierge F.")]

//Friendly name for the assembly

[assembly: AssemblyTitle("Web Service Data Reader Service Manager User Interface")]

//Short description of the assembly

[assembly:
    AssemblyDescription("Web Service Data Reader Service Manager.  This is a user interface help starting or stopping the Service and manage service settings. Licensed under Apache License Version 2.0")]
[assembly: AssemblyConfiguration("")]

//Product Name

[assembly: AssemblyProduct("Web Service Data Reader")]

//Copyright information

[assembly: AssemblyCopyright("Patrice Thivierge Fortin © 2016")]


//Enumeration indicating the target culture for the assembly

[assembly: AssemblyCulture("")]

//

[assembly: ComVisible(false)]

//Version number expressed as a string

[assembly: AssemblyVersion("1.0.*")]
//[assembly: AssemblyFileVersion("1.0.0.0")]
