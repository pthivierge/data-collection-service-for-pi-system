using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIsoft.AF.Asset;

namespace GitHubDataPlugin
{
    public class PluginParams
    {
        /// <summary>
        /// The organization name to search for
        /// </summary>
        public string GitHubOwner { get; set; }
        
        /// <summary>
        /// Application credentials token to use to connect to GitHub
        /// <see cref="https://github.com/settings/tokens"/>
        /// </summary>
        public string GithubCredentialToken { get; set; }

        /// <summary>
        /// Name of the product that is using octokit
        /// </summary>
        public string GitHubProductName { get; set; }

        /// <summary>
        /// Repository of the 
        /// </summary>
        public AFElementTemplate RepositoryTemplate { get; set; }
    }


    
}
