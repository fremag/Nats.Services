using DemoService;
using Nats.Services.Core;
using Nats.Services.Core.DiscoveryService;
using NATS.Client;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Reflection;
using System.Threading;

namespace DemoClient
{
    class Program
    {
        static ILogger logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        static void Main(string[] args)
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);
            var rule1 = new LoggingRule("*", LogLevel.Info, consoleTarget);
            config.LoggingRules.Add(rule1);
            LogManager.Configuration = config;

            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = Defaults.Url;

            using (var connection = new ConnectionFactory().CreateConnection(options))
            {
                var serverName = DiscoverServer(connection);
                var serviceFactory = new NatsServiceFactory(connection, serverName);

                IDemoService service = serviceFactory.BuildServiceClient<IDemoService>();
                service.StatusUpdatedEvent += OnStatusUpdated;

                Timer timer = new Timer(OnTimer, service, 1000, 3000);

                logger.Info("Client started !");
                Console.ReadKey();

            }
        }

        private static string DiscoverServer(IConnection connection)
        {
            var serviceFactory = new NatsServiceFactory(connection, "Unknown");
            var discoveryService = serviceFactory.BuildServiceClient<IDiscoveryService>();
            string agentName = null;
            bool serverFound = false;

            discoveryService.EchoEvent += name =>
            {
                agentName = name;
                serverFound = true;
            };

            while(! serverFound)
            {
                logger.Info("Looking for a server...");
                discoveryService.Sonar();
                Thread.Sleep(1000);
            }

            logger.Info("Server found: "+agentName);
            return agentName;
        }

        private static void OnTimer(object state)
        {
            var service = state as IDemoService;
            var time = service.GetTime("HH:mm:ss");
            logger.Info($"Time: {time}");
        }

        private static void OnStatusUpdated(string status)
        {
            logger.Info($"Status received: {status}");
        }
    }
}
