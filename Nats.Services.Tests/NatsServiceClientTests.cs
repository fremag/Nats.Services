﻿using Nats.Services.Core;
using NATS.Client;
using NFluent;
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
        NatsServiceSerializer<IDummyService> serializer;

        public NatsServiceClientTests()
        {
            connection = Substitute.For<IConnection>();
            NatsServiceFactory factory = new NatsServiceFactory(connection, agentName);
            service = factory.BuildServiceClient<IDummyService>();
            serializer = new NatsServiceSerializer<IDummyService>();
        }

        /*
         * Check that when service client is created, no subject is subscribed yet
         */
        [Fact]
        public void CreationTest()
        {
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<string>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any< EventHandler < MsgHandlerEventArgs>>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<EventHandler<MsgHandlerEventArgs>>());
        }

        /*
         * Subscribe to an event and check he connection listens to the correct subject
         */
        [Fact]
        public void EventSubscriptionTest()
        {
            service.MessageSent += Console.WriteLine;

            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<string>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<EventHandler<MsgHandlerEventArgs>>());
            connection.Received().SubscribeAsync("TEST_AGENT.IDummyService.MessageSent", Arg.Any<EventHandler<MsgHandlerEventArgs>>());
        }

        /*
         * Check that a message is sent/published when a method is called
         * Check the payload contains the serialized method arguments 
         */
        [Fact]
        public void MethodCallTest()
        {
            string msg = "Some message";
            service.Log(msg);
            List<KeyValuePair<string, object>> args = new List<KeyValuePair<string, object>>();
            args.Add(new KeyValuePair<string, object>("message", msg));
        

            byte[] payload = serializer.SerializeMethodArguments(args);
            connection.Received().Publish("TEST_AGENT.IDummyService.Log", Arg.Is<byte[]>(bytes => payload.SequenceEqual(bytes)));
        }

        /*
         * Check the method call result is correct
         */ 
        [Fact]
        public void MethodCallWithResultTest()
        {
            byte[] payload = serializer.SerializeReturnObject(42);
            Msg msg = new Msg("nothing", payload);
            connection.Request(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<int>()).Returns(msg);

            string name = "John";
            int id = service.GetId(name);
            Check.That(id).IsEqualTo(42);
        }
    }
}
