using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using log4net;
using WSR.Core;
using WSR.Core.Configuration;


namespace WSR.Service
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
                if (Config.Settings.DebugSettings.StartDebuggerOnStart)
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
                _logger.Info("Service is Started.");

            }
            catch (Exception exception)
            {
                _logger.Fatal(exception);
            }
        }

        private void InitService()
        {
            Core.Program.RunScheduler();
        }

       

        protected override void OnStop()
        {

            Core.Program.StopScheduler();
            
            base.OnStop();
            _logger.Info("Service Stopped.");
        }





        #endregion
    }
}
