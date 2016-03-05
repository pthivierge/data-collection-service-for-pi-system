using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using WSR.Core.Helpers;
using log4net;
using WSR.Core;
using WSR.Core.DataReaders;

namespace WSR.CommandLine
{
    /// <summary>
    ///     Command line program "Main"
    ///     logs are both sent to the log file and in the console
    ///     This can be configured in CommandLine.Log4Net.cfg
    /// </summary>
    internal class Program
    {
        // the manual reset event serves to transmit information to the MonitorAFAttribute task
        private ManualResetEvent _terminationRequest = new ManualResetEvent(false);

        private static void Main(string[] args)
        {
            ILog _logger = LogManager.GetLogger(typeof(Program));
            TextWriter writer = Console.Out;



            try
            {
                _logger.Info("Command Line Started"); // you could delete this line ... 

                var options = new CommandLineOptions();

                if (Parser.Default.ParseArguments(args, options))
                {
                    if (options.Test)
                    {
                        
                        var configurationManager = new ConfigurationManager();
                        var Reader = new DataReadersManager();
                        var dataWriter = new DataWriter();

                        configurationManager.RunOnce();
                        Reader.RunOnce();
                        dataWriter.RunOnce();
                      

                        Console.WriteLine("press a key to stop the data collection");
                        Console.ReadKey();

                        
                        Reader.Dispose();
                        
                        
                        Console.WriteLine("Stopped");


                    }


                    // exit ok
                    Environment.Exit(0);
                }
                else
                {
                    // exit with error
                    Environment.Exit(1);
                }
            }


            catch (Exception ex)
            {
                Console.SetOut(writer);
                Console.WriteLine("Error: " + ex);
            }
        }
    }
}
