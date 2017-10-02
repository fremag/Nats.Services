using DemoService;
using Nats.Services.Core;
using NATS.Client;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Reflection;

namespace DemoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);
            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);
            LogManager.Configuration = config;
            var logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);
            logger.Info("Server started !");

            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = Defaults.Url;
            using (var connection = new ConnectionFactory().CreateConnection(options))
            {
                var serviceImpl = new DemoServiceImpl();
                serviceImpl.StatusUpdatedEvent += status => logger.Info($"Send status: {status}");
                var serviceFactory = new NatsServiceFactory(connection, "TestServer");
                var natsServer = serviceFactory.BuildServiceServer<IDemoService>(serviceImpl);

                Console.ReadKey();
            }
        }
    }
}
