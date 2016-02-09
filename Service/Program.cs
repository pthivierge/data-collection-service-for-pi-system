using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;
using log4net;

namespace FDS.Service
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
                string parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":
                        ManagedInstallerClass.InstallHelper(new[] {Assembly.GetExecutingAssembly().Location});
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new[] {"/u", Assembly.GetExecutingAssembly().Location});
                        break;
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
