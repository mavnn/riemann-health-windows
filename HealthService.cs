using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Riemann;

namespace RiemannHealth
{
    public class HealthService
    {
        public bool Cancel { get; set; }

        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool IncludeGC { get; private set; }

        public HealthService(string host, int port = 5555, bool includeGc = false)
        {
            Cancel = false;
            Host = host;
            Port = port;
            IncludeGC = includeGc;
        }

        private HealthService() { }

        public void Start()
        {
            Cancel = false;
            var client = new Client(Host, Port);

            var reporters = Health.Reporters(IncludeGC)
                .ToList();

            Task.Factory.StartNew(() => DoStuffLoop(reporters, client));
        }

        private void DoStuffLoop(List<IHealthReporter> reporters, Client client)
        {
            while (!Cancel)
            {
                foreach (var reporter in reporters)
                {
                    string description;
                    float value;

                    if (reporter.TryGetValue(out description, out value))
                    {
                        string state;
                        if (value < reporter.WarnThreshold)
                        {
                            state = "ok";
                        }
                        else if (value < reporter.CriticalThreshold)
                        {
                            state = "warning";
                        }
                        else
                        {
                            state = "critical";
                        }
                        client.SendEvent(reporter.Name, state, description, value, 1);
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(1.0));
            }
        }

        public void Stop()
        {
            Cancel = true;
        }
    }
}
