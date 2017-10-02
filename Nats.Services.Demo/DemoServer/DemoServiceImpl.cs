using DemoService;
using System;
using System.Threading;

namespace DemoServer
{
    public class DemoServiceImpl : IDemoService
    {
        public event StatusUpdated StatusUpdatedEvent;
        DateTime startTime = DateTime.Now;
        public DemoServiceImpl()
        {

            Timer timer = new Timer(SendStatus, this, 1000, 5000);
        }

        private void SendStatus(object state)
        {
            var status = $"Service uptime: {(DateTime.Now - startTime).TotalSeconds} seconds.";
            StatusUpdatedEvent(status);
        }


        public string GetTime(string format)
        {
            return DateTime.Now.ToString(format);
        }
    }
}
