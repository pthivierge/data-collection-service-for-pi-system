using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCS.Core.Helpers;
using log4net;
using Octokit;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Search;

namespace GitHubDataPlugin
{
    public static class GitHubCommon
    {

        private static readonly ILog Logger = LogManager.GetLogger(typeof(GitHubCommon));
        const string RepositoryIdAttributeName="Repository Id";

        public static bool isGitHubRateLimitExceeded(GitHubClient github)
        {
            // Checking the rate limit
            var rateLimit = github.Miscellaneous.GetRateLimits().Result.Resources.Core;
            if (rateLimit.Remaining <= 10)
            {
                Logger.WarnFormat("Current rate limit exceeded. Only {0} left. Will reset at:{1}", rateLimit.Remaining,
                    rateLimit.Reset);

                return true;
            }

            return false;
        }


        public static void ValidateParameters(PluginParams pluginParams)
        {
            if (string.IsNullOrEmpty(pluginParams.GithubCredentialToken) || string.IsNullOrEmpty(pluginParams.GitHubProductName))
                throw new GitHubDataCollectorHasInvalidConfiguration();
        }


        public static List<AFElement> FindRepositoryById(AFElement orgElement, PluginParams pluginParams, string repositoryId)
        {
            AFSearchToken templateToken = new AFSearchToken(AFSearchFilter.Template, AFSearchOperator.Equal, pluginParams.RepositoryTemplate.GetPath());
            AFSearchToken valueToken = new AFSearchToken(AFSearchFilter.Value, AFSearchOperator.Equal, repositoryId, pluginParams.RepositoryTemplate.AttributeTemplates[RepositoryIdAttributeName].GetPath());
            AFElementSearch elementSearch = new AFElementSearch(orgElement.Database, "FindRepositoryById", new[] { templateToken, valueToken });
            elementSearch.Refresh();
            var searchResult = elementSearch.FindElements(0, true, 1).ToList();
            return searchResult;
        }


        public static PluginParams GetAFSettings(AFElement orgElement)
        {
            try
            {
                //getting repository settings from the element:
                var gitHubOwner = AFSDKHelpers.GetAttributeValue<string>(orgElement, "Owner");
                var gitHubCredentialToken = AFSDKHelpers.GetAttributeValue<string>(orgElement, "GitHubCredentialToken");
                var gitHubProductName = AFSDKHelpers.GetAttributeValue<string>(orgElement, "GitHubProductName");

                var settings = new PluginParams()
                {
                    GitHubOwner = gitHubOwner,
                    GitHubProductName = gitHubProductName,
                    GithubCredentialToken = gitHubCredentialToken,
                    RepositoryTemplate = orgElement.Database.ElementTemplates["Repository"]

                };

                ValidateParameters(settings);
                return settings;

            }
            catch (Exception e)
            {
                throw new CouldNotInitializeSettingsFromAFException(e);
            }
        }

        public static GitHubClient GetGitHubClient(PluginParams settings)
        {
            var github = new GitHubClient(new ProductHeaderValue(settings.GitHubProductName));
            github.Connection.Credentials = new Credentials(settings.GithubCredentialToken);
            return github;
        }

        public static void CreateTags(AFElement element)
        {
            var newTags = AFDataReference.CreateConfig(element, true, (obj, afProgressEventArgs) =>
            { // here report progess if needed
            });
            Logger.InfoFormat("{1} Tags Created for new element {0}", element.Name, newTags);
        }


    }
}
