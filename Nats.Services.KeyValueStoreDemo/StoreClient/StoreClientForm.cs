using BrightIdeasSoftware;
using Nats.Services.Core;
using Nats.Services.Core.DiscoveryService;
using NATS.Client;
using NLog;
using StoreServices;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<ProductInfo> productInfos;

        public StoreClientForm()
        {
            InitializeComponent();
            Generator.GenerateColumns(objectListView1, typeof(ProductInfo));
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
            var products = await Task.Run(()=>InitServices(serverName));
            logger.Info($"Store initialized: {products.Count}");
            InitData(products);
        }

        private List<Product> InitServices(string serverName)
        {
            serviceFactory = new NatsServiceFactory(connection, serverName);
            storeService = serviceFactory.BuildServiceClient<IProductStoreService>();
            storeService.ValueUpdated += OnValueUpdated;
            var products = storeService.GetAllValues();
            return products;
        }

        private void OnValueUpdated(Product product)
        {
            if (productInfos == null) return; 
            var productInfo = productInfos.FirstOrDefault(pI => pI.Name == product.Name);
            if( productInfo != null)
            {
                productInfo.Update(product);
                objectListView1.RefreshObject(productInfo);
            }
        }

        private void InitData(List<Product> products)
        {
            Text += $", {products.Count} products";
            var tmp  = products.Select(product => new ProductInfo(product)).ToList();
            productInfos = tmp;
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
