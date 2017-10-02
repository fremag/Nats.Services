namespace Nats.Services.Core.DiscoveryService
{
    public delegate void EchoDelegate(string name);

    public interface IDiscoveryService
    {
        event EchoDelegate EchoEvent;
        void Sonar();
    }
}
