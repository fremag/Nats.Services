using BrightIdeasSoftware;
using StoreServices;
using System;

namespace StoreClient
{
    public class ProductInfo : StoreRowItem<Product>
    {
        public Product Product { get; private set; }
        public ProductInfo()
        {

        }
        public ProductInfo(Product product)
        {
            Product = product;
            LastUpdate = DateTime.Now;
        }
        public void Update(Product product)
        {
            if( Product == null)
            {
                Product = product;
                return;
            }
            Product.Price = product.Price;
            Product.Quantity= product.Quantity;
            LastUpdate = DateTime.Now;
        }

        [OLVColumn]
        public string Name => Product.Name;
        [OLVColumn]
        public string Category => Product.Category;
        [OLVColumn]
        public double Price => Product.Price;
        [OLVColumn]
        public int Quantity => Product.Quantity;
        [OLVColumn(AspectToStringFormat = "{0:HH:mm:ss}")]
        public DateTime LastUpdate { get; set; }
    }
}
