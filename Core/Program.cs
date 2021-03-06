﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using DCS.Core.Configuration;
using DCS.Core.DataCollectors;
using DCS.Core.DataReaders;
using DCS.Core.Scheduler;
using log4net;
using log4net.Repository.Hierarchy;
using Quartz;

namespace DCS.Core
{
    public static class Program
    {
        static readonly ILog _logger = LogManager.GetLogger(typeof(Program));
        public static DataReadersManager DataReadersManager;
        public static DataWriter DataWriter;
        public static CronScheduler _scheduler = new CronScheduler();


        public static void RunScheduler()
        {


            Program.DataReadersManager = new DataReadersManager();
            Program.DataWriter = new DataWriter();


            // Here are added the configured data collectors
            foreach (DataCollectorSettings collectorSettings in Config.Settings.DataCollectorsSettings.Where(c=>c.LoadPlugin==1))
            {

                try
                {
                    if (!string.IsNullOrEmpty(collectorSettings.PluginFileName))
                    {

                        var applicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";
                        Assembly pluginAssembly = Assembly.LoadFrom(Path.Combine(applicationDirectory,"plugins",collectorSettings.PluginFileName));


                        foreach (var type in pluginAssembly.GetTypes())
                        {


                            if (type.GetInterface(typeof (IDataCollector).Name) != null)
                            {
                                var newCollector = Activator.CreateInstance(type) as IDataCollector;
                                var isValidDataCollector = true;

                                // performs an additional test, when a plugin class name is provided
                                if (collectorSettings.PluginClassName != null)
                                {
                                    isValidDataCollector = collectorSettings.PluginClassName == newCollector.GetType().Name;
                                }

                                if (newCollector != null && isValidDataCollector)
                                {
                                    newCollector.SetSettings(collectorSettings);
                                
                                    _logger.InfoFormat("Adding task of type: {0} to scheduler with frequency {1}. Task description: {2}",type.Name,collectorSettings.DataCollectionPeriod,collectorSettings.ReaderTaskDescription);
                                    _scheduler.AddTask(collectorSettings.ReaderTaskDescription, collectorSettings.DataCollectionPeriod, new Action(newCollector.CollectData));
                                    DataReadersManager.DataReaders.Add(newCollector);
                                }

                            }
                        }
                    
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

               


                _scheduler.Start();


            }



            _logger.InfoFormat("AF Elements structure refresh period is set to : {0}", Config.Settings.MainSettings.CronPeriod_ConfigurationRefresh);
            _logger.InfoFormat("Data write period is set to : {0}", Config.Settings.MainSettings.CronPeriod_DataWrite);
            _logger.InfoFormat("Web Application port is set to : {0}", Config.Settings.MainSettings.WebConfigPort);


            _logger.InfoFormat("Readers Initialization");
            Program.DataReadersManager.InitializeReaders();

            //_logger.InfoFormat("configuring scheduler to run tasks periodically");
            _scheduler.AddTask("Refreshing configuration", Config.Settings.MainSettings.CronPeriod_ConfigurationRefresh, new Action(DataReadersManager.RefreshReadersConfiguration));
            _scheduler.AddTask("Writing data to the PI System", Config.Settings.MainSettings.CronPeriod_DataWrite, new Action(DataWriter.FlushData));
            _scheduler.Start();
        }

        public static void StopScheduler()
        {
            Program.DataReadersManager?.Dispose();
            _scheduler?.Stop();
        }
    }

}
