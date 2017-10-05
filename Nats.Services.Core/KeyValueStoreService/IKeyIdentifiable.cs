namespace Nats.Services.Core.KeyValueStoreService
{
    public interface IKeyIdentifiable<T>
    {
        T Key { get; }
    }
}
