using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Nats.Services.Core
{
    public class NatsServiceSerializer<T>
    {
        DataContractJsonSerializer serializer;

        public NatsServiceSerializer()
        {
            HashSet<Type> types = new HashSet<Type>();
            foreach (var methInfo in typeof(T).GetMethods())
            {
                foreach (var paramInfo in methInfo.GetParameters())
                {
                    types.Add(paramInfo.ParameterType);
                }
                if (methInfo.ReturnType != typeof(void))
                {
                    types.Add(methInfo.ReturnType);
                }
            }

            serializer = new DataContractJsonSerializer(typeof(Dictionary<string, object>), types);
        }

        public Dictionary<string, object> Deserialize(byte[] buffer)
        {
            return DeserializeObject(buffer) as Dictionary<string, object>;
        }

        public object DeserializeObject(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                var obj = serializer.ReadObject(stream);
                return obj;
            }
        }

        public byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, obj);
                var buffer = stream.ToArray();
                return buffer;
            }
        }

        public string ToString(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                stream.Position = 0;
                var streamReader = new StreamReader(stream);
                return streamReader.ReadToEnd();
            }
        }
    }
}
