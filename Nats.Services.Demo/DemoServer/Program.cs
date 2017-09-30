using DemoService;
using Nats.Services.Core;
using NATS.Client;
using System;

namespace DemoServer
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
                var serviceImpl = new DemoServiceImpl();
                serviceImpl.StatusUpdatedEvent += status => Console.WriteLine($"Send status: {status}");
                var serviceFactory = new NatsServiceFactory(connection);
                var natsServer = serviceFactory.BuildServiceServer<IDemoService>(serviceImpl);

                Console.ReadKey();
            }
        }
    }
}
