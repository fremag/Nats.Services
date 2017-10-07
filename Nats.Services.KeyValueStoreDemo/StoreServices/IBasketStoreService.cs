using Nats.Services.Core.KeyValueStoreService;

namespace StoreServices
{
    public interface IBasketStoreService : IKeyValueStoreService<int, Basket>
    {
    }
}
