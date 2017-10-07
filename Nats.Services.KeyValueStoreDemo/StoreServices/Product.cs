using Nats.Services.Core.KeyValueStoreService;
using System.Runtime.Serialization;

namespace StoreServices
{
    [DataContract]
    public class Product : IKeyIdentifiable<string>
    {
        [DataMember]
        public string Key { get; set; }

        public string Name => Key;

        [DataMember]
        public string Category { get; set; }
        [DataMember]
        public double Price { get; set; }
        [DataMember]
        public int    Quantity { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, Category: { Category}, Price: {Price}, Quantity: {Quantity}";
        }
    }
}
