using NATS.Client;
using System;
using System.Reflection;

namespace Nats.Services.Core
{
    public class NatsServiceEventSubscribtion<T>
    {
        private string Subject { get; set; }
        private MulticastDelegate EventCallback { get; set; }
        private Func<MethodInfo, byte[], object[]> PayloadDecoder { get; set; }

        public NatsServiceEventSubscribtion(Func<MethodInfo, byte[], object[]> payloadDecoder, string subject, MulticastDelegate eventCallback)
        {
            PayloadDecoder = payloadDecoder;
            Subject = subject;
            EventCallback = eventCallback;
        }

        internal void OnMessage(object sender, MsgHandlerEventArgs args)
        {
            IAsyncSubscription sub = sender as IAsyncSubscription;
            var values = PayloadDecoder(EventCallback.Method, args.Message.Data);
            EventCallback.DynamicInvoke(values);
        }
    }
}
