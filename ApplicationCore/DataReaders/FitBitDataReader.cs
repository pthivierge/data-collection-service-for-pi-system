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
    public class FitBitDataReader : IWebDataReader
    {
        ILog _logger = LogManager.GetLogger(typeof(FitBitDataReader));

        AuthCredential _credentials;
        private AFElement _deviceElement;
        private int valuesCount = 0;

        private string consumerKey { get; set; }
        private string consumerSecret { get; set; }


        public FitBitDataReader(AFElement device)
        {
            _deviceElement = device;

            // load keys from settings
            consumerKey = Settings.General.Default.AppConsumerKey;
            consumerSecret = Settings.General.Default.AppConsumerSecret;
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

                DateTime startTime = DateTime.Today - TimeSpan.FromDays(30);
                DateTime endTime = DateTime.Now;

                string authToken = (string)_deviceElement.Attributes["AuthToken"].GetValue().Value;
                string authTokenSecret = (string)_deviceElement.Attributes["AuthTokenSecret"].GetValue().Value;


                // for debugging when no internet access
                //if (Settings.Advanced.Default.NoConnectionSImulateData)
                //    return true;

                var fitBitClient = new FitbitClient(consumerKey, consumerSecret, authToken, authTokenSecret);

                // user profile is not used for the time being
                // var userProfile = fitBitClient.GetUserProfile();


                // check the last time the device was updated
                var fitBitDevices = fitBitClient.GetDevices();
                var fitBitLastSync = AFTime.MinValue.LocalTime;
                if (fitBitDevices != null)
                {
                    
                    foreach (var fitBitDevice in fitBitDevices)
                    {
                        if (fitBitLastSync < fitBitDevice.LastSyncTime)
                            fitBitLastSync = fitBitDevice.LastSyncTime;
                    }

                    // The value attribute we get from an AFValue is an object.  In case there is no value yet in it
                    // the value can be of type AFEnumerationSets - so we check to make sure the value is date.
                    var afLastSync = _deviceElement.Attributes["LastSync"].GetValue().Value;
                    if (afLastSync is DateTime)
                    {
                        if ((DateTime)afLastSync >= fitBitLastSync)
                        {
                            _logger.InfoFormat("{0} will not be updated, last sync time is greater or equal the fitbit last sync time ", _deviceElement.Name, valuesCount);
                            return true; // if syncdate has not changed since last sync, we don't update.
                        }


                    }
                }

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

                // updating last sync value
                var attribute = _deviceElement.Attributes["LastSync"];
                var value = new AFValue(attribute, fitBitLastSync, DateTime.Now);
                var list = new List<AFValue>();
                list.Add(value);
                SharedData.DataQueue.Enqueue(list);

                _logger.InfoFormat("Completed data collection for device: {0} - read {1} values.", _deviceElement.Name, valuesCount);
                success = true;

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return success;
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
