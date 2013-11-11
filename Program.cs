using System;
using System.Configuration;
using Topshelf;

namespace RiemannHealth {
	public class Program {
		public static void Main(string[] args) {
            var appSettings = ConfigurationManager.AppSettings;
            var hostname = appSettings["RiemannHost"];
            var port = UInt16.Parse(appSettings["RiemannPort"]);
            var includeGCStats = Boolean.Parse(appSettings["IncludeGCstats"]);

		    HostFactory.Run(x =>
		        {
		            x.Service<HealthService>(s =>
		                {
		                    s.ConstructUsing(_ => new HealthService(hostname, port, includeGCStats));
		                    s.WhenStarted(tc => tc.Start());
		                    s.WhenStopped(tc => tc.Stop());
		                });
		            x.RunAsLocalSystem();
                    x.SetDescription("Riemann Health Monitor");
                    x.SetDisplayName("RiemannHealth");
                    x.SetServiceName("RiemannHealth");
		        });
		}
	}
}
