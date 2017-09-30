using NATS.Client;
using System;

namespace Nats.Services.Core
{
    public class NatsServiceEventSubscribption<T>
    {
        private NatsServiceSerializer<T> Serializer { get; set; }
        private string Subject { get; set; }
        private MulticastDelegate EventCallback;

        public NatsServiceEventSubscribption(NatsServiceSerializer<T> serializer, string subject, MulticastDelegate eventCallback)
        {
            Serializer = serializer;
            Subject = subject;
            EventCallback = eventCallback;
        }

        internal void OnMessage(object sender, MsgHandlerEventArgs args)
        {
            IAsyncSubscription sub = sender as IAsyncSubscription;
            var values = Serializer.Deserialize(args.Message.Data);
            EventCallback.DynamicInvoke(values);
        }
    }
}
