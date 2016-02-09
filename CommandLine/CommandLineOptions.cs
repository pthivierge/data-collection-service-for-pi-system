using CommandLine;
using CommandLine.Text;

namespace FDS.CommandLine
{
    /// <summary>
    ///     see http://commandline.codeplex.com/
    ///     see https://github.com/gsscoder/commandline/wiki
    /// </summary>
    internal class CommandLineOptions
    {
        [Option('t', "test", HelpText = "Runs the service as a command line application.")]
        public bool Test { get; set; }

        // examples
        //[Option(null, "lenght", DefaultValue = -1, HelpText = "The maximum number of bytes to process.")]
        //public int MaximumLenght { get; set; }

        //[Option("v", null, HelpText = "Print details during execution.")]
        //public bool Verbose { get; set; }


        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
