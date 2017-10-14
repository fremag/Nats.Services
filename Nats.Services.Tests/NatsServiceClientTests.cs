using Nats.Services.Core;
using NATS.Client;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Nats.Services.Tests
{
    public class NatsServiceClientTests
    {
        public interface IDummyService
        {
            event Action<string> MessageSent;
            int GetId(string name);
            void Log(string message);
        }

        const string agentName = "TEST_AGENT";
        IConnection connection;
        IDummyService service;

        public NatsServiceClientTests()
        {
            connection = Substitute.For<IConnection>();
            NatsServiceFactory factory = new NatsServiceFactory(connection, agentName);
            service = factory.BuildServiceClient<IDummyService>();
        }

        [Fact]
        public void CreationTest()
        {
            // When service client is created, no subject is subscribed yet
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<string>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any< EventHandler < MsgHandlerEventArgs>>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<EventHandler<MsgHandlerEventArgs>>());
        }

        [Fact]
        public void EventSubscriptionTest()
        {
            service.MessageSent += Console.WriteLine;

            // When service client is created, no subject is subscribed yet
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<string>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<EventHandler<MsgHandlerEventArgs>>());
            connection.Received().SubscribeAsync("TEST_AGENT.IDummyService.MessageSent", Arg.Any<EventHandler<MsgHandlerEventArgs>>());
        }

        [Fact]
        public void MethodCallTest()
        {
            string msg = "Some message";
            service.Log(msg);
            NatsServiceSerializer<IDummyService> serializer = new NatsServiceSerializer<IDummyService>();
            List<KeyValuePair<string, object>> args = new List<KeyValuePair<string, object>>();
            args.Add(new KeyValuePair<string, object>("message", msg));
        

            byte[] payload = serializer.SerializeMethodArguments(args);
            // When service client is created, no subject is subscribed yet
            connection.Received().Publish("TEST_AGENT.IDummyService.Log", Arg.Is<byte[]>(bytes => payload.SequenceEqual(bytes)));
        }
    }
}
