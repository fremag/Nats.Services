using System;
using System.Collections.Generic;

namespace Nats.Services.Core.KeyValueStoreService
{
    public interface IKeyValueStoreService<T_Key, T_Value> where T_Value : IKeyIdentifiable<T_Key>
    {
        event Action<T_Value> ValueInserted;
        event Action<T_Value> ValueUpdated;
        event Action<T_Value> ValueDeleted;

        T_Value GetByKey(T_Key key);
        List<T_Value> GetAllValues();

        void Insert(T_Value value);
        void Update(T_Value value);
        void Delete(T_Value value);
    }
}
