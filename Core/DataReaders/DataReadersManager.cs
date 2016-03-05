using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using OSIsoft.AF.Asset;
using Quartz;

namespace WSR.Core.DataReaders
{
    public class DataReadersManager : IDisposable
    {
        ILog _logger = LogManager.GetLogger(typeof(DataReadersManager));

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private BlockingCollection<List<AFElement>> _processQueue = new BlockingCollection<List<AFElement>>();
        private Timer _readTimer;
        private Task _readTask;

        

        public DataReadersManager()
        {

            _logger.InfoFormat("Data Reader Created, starting internal read data task");
            _readTask = Task.Factory.StartNew(ReadData);
        }


        public void RunOnce()
        {
            AddDevicesToQueue(null);
        }



        private void AddDevicesToQueue(object obj)
        {
            foreach (List<AFElement> afElements in SharedData.FitBitDevices.ToArray())
            {
                _processQueue.Add(afElements);
            }
        }


        private void ReadData()
        {

            foreach (List<AFElement> devices in _processQueue.GetConsumingEnumerable(_cancellationToken.Token))
            {

                _logger.InfoFormat("Gathering data for {0} FitBit Devices", SharedData.FitBitDevices.Count);

                if (_cancellationToken.IsCancellationRequested) { return; }

                // will execute data queries - maximum 10 at the time
                Parallel.ForEach(devices,
                new ParallelOptions { MaxDegreeOfParallelism = 10, CancellationToken = _cancellationToken.Token }
                , (device) =>
                {
                    var reader = new FitBitDataReader(device);
                    reader.Read();
                });

                _logger.Info("Completed reading data for the devices.");

            }

            _logger.Info("Read Data task has terminated.");
        }


        public void Dispose()
        {
            // stops the timer and wait for the reading thread to complete
            _logger.Info("Stopping Readers, waiting for remaining operations to complete...");
            _processQueue.CompleteAdding();

            var tasks = new List<Task>();
            tasks.Add(_readTask);
            Task.WaitAll(tasks.ToArray());

            _logger.Info("Readers stopped");
        }
    }
}
