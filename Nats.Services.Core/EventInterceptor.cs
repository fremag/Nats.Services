using Castle.DynamicProxy;
using NATS.Client;
using NLog;
using System.Reflection;

namespace Nats.Services.Core
{
    public class EventInterceptor<T> : IInterceptor
    {
        static Logger logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        private string name;
        private IConnection connection;
        private string subject;
        private AbstractNatsService<T> natsService;

        public EventInterceptor(string name, IConnection connection, string subject, AbstractNatsService<T> natsService)
        {
            this.name = name;
            this.connection = connection;
            this.subject = subject;
            this.natsService = natsService;
        }

        public void Intercept(IInvocation invocation)
        {
            if (logger.IsDebugEnabled) logger.Debug($"FireEvent: {name}, subject: {subject}, args: {natsService.ToString(invocation)}");
            byte[] payload = natsService.BuildPayload(invocation);
            connection.Publish(subject, payload);
        }
    }
}
