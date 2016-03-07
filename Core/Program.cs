using System;
using DCS.Core.Configuration;
using DCS.Core.DataReaders.FakeRandomDataReader;
using DCS.Core.Scheduler;
using log4net;
using Quartz;

namespace DCS.Core
{
    public static class Program
    {
        static readonly ILog _logger = LogManager.GetLogger(typeof(Program));
        public static DataReadersManager DataReadersManager;
        public static DataWriter DataWriter;
        public static CronScheduler _scheduler=new CronScheduler();


        public static void RunScheduler()
        {
        

            Program.DataReadersManager = new DataReadersManager();
            Program.DataWriter = new DataWriter();


            // Here are added the configured data readers
            foreach (ReadersConfiguration reader in Config.Settings.ReadersConfiguration)
            {
                if (!string.IsNullOrEmpty(reader.ReaderType))
                {
                    switch (reader.ReaderType)
                    {
                        case "RandomReader":
                        {
                            var newReader = new FakeRandomBaseDataReader(reader.AFServerName, reader.AFDatabaseName,reader.AFElementTemplateName);
                            _scheduler.AddTask(reader.ReaderTaskDescription, reader.DataCollectionPeriod, new Action(newReader.CollectData));
                            DataReadersManager.DataReaders.Add(newReader);
                            break;
                        }
                    }
                }


                _scheduler.Start();


            }

            

            _logger.InfoFormat("AF Elements structure refresh period is set to : {0}", Config.Settings.MainSettings.CronPeriod_ConfigurationRefresh);
            _logger.InfoFormat("Data write period is set to : {0}", Config.Settings.MainSettings.CronPeriod_DataWrite);
            _logger.InfoFormat("Web Application port is set to : {0}", Config.Settings.MainSettings.WebConfigPort);


            _logger.InfoFormat("Readers Initialization");
            Program.DataReadersManager.InitializeReaders();

            //_logger.InfoFormat("configuring scheduler to run tasks periodically");
            _scheduler.AddTask("Refreshing configuration", Config.Settings.MainSettings.CronPeriod_ConfigurationRefresh,new Action(DataReadersManager.RefreshReadersConfiguration));
            _scheduler.AddTask("Writing data to the PI System", Config.Settings.MainSettings.CronPeriod_DataWrite, new Action(DataWriter.Run));
            _scheduler.Start();
        }

        public static void StopScheduler()
        {
            Program.DataReadersManager.Dispose();
            _scheduler.Stop();
        }
    }
    
}
