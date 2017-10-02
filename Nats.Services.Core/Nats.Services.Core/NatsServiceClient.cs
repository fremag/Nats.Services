using System;
using NATS.Client;
using Castle.DynamicProxy;
using System.Linq;
using System.Reflection;
using NLog;

namespace Nats.Services.Core
{
    public class NatsServiceClient<T> : AbstractNatsService<T>, IInterceptor
    {
        static Logger logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public NatsServiceClient(IConnection connection) : base(connection)
        {
            if (logger.IsDebugEnabled) logger.Debug($"NatsServiceClient: {typeof(T).Name}");
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.IsSpecialName)
            {
                var eventInfo = typeof(T).GetEvents().FirstOrDefault(evt => evt.AddMethod.Name == invocation.Method.Name);
                if (eventInfo != null)
                {
                    var deleg = invocation.Arguments[0] as MulticastDelegate;
                    var eventsubject = GetSubject(eventInfo.Name);
                    var sub = new NatsServiceEventSubscribtion<T>(DecodePayload, eventsubject, deleg);
                    var asyncSub = SubscribeAsync(eventsubject, sub.OnMessage);
                    if (logger.IsDebugEnabled) logger.Debug($"NatsServiceClient: {typeof(T)}, Event: {eventInfo.Name}, subject: {asyncSub.Subject}");
                    return;
                }
            }

            if(typeof(T).GetMethods().Any(method => method.Name == invocation.Method.Name))
            {
                var payload = BuildPayload(invocation);
                var subject = GetSubject(invocation.Method.Name);
                if (logger.IsDebugEnabled) logger.Debug($"NatsServiceClient: {typeof(T)}, Method: {invocation.Method.Name}, subject: {subject}, parameters: {serializer.ToString(payload)}");
                if ( invocation.Method.ReturnType == typeof(void))
                {
                    connection.Publish(subject, payload);
                }
                else
                {
                    var reply = connection.Request(subject, payload, 1000);
                    var dicoResult = serializer.Deserialize(reply.Data);

                    invocation.ReturnValue = dicoResult[ResultKey];
                    if (logger.IsDebugEnabled) logger.Debug($"NatsServiceClient: {typeof(T)}, Method: {invocation.Method.Name}, result: {serializer.ToString(reply.Data)}");
                }
            }
        }
    }
}
