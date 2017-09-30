using System;
using System.Collections.Generic;
using System.Text;
using NATS.Client;
using Castle.DynamicProxy;
using System.Reflection;
using System.Linq;
using Castle.DynamicProxy.Generators;

namespace Nats.Services.Core
{
    public class NatsServiceServer<T> : AbstractNatsService<T>
    {
        private static ProxyGenerator generator = new ProxyGenerator();
        private T serviceImpl;
        private Dictionary<IAsyncSubscription, MethodInfo> dicoMethodInfoBySubscription = new Dictionary<IAsyncSubscription, MethodInfo>();

        public NatsServiceServer(IConnection connection, T serviceImpl) : base(connection)
        {
            this.connection = connection;
            this.serviceImpl = serviceImpl;

            foreach(var methInfo in typeof(T).GetMethods())
            {
                var subject = GetSubject(methInfo.Name);
                var asyncSub = SubscribeAsync(subject, OnMessage);
                dicoMethodInfoBySubscription[asyncSub] = methInfo;
            }


            if (typeof(T).GetEvents().Any())
            {
                var scope = generator.ProxyBuilder.ModuleScope;
                foreach(var eventInfo in typeof(T).GetEvents())
                {
                    var delegType = eventInfo.EventHandlerType;
                    var delegGen = new DelegateProxyGenerator(scope, delegType);
                    var eventInterceptor = new EventInterceptor<T>(eventInfo.Name, connection, GetSubject(eventInfo.Name), BuildPayload);
                    object nullFunc = Convert.ChangeType(null, delegType);
                    var proxyType = delegGen.GetProxyType();
                    var instance = Activator.CreateInstance(proxyType, nullFunc, new IInterceptor[] { eventInterceptor });
                    var deleg = Delegate.CreateDelegate(delegType, instance, "Invoke");
                    eventInfo.GetAddMethod().Invoke(serviceImpl, new object[] { deleg });
                }
            }
        }

        private void OnMessage(object sender, MsgHandlerEventArgs e)
        {
            var asyncSub = sender as IAsyncSubscription;
            if( dicoMethodInfoBySubscription.TryGetValue(asyncSub, out MethodInfo methInfo))
            {
                object[] parameters = DecodePayload(methInfo, e.Message.Data);
                var result = methInfo.Invoke(serviceImpl, parameters);
                if(! string.IsNullOrEmpty(e.Message.Reply))
                {
                    var payload = serializer.Serialize(result);
                    asyncSub.Connection.Publish(e.Message.Reply, payload);
                }
            }
        }
    }
}
