using Nats.Services.Core.KeyValueStoreService;

namespace StoreServices
{
    public interface IProductStoreService : IKeyValueStoreService<string, Product>
    {
    }
}
