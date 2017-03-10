using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using DCS.Core.Configuration;
using DCS.Core.DataCollectors;
using DCS.Core.DataReaders;
using DCS.Core.Helpers;
using Octokit;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Search;
using OSIsoft.AF.Time;



namespace GitHubDataPlugin
{
    /// <summary>
    /// This Data collector Gathers data from GitHub
    /// It requires a database that contains elements based on an ElementTemplate with the following Attributes: 
    /// value:     the PI Point where the data will be written
    /// LowValue:  An integer static attribute that defines the lowest random value
    /// HighValue: An integer static attribute that defines the highest random value
    /// 
    /// APP Token for authentication
    /// <see cref="https://github.com/settings/developers"/>
    /// </summary>
    public class GitHubDataReader : BaseDataReader, IDataCollector
    {

        DataCollectorSettings _settings;

        

        public override List<AFValue> ReadValues(AFElement orgElement)
        {
            
            var settings = GitHubCommon.GetAFSettings(orgElement);
            var github = GitHubCommon.GetGitHubClient(settings);

            if (GitHubCommon.isGitHubRateLimitExceeded(github))
                return new List<AFValue>();


            // Read repositories from GitHub
            var repos = github.Repository.GetAllForOrg(settings.GitHubOwner).Result;

            var values = new List<AFValue>();

            // for each repository, we create it if it does not exist and we retrieve repo values in AF
            foreach (var repo in repos)
            {
               
                var targetRepoElement = GitHubCommon.FindRepositoryById(orgElement, settings, repo.Id.ToString());

                AFElement repoElement;
                if (targetRepoElement.Count == 0)
                {
                    // create new repo element
                    repoElement = new AFElement(repo.Name, settings.RepositoryTemplate);
                    orgElement.Elements.Add(repoElement);
                    orgElement.CheckIn();

                    GitHubCommon.CreateTags(repoElement);
                }

                else
                {
                    // update
                    repoElement = targetRepoElement[0];
                }


                // if name has changed, we rename the element, we keep track of the repository by ids, so we can do that
                // it makes the AF structure easier to navigate
                if (repoElement.Name != repo.Name)
                    repoElement.Name = repo.Name;

                // pull requests
                var pullRequests = github.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name);
                var pullRequestsCount = pullRequests.Result.Count;

                // commits
                var contributors = github.Repository.Statistics.GetContributors(repo.Owner.Login, repo.Name);
                var contributorsCount = contributors.Result.Count;
                var totalCommits = contributors.Result.ToList().Sum(contributor => contributor.Total);


                //Create AFValues based on the GitHub Readings      
                values.AddRange(new List<AFValue>()
                    {

                        new AFValue(repoElement.Attributes["Repository Id"], repo.Id, AFTime.Now),
                        new AFValue(repoElement.Attributes["Commits"], totalCommits, AFTime.Now),
                        new AFValue(repoElement.Attributes["Contributors"], contributorsCount, AFTime.Now),
                        new AFValue(repoElement.Attributes["Forks"], repo.ForksCount, AFTime.Now),
                        new AFValue(repoElement.Attributes["Name"], repo.Name, AFTime.Now),
                        new AFValue(repoElement.Attributes["Pull Requests"], pullRequestsCount, AFTime.Now),
                        new AFValue(repoElement.Attributes["Stars"], repo.StargazersCount, AFTime.Now),
                        new AFValue(repoElement.Attributes["Url"], repo.HtmlUrl, AFTime.Now),
                        new AFValue(repoElement.Attributes["UpdatedAt"], repo.UpdatedAt.LocalDateTime, AFTime.Now),
                        new AFValue(repoElement.Attributes["HasDownloads"], repo.HasDownloads, AFTime.Now),
                        new AFValue(repoElement.Attributes["HasIssues"], repo.HasIssues, AFTime.Now),
                        new AFValue(repoElement.Attributes["Open Issues"], repo.OpenIssuesCount, AFTime.Now),
                        new AFValue(repoElement.Attributes["HasWiki"], repo.HasWiki, AFTime.Now),
                        new AFValue(repoElement.Attributes["Watchers"], github.Activity.Watching.GetAllWatchers(settings.GitHubOwner,repo.Name).Result.Count, AFTime.Now),
                    }
                );
                if (AFSDKHelpers.GetAttributeValue<DateTime>(repoElement, "CreatedAt") <= new DateTime(1970, 1, 1))
                {
                    values.Add(new AFValue(repoElement.Attributes["CreatedAt"], repo.CreatedAt.LocalDateTime, AFTime.Now));
                }
            }

            var rateLimits = github.Miscellaneous.GetRateLimits().Result.Resources;
            Logger.InfoFormat("GitHub rate limits: Search:{0}, Core: {1}", rateLimits.Search.Remaining, rateLimits.Core.Remaining);

            return values;
        }

      


        public DataCollectorSettings GetSettings()
        {
            return _settings;
        }

        public void SetSettings(DataCollectorSettings settings)
        {
            _settings = settings;
            AfDatabaseName = _settings.AFDatabaseName;
            AfElementTemplateName = _settings.AFElementTemplateName;
            AfServerName = _settings.AFServerName;
        }



    }

}
