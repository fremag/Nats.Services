using Nats.Services.Core;

namespace DemoService
{
    public delegate void StatusUpdated(string status);

    [NatsService("DEMO_SERVICE")]
    public interface IDemoService
    {
        event StatusUpdated StatusUpdatedEvent;
        string GetTime(string format);
    }
}
