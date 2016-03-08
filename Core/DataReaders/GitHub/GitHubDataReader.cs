using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;
using Octokit;
using OSIsoft.AF;

namespace DCS.Core.DataReaders
{
    /// <summary>
    /// This data reader can be used to generate random data when you need to test data collection.
    /// It requires a database that contains elements based on an ElementTemplate with the following Attributes: 
    /// value:     the PI Point where the data will be written
    /// LowValue:  An integer static attribute that defines the lowest random value
    /// HighValue: An integer static attribute that defines the highest random value
    /// 
    /// APP Token for authentication
    /// <see cref="https://github.com/settings/developers"/>
    /// </summary>
    public class GitHubDataReader : BaseDataReader, IDataReader
    {


        public override List<AFValue> ReadValues(AFElement orgElement)
        {

            AFElement repoElement;

            //getting repository settings from the element:
            var owner = GetAttributeValue<string>(orgElement, "Owner");
            
            var github = new GitHubClient(new ProductHeaderValue("yourproductnamehere"));
            github.Connection.Credentials = new Credentials("youkeyhere ");

            // Checking the rate limit
            var rateLimit = github.Miscellaneous.GetRateLimits().Result.Resources.Core;
            if (rateLimit.Remaining <= 10)
            {
                Logger.WarnFormat("Current rate limit exceeded. Only {0} left. Will reset at:{1}", rateLimit.Remaining, rateLimit.Reset);
                return new List<AFValue>();
            }
            

            // Read values from GitHub
            var repos = github.Repository.GetAllForOrg(owner).Result;

            var values = new List<AFValue>();
            foreach (var repo in repos)
            {

                if (!orgElement.Elements.Contains(repo.Name))
                {
                    // todo - validation:
                    // check if element template exist
                    // check if repository was renamed ?? maybe, not certain how to do that.
                    repoElement = new AFElement(repo.Name, orgElement.Database.ElementTemplates["repository"]);
                    orgElement.Elements.Add(repoElement);
                    orgElement.CheckIn();

                    var wait=new ManualResetEvent(false);

                    AFDataReference.CreateConfig(orgElement, true,
                        (sender, args) =>
                        {
                            Logger.Info("DataReference Tags creation completed");
                            wait.Set();

                        });
                    wait.WaitOne(30000);
                    Logger.InfoFormat("Tags Created for new element {0}",repoElement.Name);

                }
                else
                {
                    repoElement = orgElement.Elements[repo.Name];
                }
                  
                // pull requests
                var pullRequests = github.PullRequest.GetAllForRepository(repo.Owner.Login,repo.Name);
                var pullRequestsCount = pullRequests.Result.Count;

                // commits
                var contributors = github.Repository.Statistics.GetContributors(repo.Owner.Login, repo.Name);
                var contributorsCount = contributors.Result.Count;
                var totalCommits = contributors.Result.ToList().Sum(contributor => contributor.Total);

                //Create AFValues based on the GitHub Readings      
                values.AddRange(new List<AFValue>()
                    {
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
                    }
                );
                if (GetAttributeValue<DateTime>(repoElement, "CreatedAt")<=new DateTime(1970,1,1))
                {
                    values.Add(new AFValue(repoElement.Attributes["CreatedAt"], repo.CreatedAt.LocalDateTime, AFTime.Now));
                }
            }

            var rateLimits = github.Miscellaneous.GetRateLimits().Result.Resources;
            Logger.InfoFormat("GitHub rate limits: Search:{0}, Core: {1}", rateLimits.Search.Remaining, rateLimits.Core.Remaining);

            return values;
        }

        private T GetAttributeValue<T>(AFElement element, string attributeName)
        {
            return (T)element.Attributes[attributeName].GetValue().Value;
        }



        public GitHubDataReader(string afServerName, string afDataBaseName, string elementTemplateName) : base(afServerName, afDataBaseName, elementTemplateName)
        {
        }
    }
}
