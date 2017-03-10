using System;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.ServiceProcess;
using CommandLine;
using DCS.Core.WebConfig;
using log4net;
using DCS.Core;
using DCS.Core.Configuration;
using DCS.Core.DataCollectors;
using DCS.Core.DataReaders;
using DCS.Core.Helpers;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.Time;

namespace DCS.Service
{
    internal static class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        /// <summary>
        ///     Service Main Entry Point
        /// </summary>
        private static void Main(string[] args)
        {

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            if (Environment.UserInteractive)
            {
                _logger.Info("Starting service interractively");

                var options = new CommandLineOptions();

                if (Parser.Default.ParseArguments(args, options))
                {
                    if (options.Run)
                    {
                        if (!Config.IsLoaded())
                            Environment.Exit(-1);

                        WebHost.Instance.Start();
                        Core.Program.RunScheduler();

                        Console.WriteLine("press a key to stop the data collection");
                        Console.ReadKey();


                        Core.Program.StopScheduler();

                        WebHost.Instance.Dispose();
                        Console.WriteLine("Stopped");

                    }

                    if (options.Test)
                    {

                        var dataWriter = new DataWriter();

                        // Here are added the configured data collectors
                        foreach (DataCollectorSettings collectorSettings in Config.Settings.DataCollectorsSettings.Where(c => c.LoadPlugin == 1))
                        {

                            try
                            {
                                if (!string.IsNullOrEmpty(collectorSettings.PluginFileName))
                                {
                                    var applicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";
                                    Assembly pluginAssembly = Assembly.LoadFrom(Path.Combine(applicationDirectory, "plugins", collectorSettings.PluginFileName));
                                    foreach (var type in pluginAssembly.GetTypes())
                                    {
                                        if (type.GetInterface(typeof(IDataCollector).Name) != null)
                                        {
                                            var newCollector = Activator.CreateInstance(type) as IDataCollector;
                                            var isValidDataCollector = true;


                                            // performs an additional test, when a plugin class name is provided
                                            if (collectorSettings.PluginClassName != null && newCollector != null)
                                            {
                                                isValidDataCollector = collectorSettings.PluginClassName == newCollector.GetType().Name;
                                            }

                                            if (newCollector != null && isValidDataCollector)

                                            {
                                                _logger.InfoFormat("Loading task of type: {0}. Task description: {1}", type.Name, collectorSettings.ReaderTaskDescription);
                                                newCollector.SetSettings(collectorSettings);
                                                newCollector.Inititialize();
                                                newCollector.CollectData();
                                            }

                                        }
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex);
                            }

                            dataWriter.FlushData();

                            
                            
                        }

                        Console.WriteLine("Test completed, press a key to exit");

                        Console.ReadKey();

                    }

                    if (options.Install)
                    {
                        ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                    }

                    if (options.Uninstall)
                    {
                        ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                    }


                    // this code should be taken elsewhere... leaving it here for now.  unrelevant to this app

                    if(options.Stats)
                    {
                        AFDatabase afdb = null;
                        var conn=AFConnectionHelper.ConnectAndGetDatabase("Optimus", "GitHub", out afdb);
                        Console.WriteLine("repoName,views avg, views min, views max, views std dev, clones avg, clones min, clones max, clones std dev");

                        foreach (var element in afdb.Elements["OSIsoft"].Elements)
                        {
                            var viewsCount = element.Elements["Traffic"].Attributes["views-count"];
                            var clonesCount = element.Elements["Traffic"].Attributes["clones-count"];

                            var timeRange=new AFTimeRange(AFTime.Parse("T-8d"), AFTime.Parse("T"));

                            var views = viewsCount.Data.Summary(timeRange, AFSummaryTypes.All,
                                AFCalculationBasis.EventWeighted, AFTimestampCalculation.Auto);

                            var clones = clonesCount.Data.Summary(timeRange, AFSummaryTypes.All,
                                AFCalculationBasis.EventWeighted, AFTimestampCalculation.Auto);

                           
                            Console.Write(element.Name + ",");
                            Console.Write(val(views[AFSummaryTypes.Average]) + ",");
                            Console.Write(val(views[AFSummaryTypes.Minimum]) + ",");
                            Console.Write(val(views[AFSummaryTypes.Maximum]) + ",");
                            Console.Write(val(views[AFSummaryTypes.StdDev]) + ",");

                            Console.Write(val(clones[AFSummaryTypes.Average]) + ",");
                            Console.Write(val(clones[AFSummaryTypes.Minimum]) + ",");
                            Console.Write(val(clones[AFSummaryTypes.Maximum]) + ",");
                            Console.Write(val(clones[AFSummaryTypes.StdDev]) );
                            Console.Write(Environment.NewLine);


                        }
                        Console.ReadKey();
                    }



                    // exit ok
                    Environment.Exit(0);

                }


            }
            else
            {
                ServiceBase[] ServicesToRun =
                {
                    new Service()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private static string val(AFValue value)
        {
            string res="";

            Type t = value.Value.GetType();

            if (t== typeof(int) || t==typeof(double))
            {
                res = string.Format("{0:0}", value.Value);
            }

            return res;
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Error(e.ExceptionObject);


        }
    }
}
