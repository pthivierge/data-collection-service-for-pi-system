using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using DCS.Core.Configuration;
using log4net;
using DCS.Core;
using DCS.Core.WebConfig;


namespace DCS.Service
{
    public partial class Service : ServiceBase
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(Service));

        #region Constructors

        /// <summary>
        ///     Service Constructor
        ///     Fist byte of code called when service is started
        /// </summary>
        public Service()
        {
            try
            {
                _logger.Info("Service is Initializing...");
                InitializeComponent();

                // do not put your code here... put it in on start, otherwise if the service crashes you wont see anything
            }


            catch (Exception e)
            {
                _logger.Fatal(e);
                throw;
            }
        }

        #endregion

        #region Readonly & Static Fields

        #endregion

        #region Instance Methods

        protected override void OnShutdown()
        {
            // your code here

            base.OnShutdown();
            _logger.Info("Service is shutting down.");
        }

        protected override void OnStart(string[] args)
        {
            try
            {

                //if (true)
                if (Config.Settings != null && Config.Settings.DebugSettings.StartDebuggerOnStart)
                {
                    Debugger.Launch();

                    while (!Debugger.IsAttached)
                    {
                        // Waiting until debugger is attached
                        RequestAdditionalTime(1000); // Prevents the service from timeout
                        Thread.Sleep(1000); // Gives you time to attach the debugger   
                    }
                    RequestAdditionalTime(20000); // for Debugging the OnStart method,
                    // increase as needed to prevent timeouts
                }

                base.OnStart(args);

                // put your startup service code here:
                _logger.Info("Initializing service.");
                InitService();


            }
            catch (Exception exception)
            {
                _logger.Fatal(exception);
            }
        }

        private void InitService()
        {
            if (!Config.IsLoaded())
            {
                ExitCode = -1;
                Stop();
                throw new ApplicationException("Settings file could not be found.");
            }

            WebHost.Instance.Start();
            Core.Program.RunScheduler();

        }



        protected override void OnStop()
        {
            try
            {

                WebHost.Instance.Dispose();
                Core.Program.StopScheduler();

                base.OnStop();
                _logger.Info("Service Stopped.");

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }





        #endregion
    }
}
