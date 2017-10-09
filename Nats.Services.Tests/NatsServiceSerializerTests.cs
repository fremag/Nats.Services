using Nats.Services.Core;
using NFluent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit;

namespace Nats.Services.Tests
{
    public class NatsServiceSerializerTests
    {
        NatsServiceSerializer<IDummyService> serializer;
        List<KeyValuePair<string, object>> args;
        AnotherDummyClass anotherArg;

        public interface IDummyService
        {
            void Log(List<DummyClass> obj);
            AnotherDummyClass Create(double x, int n);
        }

        public NatsServiceSerializerTests()
        {
            serializer = new NatsServiceSerializer<IDummyService>();
            args = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("dummy1", new DummyClass { Name = "aaaaa", Id = 11 }),
                new KeyValuePair<string, object>("dummy2", new DummyClass { Name = "bbbbb", Id = 22 })
            };

            anotherArg = new AnotherDummyClass { X = 1.25, N = 11 };
        }


        [Fact]
        public void SerializeMethodArgumentsTest()
        {
            byte[] payload = serializer.SerializeMethodArguments(args);
            string json = serializer.ToString(payload);
            string jsonExpected = "[{\"key\":\"dummy1\",\"value\":{\"__type\":\"NatsServiceSerializerTests.DummyClass:#Nats.Services.Tests\",\"Id\":11,\"Name\":\"aaaaa\"}},{\"key\":\"dummy2\",\"value\":{\"__type\":\"NatsServiceSerializerTests.DummyClass:#Nats.Services.Tests\",\"Id\":22,\"Name\":\"bbbbb\"}}]";
            Check.That(json).IsEqualTo(jsonExpected);

            var deserializedArgs = serializer.DeserializeMethodArguments(payload);
            Check.That(deserializedArgs.Count).IsEqualTo(2);
            Check.That(deserializedArgs.Count).IsEqualTo(args.Count);
            Check.That(deserializedArgs[0]).IsEqualTo(args[0]);
            Check.That(deserializedArgs[1]).IsEqualTo(args[1]);
        }

        [Fact]
        public void SerializeMethodArgumentsExceptionTest()
        {
            var serializer = new NatsServiceSerializer<IDummyService>();

            var args = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("dummy1", anotherArg)
            };

            Check.ThatCode(() => serializer.SerializeMethodArguments(args)).Throws<SerializationException>(); ;
        }

        [Fact]
        public void SerializeReturnObjectTest()
        {
            byte[] payload = serializer.SerializeReturnObject(anotherArg);
            string json = serializer.ToString(payload);
            string jsonExpected = "{\"N\":11,\"X\":1.25}";
            Check.That(json).IsEqualTo(jsonExpected);

            var deserializedObj = serializer.DeserializeReturnObject(anotherArg.GetType(), payload);
            Check.That(deserializedObj).IsEqualTo(anotherArg);
        }

        [Fact]
        public void SerializeReturnObjectExceptionTest()
        {
            Check.ThatCode(() => serializer.SerializeReturnObject(args)).Throws<KeyNotFoundException>(); ;
        }

        [DataContract]
        public class DummyClass
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public int Id { get; set; }

            public override bool Equals(object obj)
            {
                var c = obj as DummyClass;
                return c != null && Name == c.Name && Id == c.Id;
            }

            public override int GetHashCode()
            {
                var hashCode = 1460282102;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                hashCode = hashCode * -1521134295 + Id.GetHashCode();
                return hashCode;
            }
        }

        [DataContract]
        public class AnotherDummyClass
        {
            [DataMember]
            public double X { get; set; }
            [DataMember]
            public int N { get; set; }

            public override bool Equals(object obj)
            {
                var c = obj as AnotherDummyClass;
                return c != null && X == c.X && N == c.N;
            }

            public override int GetHashCode()
            {
                var hashCode = 1576192272;
                hashCode = hashCode * -1521134295 + X.GetHashCode();
                hashCode = hashCode * -1521134295 + N.GetHashCode();
                return hashCode;
            }
        }
    }
}
