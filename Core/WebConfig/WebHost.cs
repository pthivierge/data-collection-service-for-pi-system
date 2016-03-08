using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DCS.Core.Configuration;
using log4net;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Hosting.Self;
using Nancy.TinyIoc;

namespace DCS.Core.WebConfig
{

    public sealed class WebHost : IDisposable
    {
        static readonly ILog _logger = LogManager.GetLogger(typeof(WebHost));
        private static WebHost _webHost;
        private NancyHost _nancyHost;
        private bool _isStarted;

        public static WebHost Instance
        {
            get
            {
                if (_webHost == null)
                {
                    _webHost = new WebHost();
                }
                return _webHost;
            }
        }

        private WebHost()
        {

            if(Config.Settings==null)
                return;

            _logger.InfoFormat("Creating and applying configuration to Web Host");

            try
            {
                _nancyHost = new NancyHost(
                    new Uri("http://localhost:9999"),
                    new NancyBootstrapper(),
                    new HostConfiguration()
                    {
                        UrlReservations = new UrlReservations() { CreateAutomatically = true },
                        RewriteLocalhost = true
                    });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        

        public void Start()
        {

            _logger.InfoFormat("Starting Web Host");

            try
            {
                _nancyHost.Start();
                _isStarted = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }


        }



        public void Dispose()
        {
            if(_isStarted)
                _nancyHost?.Dispose();
        }
        

    }

    public class NancyBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Clear();
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddFile("script.js", "script.js"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("css", "/css"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("js", "/js"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("content", "/content"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("fonts", "/fonts"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("pages", "/views/home/pages"));

        }

        protected override IRootPathProvider RootPathProvider => new CustomRootPathProvider();

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {

        }
    }

    public class CustomRootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "";
            return Path.Combine(applicationDirectory, "WebConfig");
        }
    }


    public class Home : NancyModule
    {
        public Home(IRootPathProvider pathProvider)
        {
            Get["/"] = (page) => View["index.html"];

            Get["/sysinfo"] = (x) =>
            {
                var loadedReaders = Program.DataReadersManager.DataReaders.Select((r)=>r.GetType().ToString());

                return Response.AsJson(loadedReaders);
            };

        }
    }


}

