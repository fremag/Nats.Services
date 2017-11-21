using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Nats.Services.Core
{
	public class NatsServiceSerializer<T>
	{
		DataContractJsonSerializer serializer;
		Dictionary<Type, DataContractJsonSerializer> returnTypeSerializers = new Dictionary<Type, DataContractJsonSerializer>();

		public NatsServiceSerializer()
		{
			HashSet<Type> types = new HashSet<Type>();
			foreach (var methInfo in typeof(T).GetAllMethodInfos())
			{
				foreach (var paramInfo in methInfo.GetParameters())
				{
					types.Add(paramInfo.ParameterType);
				}
				if (methInfo.ReturnType != typeof(void) && ! returnTypeSerializers.ContainsKey(methInfo.ReturnType))
				{
					returnTypeSerializers.Add(methInfo.ReturnType, new DataContractJsonSerializer(methInfo.ReturnType));
				}
			}

			foreach (var evtInfo in typeof(T).GetAllEventInfos())
			{
				var methInfo = evtInfo.EventHandlerType.GetMethod(nameof(EventHandler.Invoke));
				foreach (var paramInfo in methInfo.GetParameters())
				{
					types.Add(paramInfo.ParameterType);
				}
			}

			serializer = new DataContractJsonSerializer(typeof(List<KeyValuePair<string, object>>), types);
		}

		public List<KeyValuePair<string, object>> DeserializeMethodArguments(byte[] buffer)
		{
			return DeserializeObject(serializer, buffer) as List<KeyValuePair<string, object>>;
		}
		public byte[] SerializeMethodArguments(object obj)
		{
			return SerializeObject(serializer, obj);
		}

		public object DeserializeReturnObject(Type type, byte[] buffer)
		{
			DataContractJsonSerializer returnTypeSerializer = returnTypeSerializers[type];
			return DeserializeObject(returnTypeSerializer, buffer);
		}
		public byte[] SerializeReturnObject(object obj)
		{
			DataContractJsonSerializer returnTypeSerializer = returnTypeSerializers[obj.GetType()];
			return SerializeObject(returnTypeSerializer, obj);
		}

		public static object DeserializeObject(DataContractJsonSerializer serializer, byte[] buffer)
		{
			using (var stream = new MemoryStream(buffer))
			{
				var obj = serializer.ReadObject(stream);
				return obj;
			}
		}

		public static byte[] SerializeObject(DataContractJsonSerializer serializer, object obj)
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
