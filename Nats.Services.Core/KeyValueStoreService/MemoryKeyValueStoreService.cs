using System.Collections.Generic;
using System.Linq;

namespace Nats.Services.Core.KeyValueStoreService
{
    public class MemoryKeyValueStoreService<T_Key, T_Value> : AbstractKeyValueStoreService<T_Key, T_Value> where T_Value : IKeyIdentifiable<T_Key>
    {
        Dictionary<T_Key, T_Value> dicoKeyValues = new Dictionary<T_Key, T_Value>();
        List<T_Value> allValues = null;
        public override List<T_Value> GetAllValues()
        {
            if( allValues == null)
            {
                allValues = dicoKeyValues.Values.ToList();
            }
            return allValues;
        }

        public override void DoInsert(T_Value value)
        {
            allValues = null;
            dicoKeyValues[value.Key] = value;
        }

        public override void DoUpdate(T_Value value)
        {
            if (dicoKeyValues.ContainsKey(value.Key))
            {
                dicoKeyValues[value.Key] = value;
            }
        }
        public override void DoDelete(T_Value value)
        {
            if (dicoKeyValues.ContainsKey(value.Key))
            {
                allValues = null;
                dicoKeyValues.Remove(value.Key);
            }
        }

        public override T_Value GetByKey(T_Key key)
        {
            if( dicoKeyValues.TryGetValue(key, out T_Value value))
            {
                return value;
            }
            return default(T_Value);
        }
        
    }
}
