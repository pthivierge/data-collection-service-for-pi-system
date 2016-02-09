using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using FDS.Core.Scheduler;
using log4net;
using FDS.Settings;

namespace FDS.Service
{
    public partial class Service : ServiceBase
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (Service));

        /// Scheduler that handles exact time scheduling
        private readonly CronScheduler _scheduler = new CronScheduler();

        #region Constructors

        /// <summary>
        ///     constructeur du service
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
                if (Advanced.Default.StartDebuggerOnStart)
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

                _logger.Info("Service is Started.");
                InitService();
            }
            catch (Exception exception)
            {
                _logger.Fatal(exception);
            }
        }

        private void InitService()
        {
            
        }

        protected override void OnStop()
        {
            // your code here

            base.OnStop();
            _logger.Info("Service Stopped.");
        }





        #endregion
    }
}
