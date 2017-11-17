using Nats.Services.Core;
using Nats.Services.Core.DiscoveryService;
using NATS.Client;
using NLog;
using StoreServices;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StoreClient
{
    public partial class StoreClientForm : Form
    {
        ILogger logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);
        IConnection connection;
        NatsServiceFactory serviceFactory;
        IProductStoreService storeService;
        StoreClientModel<string, Product, ProductInfo> model = new StoreClientModel<string, Product, ProductInfo>();

        public StoreClientForm()
        {
            InitializeComponent();
        }

        private async void StoreClientForm_LoadAsync(object sender, EventArgs e)
        {
            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = Defaults.Url;
            connection = new ConnectionFactory().CreateConnection(options);
            logger.Info($"Looking for a server.");
            var uiSched = TaskScheduler.FromCurrentSynchronizationContext();

            string serverName = await Task.Run(() => DiscoverServer(connection, logger));
            logger.Info($"Server found: {serverName}.");
            Text += serverName;
            await InitServicesAsync(serverName);
            logger.Info($"Store initialized: {objectListView1.GetItemCount()}");
        }

        private async Task InitServicesAsync(string serverName)
        {
            serviceFactory = new NatsServiceFactory(connection, serverName);
            storeService = serviceFactory.BuildServiceClient<IProductStoreService>();
            await model.InitAsync(storeService, objectListView1);
            Text += $", {objectListView1.GetItemCount()} products";
        }

        public static string DiscoverServer(IConnection connection, ILogger logger=null, int periodMs=1000)
        {
            var serviceFactory = new NatsServiceFactory(connection, "Unknown");
            var discoveryService = serviceFactory.BuildServiceClient<IDiscoveryService>();
            string agentName = null;
            bool serverFound = false;

            discoveryService.EchoEvent += name =>
            {
                agentName = name;
                serverFound = true;
            };

            while (!serverFound)
            {
                logger?.Info("Looking for a server...");
                discoveryService.Sonar();
                Thread.Sleep(periodMs);
            }

            return agentName;
        }

        private void StoreClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.Close();
        }
    }
}
