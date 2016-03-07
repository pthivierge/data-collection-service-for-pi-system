using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace DCS.Core.Configuration
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

        public static bool IsLoaded()
        {
            return Settings != null;
        }


        public static void Load()
        {
            Logger.InfoFormat("Loading settings from {0}",ConfigurationFileName);

            try
            {
                var applicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";
                var configFile = Path.Combine(applicationDirectory, ConfigurationFileName);

                if (File.Exists(configFile))
                {
                    Settings = JsonConvert.DeserializeObject<RootObject>(File.ReadAllText(configFile));
                }
                else
                {
                    Logger.Fatal("Could not load application settings.");
                    throw new FileNotFoundException(string.Format("File {0} was not found", configFile));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            
           
        }

        
    }

 


    public interface IConfig
    {
        void SetDefaultSettings();
    }
}
