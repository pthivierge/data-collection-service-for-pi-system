using System;
using System.Configuration.Install;
using System.Net.Mime;
using System.Reflection;
using System.ServiceProcess;
using CommandLine;
using DCS.Core.WebConfig;
using log4net;
using DCS.Core;
using DCS.Core.Configuration;

namespace DCS.Service
{
    internal static class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof (Program));

        /// <summary>
        ///     Service Main Entry Point
        /// </summary>
        private static void Main(string[] args)
        {
           
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            if (Environment.UserInteractive)
            {
                _logger.Info("Starting service interractively");
                
                var options = new CommandLineOptions();

                if (Parser.Default.ParseArguments(args, options))
                {
                    if (options.Run)
                    {
                        if(!Config.IsLoaded())
                            Environment.Exit(-1);

                        WebHost.Instance.Start();
                        Core.Program.RunScheduler();

                        Console.WriteLine("press a key to stop the data collection");
                        Console.ReadKey();


                        Core.Program.StopScheduler();

                        WebHost.Instance.Dispose();
                        Console.WriteLine("Stopped");

                    }

                    if (options.Install)
                    {
                        ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                    }

                    if (options.Uninstall)
                    {
                        ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                    }
                    
                    // exit ok
                    Environment.Exit(0);
                    
                }

                
            }
            else
            {
                ServiceBase[] ServicesToRun =
                {
                    new Service()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Error(e.ExceptionObject);
            
            
        }
    }
}
