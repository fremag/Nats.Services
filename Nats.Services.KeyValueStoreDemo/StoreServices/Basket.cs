using Nats.Services.Core.KeyValueStoreService;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace StoreServices
{

    [DataContract]
    public class Basket : IKeyIdentifiable<int>
    {
        [DataMember]
        public int Key { get; set;}
        [DataMember]
        public string CustomerName { get; set; }
        [DataMember]
        List<Product> Products { get; set; }
    }
}
