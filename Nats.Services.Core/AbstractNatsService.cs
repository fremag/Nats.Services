using Castle.DynamicProxy;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Linq;
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
                ServiceName = typeof(T).FullName;
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
            var dico = new Dictionary<string, object>();
            for(int i=0; i < paramsInfo.Length; i++)
            {
                var paramInfo = paramsInfo[i];
                var arg = invoc.Arguments[i];
                dico[paramInfo.Name] = arg;
            }
            var payload = serializer.SerializeMethodArguments(dico);
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
            Dictionary<string, object> dico = serializer.DeserializeMethodArguments(payload);
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

        protected EventInfo GetEventInfo(string eventName)
        {
            return GetAllEventInfos().FirstOrDefault(evt => evt.AddMethod.Name == eventName);
        }

        protected IEnumerable<EventInfo> GetAllEventInfos()
        {
            foreach(var evtInfo in typeof(T).GetEvents())
            {
                yield return evtInfo;
            }
            foreach (var interfaceType in typeof(T).GetInterfaces())
            {
                foreach (var evtInfo in interfaceType.GetEvents())
                {
                    yield return evtInfo;
                }
            }
        }

        protected MethodInfo GetMethodInfo(string methodName)
        {
            return GetAllMethodInfos().FirstOrDefault(meth => meth.Name == methodName);
        }

        protected IEnumerable<MethodInfo> GetAllMethodInfos()
        {
            foreach(var methInfo in typeof(T).GetMethods())
            {
                yield return methInfo;
            }
            foreach (var interfaceType in typeof(T).GetInterfaces())
            {
                foreach (var methInfo in interfaceType.GetMethods())
                {
                    yield return methInfo;
                }
            }
        }
    }
}
