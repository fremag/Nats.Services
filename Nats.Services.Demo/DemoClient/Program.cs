using DemoService;
using Nats.Services.Core;
using NATS.Client;
using System;
using System.Threading;

namespace DemoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = Defaults.Url;


            Console.WriteLine("Hello World!");

            using (var connection = new ConnectionFactory().CreateConnection(options))
            {

                var serviceFactory = new NatsServiceFactory(connection);
                IDemoService service = serviceFactory.BuildServiceClient<IDemoService>();

                service.StatusUpdatedEvent += OnStatusUpdated;

                Timer timer = new Timer(OnTimer, service, 1000, 2000);

                Console.ReadKey();
            }

        }

        private static void OnTimer(object state)
        {
            var service = state as IDemoService;
            var time = service.GetTime("HH:mm:ss");
            Console.WriteLine($"Time: {time}");
        }

        private static void OnStatusUpdated(string status)
        {
            Console.WriteLine($"Status received: {status}");
        }
    }
}
