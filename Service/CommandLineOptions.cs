using CommandLine;
using CommandLine.Text;

namespace WSR
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




        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
