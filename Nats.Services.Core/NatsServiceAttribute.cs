using System;

namespace Nats.Services.Core
{
    public class NatsServiceAttribute : Attribute
    {
        public string ServiceName { get; set; }
        public NatsServiceAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }
    }
}
