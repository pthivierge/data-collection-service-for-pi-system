using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIsoft.AF.Asset;
using Quartz;
using Quartz.Impl;

namespace DCS.Core
{
    public static class SharedData
    {
        
        public static readonly ConcurrentQueue<List<AFValue>> DataWriterQueue = new ConcurrentQueue<List<AFValue>>();

        public static readonly ISchedulerFactory SchedulerFactory = new StdSchedulerFactory();

    }
}
