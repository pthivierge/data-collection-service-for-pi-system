using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using log4net;
using WSR.Settings;
using Quartz;
using WSR.Core;
using WSR.Core.DataReaders;

namespace WSR.Service
{
    public partial class Service : ServiceBase
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(Service));

      
        private IScheduler _scheduler;


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


            
            _scheduler = SharedData.SchedulerFactory.GetScheduler();
            

            ServiceTasksInstances.ConfigurationManager = new ConfigurationManager();
            ServiceTasksInstances.DataReadersManager=new DataReadersManager();
            ServiceTasksInstances.DataWriter = new DataWriter();
            
            _logger.InfoFormat("AF Elements structure refresh period is set to : {0}",Settings.General.Default.cronPeriod_Refresh);
            _logger.InfoFormat("FitBit date refresh period is set to : {0}", Settings.General.Default.cronPeriod_Update);
            _logger.InfoFormat("Data write period is set to : {0}", Settings.General.Default.cronPeriod_Write);
            
            _logger.InfoFormat("Executing first time data refresh");
            ServiceTasksInstances.ConfigurationManager.RunOnce();
            ServiceTasksInstances.DataReadersManager.RunOnce();
            ServiceTasksInstances.DataWriter.RunOnce();

            _logger.InfoFormat("configuring scheduler to run tasks periodically");
            AddJobToScheduler<ConfigurationJob>(_scheduler,Settings.General.Default.cronPeriod_Refresh,"configuration");
            AddJobToScheduler<DataReadersManagerJob>(_scheduler, Settings.General.Default.cronPeriod_Update, "DataReader");
            AddJobToScheduler<DataWriterJob>(_scheduler, Settings.General.Default.cronPeriod_Write, "DataWriter");

            _scheduler.Start();

        }

        // Settings.General.Default.cronPeriod_Refresh
        private void AddJobToScheduler<T>(IScheduler scheduler, string jobCronSchedule, string jobName) where T:IJob
        {
           
            IJobDetail cmJob = JobBuilder.Create<T>().WithIdentity(jobName).Build();
            ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(jobCronSchedule).Build();

            scheduler.ScheduleJob(cmJob, trigger);
        }

        protected override void OnStop()
        {
            // your code here
            _scheduler.Shutdown(true);

            ServiceTasksInstances.DataReadersManager.Dispose();
            
            base.OnStop();
            _logger.Info("Service Stopped.");
        }





        #endregion
    }
}
