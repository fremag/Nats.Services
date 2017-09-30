using Castle.DynamicProxy;
using NATS.Client;
using System;

namespace Nats.Services.Core
{
    public class EventInterceptor<T> : IInterceptor
    {
        private string name;
        private IConnection connection;
        private string subject;
        private Func<IInvocation, byte[]> payloadBuilder;

        public EventInterceptor(string name, IConnection connection, string subject, Func<IInvocation, byte[]> payloadBuilder)
        {
            this.name = name;
            this.connection = connection;
            this.subject = subject;
            this.payloadBuilder = payloadBuilder;
        }

        public void Intercept(IInvocation invocation)
        {
            byte[] payload = payloadBuilder(invocation);
            connection.Publish(subject, payload);
        }
    }
}
