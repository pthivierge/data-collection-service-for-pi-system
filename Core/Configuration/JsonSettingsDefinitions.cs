using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCS.Core.Configuration
{
    // Make changes in settings.json
    // then use this site to generate your classes here
    // http://json2csharp.com/#


    public class MainSettings
    {
        public string CronPeriod_DataWrite { get; set; }
        public string CronPeriod_ConfigurationRefresh { get; set; }
        public string WebConfigPort { get; set; }
    }

    public class DataCollectorSettings
    {

        public int LoadPlugin { get; set; }
        public string PluginFileName { get; set; }
        public string PluginClassName { get; set; }
        public string ReaderTaskDescription { get; set; }
        public string AFDatabaseName { get; set; }
        public string AFElementTemplateName { get; set; }
        public string AFServerName { get; set; }
        public string DataCollectionPeriod { get; set; }
        public string APIUrl { get; set; }
        public string AppKey { get; set; }
        public dynamic ExtendedSettings { get; set; }
    }

    public class DebugSettings
    {
        public bool StartDebuggerOnStart { get; set; }
    }

    public class RootObject
    {
        public MainSettings MainSettings { get; set; }
        public List<DataCollectorSettings> DataCollectorsSettings { get; set; }
        public DebugSettings DebugSettings { get; set; }
    }





}
