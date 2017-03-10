using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHubDataPlugin
{
    public class GitHubDataPluginExceptions
    {
        
    }

    public class CouldNotInitializeSettingsFromAFException : ApplicationException
    {
        public CouldNotInitializeSettingsFromAFException(Exception e)
            :base("There was an error while attempting to read settings from AF attributes. Check that attributes exists or are correctly initialized.",e)
        {}
    }


    public class GitHubDataCollectorHasInvalidConfiguration : ApplicationException
    {
        public GitHubDataCollectorHasInvalidConfiguration() : base("The collector could not initialize because the settings were not valid.") { }
    }
}
