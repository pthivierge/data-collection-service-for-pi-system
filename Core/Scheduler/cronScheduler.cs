﻿using System;
using log4net;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;

namespace DCS.Core.Scheduler
{
    /// <summary>
    /// Scheduler class
    /// for cron config
    /// <see cref="http://www.quartz-scheduler.org/documentation/quartz-1.x/tutorials/crontrigger"/>
    /// </summary>
    public class CronScheduler
    {
        #region Readonly & Static Fields

        private static readonly ILog Logger = LogManager.GetLogger(typeof(CronTask));
        private readonly ISchedulerFactory schedFact = new StdSchedulerFactory();

        #endregion

        #region Fields

        private IScheduler _sched;

        #endregion

        #region Instance Methods

        public void AddTask(string taskName, string cronConfig, Action action)
        {
            // get a scheduler
            _sched = schedFact.GetScheduler();

            // construct job info
            var task = new CronTask(taskName, cronConfig, action);


            var jobDetail = new JobDetailImpl(task.TaskName, typeof(CronTask));
            jobDetail.JobDataMap.Put("task", task);

            // fire every hour
            var trigger = new CronTriggerImpl(task.TaskName, "Group1", task.CronConfig);

            _sched.ScheduleJob(jobDetail, trigger);
        }

        public bool IsStarted()
        {
            if (_sched != null)
                return (_sched.IsStarted);
            return false;
        }


        public void Start()
        {
            _sched.Start();
        }

        public void Stop()
        {
            Logger.Debug("Stopping Cron Scheduler...");
            _sched?.Shutdown();
        }

        #endregion

        // _logger pour écrire les logs

        // construct a scheduler factory
    }
}