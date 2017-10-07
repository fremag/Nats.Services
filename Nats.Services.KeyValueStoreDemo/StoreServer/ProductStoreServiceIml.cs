using Nats.Services.Core.KeyValueStoreService;
using StoreServices;
using System.Threading;
using System;
using NLog;
using System.Reflection;

namespace StoreServer
{
    public class ProductStoreServiceIml : MemoryKeyValueStoreService<string, Product>, IProductStoreService
    {
        private Random rand = new Random(0);
        static ILogger logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public ProductStoreServiceIml()
        {
            Insert(new Product { Category = "Fruit", Key = "Orange", Price = 1, Quantity = 5 });
            Insert(new Product { Category = "Fruit", Key = "Apple", Price = 1, Quantity = 5 });
            Insert(new Product { Category = "Fruit", Key = "Blackberry", Price = 1, Quantity = 5 });
            Insert(new Product { Category = "Fruit", Key = "Banana", Price = 1, Quantity = 5 });
            Insert(new Product { Category = "Fruit", Key = "Grapefruit", Price = 1, Quantity = 5 });
            Insert(new Product { Category = "Vegetable", Key = "Tomato", Price = 1, Quantity = 5 });
            Insert(new Product { Category = "Vegetable", Key = "Beans", Price = 1, Quantity = 5 });
            Insert(new Product { Category = "Vegetable", Key = "Carrots", Price = 1, Quantity = 5 });
            Insert(new Product { Category = "Vegetable", Key = "Pumpkin", Price = 1, Quantity = 5 });
            Insert(new Product { Category = "Vegetable", Key = "Spinach", Price = 1, Quantity = 5 });

            Timer timer = new Timer(DoUpdateStore, this, 1000, 2000);
        }

        private void DoUpdateStore(object state)
        {
            var products = GetAllValues();
            int idx = rand.Next(products.Count);
            var product = products[idx];

            product.Price += (1-rand.Next(3)) / 100d;
            product.Quantity += rand.Next(2);
//            logger.Info($"Update - {product}");
            Update(product);
        }
    }
}
