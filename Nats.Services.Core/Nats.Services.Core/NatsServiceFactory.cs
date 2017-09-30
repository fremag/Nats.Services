using Castle.DynamicProxy;
using NATS.Client;

namespace Nats.Services.Core
{
    public class NatsServiceFactory
    {
        private static ProxyGenerator genertor = new ProxyGenerator();
        private IConnection connection;

        public NatsServiceFactory(IConnection connection)
        {
            this.connection = connection;
        }

        public T BuildServiceClient<T>() where T: class
        {
            return genertor.CreateInterfaceProxyWithoutTarget<T>(new NatsServiceClient<T>(connection));
        }

        public T BuildServiceServer<T>(T serviceImpl)
        {
            var server = new NatsServiceServer<T>(connection, serviceImpl);
            return serviceImpl;
        }
    }

}
