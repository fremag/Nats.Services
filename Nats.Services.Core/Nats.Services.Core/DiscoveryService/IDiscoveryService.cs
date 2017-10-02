namespace Nats.Services.Core.DiscoveryService
{
    public delegate void EchoDelegate(string name);

    public interface IDiscoveryService
    {
        [NatsServiceGlobal]
        event EchoDelegate EchoEvent;

        [NatsServiceGlobal]
        void Sonar();
    }
}
