using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WSR.Core.Helpers;
using log4net;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using Quartz;

namespace WSR.Core
{
    /// <summary>
    /// This class exposes a ConcurrentQueue to make sure information can be gathered smotthly form other threads.
    /// This class is dedicated to write the data collected by data readers into the PI System
    /// 
    /// </summary> 
    public class DataWriter
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(DataWriter));
        public static readonly ConcurrentQueue<List<AFValue>> DataQueue = new ConcurrentQueue<List<AFValue>>();
        

        public void RunOnce()
        {
            WriteData();
        }



        private void WriteData()
        {

            List<AFValue> values;
            var allValues = new List<AFValue>();


            // gets all currently available values from the queue
            while (SharedData.DataWriterQueue.TryDequeue(out values))
            {
                allValues.AddRange(values);
            }

            // writes data only if there is data
            if (allValues.Count != 0)
            {

                _logger.InfoFormat("Sorting and writing {0} values.", allValues.Count);
                // writing into PI : we sort all the values per timestamp, that will make life easier for the PI Server
                allValues.Sort();
                AFListData.UpdateValues(allValues, AFUpdateOption.Replace, AFBufferOption.BufferIfPossible);

                // clear the values
                allValues.Clear();

                _logger.InfoFormat("Write completed. Waiting for new data.");
            }

        }



    }


}
