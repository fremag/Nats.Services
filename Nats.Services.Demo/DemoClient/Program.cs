using DemoService;
using Nats.Services.Core;
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
            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);
            LogManager.Configuration = config;

            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = Defaults.Url;

            using (var connection = new ConnectionFactory().CreateConnection(options))
            {

                var serviceFactory = new NatsServiceFactory(connection);
                IDemoService service = serviceFactory.BuildServiceClient<IDemoService>();

                service.StatusUpdatedEvent += OnStatusUpdated;

                Timer timer = new Timer(OnTimer, service, 1000, 2000);

                logger.Info("Client started !");
                Console.ReadKey();
            }
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
