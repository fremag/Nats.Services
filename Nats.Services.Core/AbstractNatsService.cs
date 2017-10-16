using Castle.DynamicProxy;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nats.Services.Core
{
    public abstract class AbstractNatsService<T> 
    {
        public string AgentName { get; }
        public string ServiceName { get; }

        protected NatsServiceSerializer<T> serializer = new NatsServiceSerializer<T>();
        protected IConnection connection;
        protected List<IAsyncSubscription> subscriptions = new List<IAsyncSubscription>();

        public AbstractNatsService(IConnection connection, string agentName)
        {
            this.connection = connection;
            AgentName = agentName;

            var attrib = typeof(T).GetCustomAttribute<NatsServiceAttribute>();
            if (attrib == null)
            {
                ServiceName = typeof(T).Name;
            }
            else
            {
                ServiceName = attrib.ServiceName;
            }
        } 

        public string GetListenSubject(MemberInfo memberInfo)
        {
            string name = IsMemberGlobal(memberInfo) ? "*" : AgentName;
            return $"{name}.{ServiceName}.{memberInfo.Name}";
        }

        private bool IsMemberGlobal(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttribute<NatsServiceGlobalAttribute>() != null;
        }

        public string GetPublishSubject(MemberInfo memberInfo)
        {
            string name = IsMemberGlobal(memberInfo) ? "ANYONE" : AgentName;
            return $"{name}.{ServiceName}.{memberInfo.Name}";
        }

        public byte[] BuildPayload(IInvocation invoc)
        {
            var paramsInfo = invoc.Method.GetParameters();
            var args = new List<KeyValuePair<string, object>>();
            for(int i=0; i < paramsInfo.Length; i++)
            {
                var paramInfo = paramsInfo[i];
                var arg = invoc.Arguments[i];
                args.Add(new KeyValuePair<string, object>(paramInfo.Name, arg));
            }
            var payload = serializer.SerializeMethodArguments(args);
            return payload;
        }

        public string ToString(IInvocation invoc)
        {
            var payload = BuildPayload(invoc);
            var str = serializer.ToString(payload);
            return str;
        }

        public object[] DecodePayload(MethodInfo methInfo, byte[] payload)
        {
            IList<KeyValuePair<string, object>> args= serializer.DeserializeMethodArguments(payload);
            var parameters = methInfo.GetParameters();
            object[] values = new object[parameters.Length];
            for(int i=0; i < parameters.Length; i++)
            {
                var value = args[i].Value;
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
