using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace WSR.Core.Configuration
{
    public static class Config
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Config));
        public static RootObject Settings { get; private set; }
        const string ConfigurationFileName = "settings.json";

        static Config()
        {
            Load();
        }


        public static void Load()
        {
            Logger.InfoFormat("Loading settings from {0}",ConfigurationFileName);

            try
            {
                if (File.Exists(ConfigurationFileName))
                {
                    Settings = JsonConvert.DeserializeObject<RootObject>(File.ReadAllText(ConfigurationFileName));
                }
                else
                {
                    Logger.Fatal("Could not load application settings.");
                    throw new FileNotFoundException(string.Format("File {0} was not found",ConfigurationFileName));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Could not load settings");
            }
            
           
        }

        
    }

 


    public interface IConfig
    {
        void SetDefaultSettings();
    }
}
