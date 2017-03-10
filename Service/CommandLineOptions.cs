using CommandLine;
using CommandLine.Text;

namespace DCS
{
    /// <summary>
    ///     see http://commandline.codeplex.com/
    ///     see https://github.com/gsscoder/commandline/wiki
    /// </summary>
    internal class CommandLineOptions
    {
        [Option('r', "run", HelpText = "Run the service interactively as a command line application.",MutuallyExclusiveSet = "SingleAction")]
        public bool Run { get; set; }

        [Option('i', "install", HelpText = "Install the windows service", MutuallyExclusiveSet = "SingleAction")]
        public bool Install { get; set; }

        [Option('u', "uninstall", HelpText = "Uninstall the windows service", MutuallyExclusiveSet = "SingleAction")]
        public bool Uninstall { get; set; }

        [Option('t',"test",HelpText = "Execute the ReadValues call for each reader, only one time.  You may use that for testing.",MutuallyExclusiveSet = "SingleAction")]
        public bool Test { get; set; }

        [Option('s', "stats", HelpText = ".", MutuallyExclusiveSet = "SingleAction")]
        public bool Stats { get; set; }


        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
