using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DCS.Core.DataCollectors;
using DCS.Core.DataReaders;
using log4net;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using Quartz;
using DCS.Core.Helpers;

namespace DCS.Core
{
    /// <summary>
    /// Maintains the list of data collectors into memory
    /// Contains methods to make initialisation and / or periodic configuration update for the devices.
    /// </summary>
    public class DataReadersManager : IDisposable
    {
        ILog _logger = LogManager.GetLogger(typeof(DataReadersManager));
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken _cancellationToken;
        public List<IDataCollector> DataReaders=new List<IDataCollector>();

        private ManualResetEvent dataCollectionCompleted = new ManualResetEvent(true);
        private ManualResetEvent refreshCompleted = new ManualResetEvent(true);
        private ManualResetEvent initCompleted = new ManualResetEvent(true);


        public DataReadersManager()
        {
            _cancellationToken = _cancellationTokenSource.Token;
        }

        /// <summary>
        /// This method should be called only once after all readers have been added to the DataReaders collection
        /// It makes first time initialization for all the readers
        /// </summary>
        public void InitializeReaders()
        {

            _logger.Info("Initializing data collectors");

            initCompleted.Reset();
            try
            {
                Parallel.ForEach(DataReaders, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, (reader, loopState) =>
                {
                    try
                    {
                       if(_cancellationToken.IsCancellationRequested)
                            loopState.Break();

                       reader.Inititialize();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                    
                });
            }
            finally
            {
                initCompleted.Set();
            }
            
            
        }

        public void RefreshReadersConfiguration()
        {
            _logger.Info("Refreshing readers configuration");

            refreshCompleted.Reset();
            try
            {
                Parallel.ForEach(DataReaders, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, (reader, loopState) =>
                {
                    try
                    {
                        if (_cancellationToken.IsCancellationRequested)
                            loopState.Break();

                        reader.RefreshConfiguration();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }

                });
            }
            finally
            {
                refreshCompleted.Set();
            }
        }

        public void CollectReadersData()
        {
            _logger.Debug("Collecting data");

            dataCollectionCompleted.Reset();
            try
            {
                Parallel.ForEach(DataReaders, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, (reader, loopState) =>
                {
                    try
                    {
                        if (_cancellationToken.IsCancellationRequested)
                            loopState.Break();

                        reader.CollectData();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }

                });
            }
            finally
            {
                dataCollectionCompleted.Set();
            }
        }

 

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();

            WaitHandle.WaitAll( new WaitHandle[] { dataCollectionCompleted, refreshCompleted, initCompleted});
        }
    }
}
