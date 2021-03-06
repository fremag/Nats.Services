﻿using Nats.Services.Core;
using Nats.Services.Core.DiscoveryService;
using NATS.Client;
using NLog;
using NLog.Config;
using NLog.Targets;
using StoreServices;
using System;
using System.Reflection;

namespace StoreServer
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
                string agentName = "TestServer";

                var discoveryService = new DiscoveryService(agentName);
                var productStoreService = new ProductStoreServiceIml();
                
                var serviceFactory = new NatsServiceFactory(connection, agentName);
                var productStoreServer = serviceFactory.BuildServiceServer<IProductStoreService>(productStoreService);
                var discoveryserver = serviceFactory.BuildServiceServer<IDiscoveryService>(discoveryService);


                Console.ReadKey();
            }
        }
    }
}
