using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fitbit.Api;
using Fitbit.Models;
using log4net;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;

namespace FDS.Core.DataReaders
{
    public class FitBitDataReader
    {
        ILog _logger = LogManager.GetLogger(typeof(FitBitDataReader));

        AuthCredential _credentials;
        private AFElement _deviceElement;
        private int valuesCount = 0;

        private string _consumerKey { get; set; }
        private string _consumerSecret { get; set; }

        private DateTime _startTime;
        private DateTime _endTime;
        private string _authToken;
        private string _authTokenSecret;


        public FitBitDataReader(AFElement device)
        {
            _deviceElement = device;

            // load keys from settings
            _consumerKey = Settings.General.Default.AppConsumerKey;
            _consumerSecret = Settings.General.Default.AppConsumerSecret;

            Init();
        }


        private void Init()
        {
            // default start time = 30 days from last sync --> fitbit history on fitbit devices is 30 days 
            _startTime = DateTime.Today - TimeSpan.FromDays(Settings.General.Default.Readers_InitdaysOfHistory);
            _endTime = DateTime.Now;
            _authToken = (string)_deviceElement.Attributes["AuthToken"].GetValue().Value;
            _authTokenSecret = (string)_deviceElement.Attributes["AuthTokenSecret"].GetValue().Value;
        }

