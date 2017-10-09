using Nats.Services.Core;
using NFluent;
using System;
using System.Linq;
using Xunit;

namespace Nats.Services.Tests
{
    public class TypeHelpersTests
    {

        [Theory]
        [InlineData(typeof(IDummyInterface), 12)]
        [InlineData(typeof(IDummyInterfaceWithProperties), 2)]
        [InlineData(typeof(IDummyInterfaceWithEvent), 3)]
        [InlineData(typeof(IDummyInterfaceWithThreeEvents), 6)]
        public void TestGetAllMethodInfos(Type type, int expectedNbMethods)
        {
            var methods = type.GetAllMethodInfos().ToArray();
            Check.That(methods).IsNotNull();
            Check.That(methods.Length).IsEqualTo(expectedNbMethods);
        }

        [Theory]
        [InlineData(typeof(IDummyInterface), 4)]
        [InlineData(typeof(IDummyInterfaceWithProperties), 0)]
        [InlineData(typeof(IDummyInterfaceWithEvent), 1)]
        [InlineData(typeof(IDummyInterfaceWithThreeEvents), 3)]
        public void TestGetAllEventInfos(Type type, int expectedNbEvents)
        {
            var events = type.GetAllEventInfos().ToArray();
            Check.That(events).IsNotNull();
            Check.That(events.Length).IsEqualTo(expectedNbEvents);
        }
    }



    public interface IDummyInterface : IDummyInterfaceWithProperties, IDummyInterfaceWithEvent, IDummyInterfaceWithThreeEvents
    {
        void Print();
    }

    public interface IDummyInterfaceWithProperties
    {
        void SayHello();
        int Id { get; }
    }

    public interface IDummyInterfaceWithEvent
    {
        event Action<int> SomeEvent;
        void DoSomething();
    }

    public interface IDummyInterfaceWithThreeEvents
    {
        event Action<int> EventNumber1;
        event Action<int> EventNumber2;
        event Action<int> EventNumber3;
    }
}
