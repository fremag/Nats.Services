namespace Nats.Services.Core.DiscoveryService
{
    public class DiscoveryService : IDiscoveryService
    {
        public event EchoDelegate EchoEvent;

        public string AgentName { get; }
        public DiscoveryService(string agentName)
        {
            AgentName = agentName;
        }
        public void Sonar()
        {
            EchoEvent?.Invoke(AgentName);
        }
    }
}
