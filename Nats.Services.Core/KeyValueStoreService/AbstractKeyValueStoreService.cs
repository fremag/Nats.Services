using System;
using System.Collections.Generic;

namespace Nats.Services.Core.KeyValueStoreService
{
    public abstract class AbstractKeyValueStoreService<T_Key, T_Value> : IKeyValueStoreService<T_Key, T_Value> where T_Value : IKeyIdentifiable<T_Key>
    {
        public event Action<T_Value> ValueInserted;
        public event Action<T_Value> ValueUpdated;
        public event Action<T_Value> ValueDeleted;

        public abstract List<T_Value> GetAllValues();
        public abstract void DoInsert(T_Value value);
        public abstract void DoUpdate(T_Value value);
        public abstract void DoDelete(T_Value value);

        public void Insert(T_Value value)
        {
            DoInsert(value);
            ValueInserted?.Invoke(value);
        }

        public void Update(T_Value value)
        {
            DoUpdate(value);
            ValueUpdated?.Invoke(value);
        }

        public void Delete(T_Value value)
        {
            DoDelete(value);
            ValueDeleted?.Invoke(value);
        }
    }
}
