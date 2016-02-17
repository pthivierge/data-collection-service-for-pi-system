using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using Quartz;

namespace FDS.Core
{
    /// <summary>
    /// Maintains configuration into memory:
    /// keeps the list of configuration settings to authenticate with Web APIs
    /// </summary>
    public class ConfigurationManager
    {
        ILog _logger = LogManager.GetLogger(typeof(ConfigurationManager));
        
        public ConfigurationManager()
        {
            // this option will force the creation of the PI AF Server in the KST (Known Servers Table) if it does not exist.
            PISystems.DirectoryOptions = PISystems.AFDirectoryOptions.AutoAdd;
        }


        public void RunOnce()
        {
            RefreshConfiguration(null);
        }

        private void RefreshConfiguration(object obj)
        {
            try
            {
                var serverName = Settings.General.Default.AFServerName;
                var databaseName = Settings.General.Default.AFDatabaseName;
                var templateName = Settings.General.Default.AFTemplateForFitBit;

                _logger.InfoFormat("Updating configuration from AF Server: {0}", serverName);

                PISystem piSystem = new PISystems()[serverName];
                piSystem.Connect();

                _logger.InfoFormat("Connected to server : {0}", serverName);

                if (piSystem.Databases.Contains(databaseName))
                {
                    var database = piSystem.Databases[databaseName];

                    // get the AFElements that contains the configuration for the fitbit devices
                    const int chunkSize = 10000;
                    int index = 0;
                    int total;

                    // remove existing items in the queue before configuration is updated
                    List<AFElement> ignoredList;
                    while (SharedData.FitBitDevices.TryDequeue(out ignoredList)) { };

                    do
                    {
                        // loads elements by chunk of 10K values 
                        var elements = database.ElementTemplates[templateName].FindInstantiatedElements(true,
                            AFSortField.Name, AFSortOrder.Ascending, index, chunkSize, out total);

                        var elementCount = elements.Count;
                        if (elementCount == 0)
                            break;

                        List<AFElement> elementsList = elements.Select(e => (AFElement)e).ToList();

                        // forces full load of elements
                        AFElement.LoadElements(elementsList);

                        // Convert a list of AFBaseElement to a list of AFElement
                        elementsList = elementsList.Where(e => (bool)e.Attributes["ReadEnabled"].GetValue().Value == true).ToList();

                        SharedData.FitBitDevices.Enqueue(elementsList);


                        _logger.InfoFormat(" Configuration Manager | StartIndex = {1} | Found a chunk of {2}  elements", DateTime.Now, index, elementCount);

                        index += chunkSize;

                    } while (index < total);

                    // the findElements call we are using returns a paged collection to lower the memory foortprint


                }
                else
                {
                    _logger.Error("Could not refresh the configuration.  Database could not be found");
                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error when trying to update the configuration.", ex);
            }




        }

    }
}
