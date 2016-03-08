using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using CommandLine;
using log4net;
using WSR.Core;
using WSR.Core.WebConfig;

namespace WSR.Service
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
