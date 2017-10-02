using Castle.DynamicProxy;
using NATS.Client;

namespace Nats.Services.Core
{
    public class NatsServiceFactory
    {
        private static ProxyGenerator generator = new ProxyGenerator();
        private IConnection connection;
        public string AgentName { get; }

        public NatsServiceFactory(IConnection connection, string agentName)
        {
            this.connection = connection;
            AgentName = agentName;
        }

        public T BuildServiceClient<T>() where T: class
        {
            return generator.CreateInterfaceProxyWithoutTarget<T>(new NatsServiceClient<T>(connection, AgentName));
        }

        public T BuildServiceServer<T>(T serviceImpl)
        {
            var server = new NatsServiceServer<T>(connection, serviceImpl, AgentName);
            return serviceImpl;
        }
    }

}
