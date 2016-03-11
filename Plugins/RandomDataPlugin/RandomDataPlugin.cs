using System;
using System.Collections.Generic;
using DCS.Core.Configuration;
using DCS.Core.DataCollectors;
using DCS.Core.DataReaders;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;

namespace RandomDataPlugin
{
    /// <summary>
    /// This data collector can be used to generate random data when you need to test data collection.
    /// It requires a database that contains elements based on an ElementTemplate with the following Attributes: 
    /// value:     the PI Point where the data will be written
    /// LowValue:  An integer static attribute that defines the lowest random value
    /// HighValue: An integer static attribute that defines the highest random value
    /// </summary>
    /// 
    /*

    JSON Settings to configure this data collector.
    
    "ReaderType": "RandomReader",
    "ReaderTaskDescription": "Random data collector",
    "AFDatabaseName": "WebServiceDataReader",
    "AFElementTemplateName": "RandomDataReader",
    "AFServerName": "megatron",
    "DataCollectionPeriod": "0/5 * * * * ?"
    
    */
    public class RandomDataPlugin : BaseDataReader, IDataCollector 
    {
        Random _randomGenerator=new Random();
        private DataCollectorSettings _settings;


        public override List<AFValue> ReadValues(AFElement element)
        {
            var attribute = element.Attributes["Value"];
            var min= (int)element.Attributes["LowValue"].GetValue().Value;
            var max = (int)element.Attributes["HighValue"].GetValue().Value;

            var newValue = _randomGenerator.Next(min,max);
            var afValue=new AFValue(attribute,newValue,AFTime.Now);

            return new List<AFValue>() {afValue};
        }



        public DataCollectorSettings GetSettings()
        {
            return _settings;
        }

        public void SetSettings(DataCollectorSettings settings)
        {
            _settings = settings;

            AfDatabaseName = _settings.AFDatabaseName;
            AfElementTemplateName = _settings.AFElementTemplateName;
            AfServerName = _settings.AFServerName;
        }

    }
}
