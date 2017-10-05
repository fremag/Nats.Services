using System.Collections.Generic;
using System.Linq;

namespace Nats.Services.Core.KeyValueStoreService
{
    public class MemoryKeyValueStoreService<T_Key, T_Value> : AbstractKeyValueStoreService<T_Key, T_Value> where T_Value : IKeyIdentifiable<T_Key>
    {
        Dictionary<T_Key, T_Value> dicoKeyValues = new Dictionary<T_Key, T_Value>();
        public override List<T_Value> GetAllValues()
        {
            return dicoKeyValues.Values.ToList();
        }

        public override void DoInsert(T_Value value)
        {
            dicoKeyValues[value.Key] = value;
        }

        public override void DoUpdate(T_Value value)
        {
            dicoKeyValues[value.Key] = value;
        }
        public override void DoDelete(T_Value value)
        {
            dicoKeyValues.Remove(value.Key);
        }
    }
}
