using log4net;
using Quartz;
using WSR.Core.DataReaders.FakeRandomDataReader;

namespace WSR.Core
{
    public static class QuartzJobs
    {
        static readonly ILog _logger = LogManager.GetLogger(typeof(QuartzJobs));
        public static DataReadersManager DataReadersManager;
        public static DataWriter DataWriter;
        private static IScheduler _scheduler;

        // Settings.General.Default.cronPeriod_Refresh
        public static void AddJobToScheduler<T>(IScheduler scheduler, string jobCronSchedule, string jobName) where T : IJob
        {

            IJobDetail cmJob = JobBuilder.Create<T>().WithIdentity(jobName).Build();
            ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(jobCronSchedule).Build();

            scheduler.ScheduleJob(cmJob, trigger);
        }

        public static void RunScheduler()
        {
            _scheduler = SharedData.SchedulerFactory.GetScheduler();

            QuartzJobs.DataReadersManager = new DataReadersManager();
            QuartzJobs.DataWriter = new DataWriter();

            DataReadersManager.DataReaders.Add(new FakeRandomDataReader("megatron", "WebServiceDataReader", "RandomDataReader"));

            _logger.InfoFormat("AF Elements structure refresh period is set to : {0}", Settings.General.Default.cronPeriod_Refresh);
            _logger.InfoFormat("Data write period is set to : {0}", Settings.General.Default.cronPeriod_Write);

            _logger.InfoFormat("Readers Initialization");
            QuartzJobs.DataReadersManager.InitializeReaders();

            _logger.InfoFormat("configuring scheduler to run tasks periodically");
            QuartzJobs.AddJobToScheduler<ConfigurationJob>(_scheduler, Settings.General.Default.cronPeriod_Refresh, "configuration");
            QuartzJobs.AddJobToScheduler<DataReadersManagerJob>(_scheduler, Settings.General.Default.cronPeriod_CollectData, "DataReader");
            QuartzJobs.AddJobToScheduler<DataWriterJob>(_scheduler, Settings.General.Default.cronPeriod_Write, "DataWriter");

            _scheduler.Start();
        }

        public static void StopScheduler()
        {
            QuartzJobs.DataReadersManager.Dispose();
            _scheduler.Shutdown(true);
        }
    }

    public class ConfigurationJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            QuartzJobs.DataReadersManager.RefreshReadersConfiguration();
        }
    }

    public class DataWriterJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            QuartzJobs.DataWriter.RunOnce();
        }
    }

    public class DataReadersManagerJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            QuartzJobs.DataReadersManager.CollectReadersData();
        }
    }
}
