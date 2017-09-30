using System;
using System.Collections.Generic;
using System.Text;
using NATS.Client;
using Castle.DynamicProxy;
using System.Linq;

namespace Nats.Services.Core
{
    public class NatsServiceClient<T> : AbstractNatsService<T>, IInterceptor
    {
        public NatsServiceClient(IConnection connection) : base(connection)
        {

        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.IsSpecialName)
            {
                var myEvent = typeof(T).GetEvents().FirstOrDefault(evt => evt.AddMethod.Name == invocation.Method.Name);
                if (myEvent != null)
                {
                    var deleg = invocation.Arguments[0] as MulticastDelegate;
                    var eventsubject = GetSubject(myEvent.Name);
                    var sub = new NatsServiceEventSubscribption<T>(serializer, eventsubject, deleg);
                    SubscribeAsync(eventsubject, sub.OnMessage);
                }
            }

            if(typeof(T).GetMethods().Any(method => method.Name == invocation.Method.Name))
            {
                var payload = BuildPayload(invocation);
                var subject = GetSubject(invocation.Method.Name);
                if( invocation.Method.ReturnType == typeof(void))
                {
                    connection.Publish(subject, payload);
                } else
                {
                    var reply = connection.Request(subject, payload, 1000);
                    var result = serializer.DeserializeObject(reply.Data);
                    invocation.ReturnValue = result;
                }

            }
        }
    }
}