        /// <summary>
        /// Reads data from fitbit API.
        /// Limit is 150 API calls per hour, per user. -> 2016-02-05
        /// </summary>
        /// <returns>true if operation completed successfuly</returns>
        public bool Read()
        {
            bool success = false;

            try
            {
                _logger.InfoFormat("Starting data collection for device: {0} ", _deviceElement.Name);

                // for debugging when no internet access
                if (Settings.Advanced.Default.NoConnectionSImulateData)
                    return true;

                var fitBitClient = new FitbitClient(_consumerKey, _consumerSecret, _authToken, _authTokenSecret);

                // user profile is not used for the time being
                // var userProfile = fitBitClient.GetUserProfile();

                // check the last time the device was updated
                var fitBitDevices = fitBitClient.GetDevices();

                if (fitBitDevices == null)
                    _logger.Warn("API call did not returned any fitbit device.");
                else
                    _logger.InfoFormat("fitbit profile contained {0} devices", fitBitDevices.Count);

                var fitBitLastSyncTime = AFTime.MinValue.LocalTime;
                if (fitBitDevices != null)
                {

                    // a user can have many fitbit devices registered - 
                    // goes over all configured user devices and get most recent sync time
                    fitBitLastSyncTime = GetLastSyncTime(fitBitDevices, fitBitLastSyncTime);

                    // The value attribute we get from an AFValue is an object.  In case there is no value yet in it
                    // the value can be of type AFEnumerationSets - so we check to make sure the value is date.
                    var afLastSync = _deviceElement.Attributes["LastSync"].GetValue().Value;
                    if (afLastSync is DateTime)
                    {
                        if ((DateTime)afLastSync >= fitBitLastSyncTime)
                        {
                            _logger.InfoFormat("{0} will not be updated, last sync time is greater or equal the fitbit last sync time ", _deviceElement.Name, valuesCount);

                            return true; // if syncdate has not changed since last sync, we don't update.
                        }

                        //adjust the StartTime according to last sync
                        _startTime = fitBitLastSyncTime - TimeSpan.FromDays(Settings.General.Default.Readers_NormalRunDaysOfHistory);
                        _endTime = fitBitLastSyncTime;
                        _logger.DebugFormat("Gathering data 30days prior last sync time, st: {0} et: {1}", _startTime, _endTime);
                    }
                }


                GetActivityData(fitBitClient, _startTime, _endTime);

                GetSleepData(fitBitClient, _startTime, _endTime);

                UpdateAFLastSyncTime(fitBitLastSyncTime);

                _logger.InfoFormat("Completed data collection for device: {0} - read {1} values.", _deviceElement.Name, valuesCount);
                success = true;

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return success;
        }

        private void UpdateAFLastSyncTime(DateTime fitBitLastSyncTime)
        {
            // updating last sync value into AF
            var attribute = _deviceElement.Attributes["LastSync"];
            var value = new AFValue(attribute, fitBitLastSyncTime, DateTime.Now);
            var list = new List<AFValue>();
            list.Add(value);
            SharedData.DataQueue.Enqueue(list);
        }


        private void GetSleepData(FitbitClient fitBitClient, DateTime startTime, DateTime endTime)
        {
            // sleep
            AFElement sleepElement = _deviceElement.Elements["Sleep"];
            CollectAndSaveData(fitBitClient, startTime, endTime, sleepElement, TimeSeriesResourceType.AwakeningsCount, "Awakenings count");
            CollectAndSaveData(fitBitClient, startTime, endTime, sleepElement, TimeSeriesResourceType.MinutesAsleep, "Hours asleep");
            CollectAndSaveData(fitBitClient, startTime, endTime, sleepElement, TimeSeriesResourceType.MinutesAfterWakeup, "Minutes after wakeup");
            CollectAndSaveData(fitBitClient, startTime, endTime, sleepElement, TimeSeriesResourceType.MinutesAwake, "Minutes awake");
            CollectAndSaveData(fitBitClient, startTime, endTime, sleepElement, TimeSeriesResourceType.MinutesToFallAsleep, "Minutes to fall asleep");
            CollectAndSaveData(fitBitClient, startTime, endTime, sleepElement, TimeSeriesResourceType.SleepEfficiency, "Sleep efficiency");
            CollectAndSaveData(fitBitClient, startTime, endTime, sleepElement, TimeSeriesResourceType.TimeEnteredBed, "Time entered bed");
            CollectAndSaveData(fitBitClient, startTime, endTime, sleepElement, TimeSeriesResourceType.TimeInBed, "Time spent in bed");
        }

        private void GetActivityData(FitbitClient fitBitClient, DateTime startTime, DateTime endTime)
        {
            // activity
            //  --- active hours is calculated - so not here
            AFElement activityElement = _deviceElement.Elements["Activity"];
            CollectAndSaveData(fitBitClient, startTime, endTime, activityElement, TimeSeriesResourceType.CaloriesOut, "Calories");
            CollectAndSaveData(fitBitClient, startTime, endTime, activityElement, TimeSeriesResourceType.DistanceTracker, "Distance");
            CollectAndSaveData(fitBitClient, startTime, endTime, activityElement, TimeSeriesResourceType.MinutesFairlyActiveTracker, "Minutes fairly active");
            CollectAndSaveData(fitBitClient, startTime, endTime, activityElement, TimeSeriesResourceType.MinutesLightlyActiveTracker, "Minutes lightly active");
            CollectAndSaveData(fitBitClient, startTime, endTime, activityElement, TimeSeriesResourceType.MinutesSedentaryTracker, "Minutes sedentary");
            CollectAndSaveData(fitBitClient, startTime, endTime, activityElement, TimeSeriesResourceType.MinutesVeryActiveTracker, "Minutes very active");
            CollectAndSaveData(fitBitClient, startTime, endTime, activityElement, TimeSeriesResourceType.StepsTracker, "Steps");
        }

        private static DateTime GetLastSyncTime(List<Device> fitBitDevices, DateTime fitBitLastSync)
        {
            // loop into all devices to get the latest sync time
            foreach (var fitBitDevice in fitBitDevices)
            {
                if (fitBitLastSync < fitBitDevice.LastSyncTime)
                    fitBitLastSync = fitBitDevice.LastSyncTime;
            }
            return fitBitLastSync;
        }

        private void CollectAndSaveData(FitbitClient fitBitClient, DateTime startTime, DateTime endTime, AFElement deviceElement, TimeSeriesResourceType type, string attributeName)
        {
            var fitBitData = fitBitClient.GetTimeSeries(type, startTime, endTime);
            AFValues values = Helpers.FitBitHelpers.ConvertToAFValues(fitBitData, type, deviceElement, attributeName);
            SharedData.DataQueue.Enqueue(values);
            valuesCount += values.Count;
        }
    }

}
