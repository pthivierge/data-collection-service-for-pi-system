using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DCS.Core.Configuration;
using DCS.Core.DataCollectors;
using DCS.Core.DataReaders;
using Octokit;
using Octokit;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
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
    public class GitHubTraffic : BaseDataReader, IDataCollector
    {

        DataCollectorSettings _settings;


        public override List<AFValue> ReadValues(AFElement orgElement)
        {
            var values = new List<AFValue>();
            var settings = GitHubCommon.GetAFSettings(orgElement);
            var github = GitHubCommon.GetGitHubClient(settings);

            if (GitHubCommon.isGitHubRateLimitExceeded(github))
                return values;


            // Read repositories from GitHub
            var repos = github.Repository.GetAllForOrg(settings.GitHubOwner).Result;
            

            foreach (var repo in repos)
            {
                var targetRepoElement = GitHubCommon.FindRepositoryById(orgElement, settings, repo.Id.ToString());

                AFElement repoElement;

                // check if the repo exists in the AF structure
                if (targetRepoElement.Count == 0)
                {
                    Logger.InfoFormat("{0} Repository does not exist in AF.  Traffic data will not be collected.", repo.Name);
                }

                else
                {
                    // repo exists 
                    repoElement = targetRepoElement[0];

                    // create the af structure if it does not exist
                    CreateTrafficStructure(repoElement);

                    //traffic
                    // traffic - clones and views
                    var traffic = repoElement.Elements["Traffic"];

                    var clonesTask = github.Repository.Traffic.GetClones(repo.Id, new RepositoryTrafficRequest(TrafficDayOrWeek.Day));
                    var viewsTask = github.Repository.Traffic.GetViews(repo.Id, new RepositoryTrafficRequest(TrafficDayOrWeek.Day));

                    Task.WaitAll(clonesTask, viewsTask);

                    foreach (var clone in clonesTask.Result.Clones)
                    {
                        values.Add(new AFValue(traffic.Attributes["clones-uniques"], clone.Uniques, clone.Timestamp.DateTime));
                        values.Add(new AFValue(traffic.Attributes["clones-count"], clone.Count, clone.Timestamp.DateTime));
                    }


                    foreach (var view in viewsTask.Result.Views)
                    {
                        values.Add(new AFValue(traffic.Attributes["views-uniques"], view.Uniques, view.Timestamp.DateTime));
                        values.Add(new AFValue(traffic.Attributes["views-count"], view.Count, view.Timestamp.DateTime));
                    }



                    // referrers
                    var refElem = repoElement.Elements["Traffic"].Elements["Referrers"];
                    var referrers = github.Repository.Traffic.GetReferrers(repo.Id).Result;
                    foreach (var referrer in referrers)
                    {
                        AFElement refererElem;
                        if (!refElem.Elements.Contains(referrer.Referrer))
                        {
                            refererElem = refElem.Elements.Add(referrer.Referrer, repoElement.Database.ElementTemplates["Referrer"]);
                            refererElem.CheckIn();
                            GitHubCommon.CreateTags(refererElem);
                        }
                        else
                        {
                            refererElem = refElem.Elements[referrer.Referrer];
                        }


                        //Create AFValues based on the GitHub Readings      
                        values.AddRange(new List<AFValue>()
                        {

                            new AFValue(refererElem.Attributes["count"], referrer.Count, AFTime.Now),
                            new AFValue(refererElem.Attributes["uniques"], referrer.Uniques, AFTime.Now)
                        }
                        );


                    }

                    // popular
                    var popularElems = repoElement.Elements["Traffic"].Elements["PopularContents"];
                    var popContents = github.Repository.Traffic.GetPaths(repo.Id).Result;
                    for (int i = 0; i < popContents.Count; i++)
                    {
                        var popContent = popContents[i];
                        var popularElem = popularElems.Elements[i];

                        //Create AFValues based on the GitHub Readings      
                         values.AddRange(new List<AFValue>()
                        {

                            new AFValue(popularElem.Attributes["count"], popContent.Count, AFTime.Now),
                            new AFValue(popularElem.Attributes["uniques"], popContent.Uniques, AFTime.Now),
                            new AFValue(popularElem.Attributes["path"], popContent.Path, AFTime.Now),
                            new AFValue(popularElem.Attributes["title"], popContent.Title, AFTime.Now)
                        }
                        );

                    }


                 



                }
            }

            return values;
        }

        public void CreateTrafficStructure(AFElement repoElement)
        {
            if (!repoElement.Elements.Contains("Traffic"))
            {
                var traffic = repoElement.Elements.Add("Traffic", repoElement.Database.ElementTemplates["Traffic"]);
                var referrers = traffic.Elements.Add("Referrers", repoElement.Database.ElementTemplates["Referrers"]);
                var popularContent = traffic.Elements.Add("PopularContents", repoElement.Database.ElementTemplates["PopularContents"]);

                // Popular content is a fixed number, there is always only 10
                for (int i = 1; i < 11; i++)
                {
                    popularContent.Elements.Add(string.Format("Popular-{0:00}", i), repoElement.Database.ElementTemplates["PopularContent"]);
                }

                repoElement.CheckIn();
                GitHubCommon.CreateTags(repoElement);

            }

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
