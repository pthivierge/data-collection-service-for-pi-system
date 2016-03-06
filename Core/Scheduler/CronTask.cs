using System;
using log4net;
using Quartz;

namespace WSR.Core.Scheduler
{
    public sealed class CronTask : IJob
    {
        private readonly string _cronConfig;
        private readonly ILog _logger = LogManager.GetLogger(typeof(CronTask));
        private readonly Action _task;
        private readonly string _taskName;

        public CronTask()
        {
        }

        public CronTask(string taskName, string cronConfig, Action action)
        {
            _taskName = taskName;
            _task = action;
            _cronConfig = cronConfig;
        }

        public string TaskName
        {
            get { return _taskName; }
        }

        public string CronConfig
        {
            get { return _cronConfig; }
        }

        public void Execute(IJobExecutionContext context)
        {
            var task = (CronTask)context.MergedJobDataMap.Get("task");

            // executes the task 
            _logger.Info("Executing task : " + task.TaskName);
            task._task();
        }
    }
}