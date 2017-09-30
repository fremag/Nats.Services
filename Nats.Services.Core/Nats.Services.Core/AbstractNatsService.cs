using Castle.DynamicProxy;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nats.Services.Core
{
    public abstract class AbstractNatsService<T> 
    {
        protected NatsServiceSerializer<T> serializer = new NatsServiceSerializer<T>();
        protected IConnection connection;
        protected List<IAsyncSubscription> subscriptions = new List<IAsyncSubscription>();
        protected readonly string ResultKey = "result";

        public AbstractNatsService(IConnection connection)
        {
            this.connection = connection;
        } 

        public string GetSubject(string name)
        {
            return $"{typeof(T).FullName}.{name}";
        }

        protected byte[] BuildPayload(IInvocation invoc)
        {
            var paramsInfo = invoc.Method.GetParameters();
            var dico = new Dictionary<string, object>();
            for(int i=0; i < paramsInfo.Length; i++)
            {
                var paramInfo = paramsInfo[i];
                var arg = invoc.Arguments[i];
                dico[paramInfo.Name] = arg;
            }
            var payload = serializer.Serialize(dico);
            return payload;
        }

        protected object[] DecodePayload(MethodInfo methInfo, byte[] payload)
        {
            Dictionary<string, object> dico = serializer.Deserialize(payload);
            var parameters = methInfo.GetParameters();
            object[] values = new object[parameters.Length];
            for(int i=0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var value = dico[param.Name];
                values[i] = value;
            }
            return values;
        }

        protected IAsyncSubscription SubscribeAsync(string subject, EventHandler<MsgHandlerEventArgs> callback)
        {
            var asyncSub = connection.SubscribeAsync(subject, callback);
            subscriptions.Add(asyncSub);
            return asyncSub;
        }
    }
}
