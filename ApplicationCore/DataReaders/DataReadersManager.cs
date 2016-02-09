using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using OSIsoft.AF.Asset;

namespace FDS.Core.DataReaders
{
    public class DataReadersManager
    {
        ILog _logger = LogManager.GetLogger(typeof(DataReadersManager));

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private Timer _readTimer;
        private Task _readTask;

        private BlockingCollection<List<AFElement>> processQueue = new BlockingCollection<List<AFElement>>();

        public void Start()
        {

            _logger.InfoFormat("Starting the Data Readers ");

            // set the automatic refresh
            _readTimer = new Timer(AddDevicesToQueue, null, 0, (int)TimeSpan.FromSeconds(Settings.General.Default.Readers_DataUpdateFrequency_s).TotalMilliseconds);

            // starts the reading task
            _readTask = Task.Factory.StartNew(ReadData);


        }


        public void Stop()
        {
            // stops the timer and wait for the current operation to complete
            _logger.Info("Stopping Readers, waiting for remaining operations to complete...");
            processQueue.CompleteAdding();


            WaitHandle waitHandle = new AutoResetEvent(false);
            _readTimer.Dispose(waitHandle);
            waitHandle.WaitOne();
            waitHandle.Dispose();

            var tasks=new List<Task>();
            tasks.Add(_readTask);
            Task.WaitAll(tasks.ToArray());

            _logger.Info("Readers stopped");



        }

        private void AddDevicesToQueue(object obj)
        {
            foreach (List<AFElement> afElements in SharedData.FitBitDevices.ToArray())
            {
                processQueue.Add(afElements);
            }
        }


        private void ReadData()
        {

            foreach (List<AFElement> devices in processQueue.GetConsumingEnumerable(_cancellationToken.Token))
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







        }
    }
}
