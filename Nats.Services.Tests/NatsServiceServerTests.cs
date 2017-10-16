using Nats.Services.Core;
using NATS.Client;
using NFluent;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Nats.Services.Tests
{
    public class NatsServiceServerTests
    {
        public interface IDummyService
        {
            event Action<string> MessageSent;
            int GetId(string name);
            void Log(string message);
        }

        public class MyDummyServiceImpl : IDummyService
        {
            public event Action<string> MessageSent;

            public int GetId(string name) => 42;
            public void Log(string message) => MessageSent?.Invoke(message);
        }

        const string agentName = "TEST_AGENT";
        IConnection connection;
        IDummyService serviceImpl, dummyServiceImpl;
        NatsServiceSerializer<IDummyService> serializer;
        NatsServiceFactory factory;

        public NatsServiceServerTests()
        {
            connection = Substitute.For<IConnection>();
            factory = new NatsServiceFactory(connection, agentName);
            dummyServiceImpl = new MyDummyServiceImpl();
            serializer = new NatsServiceSerializer<IDummyService>();
            serviceImpl = factory.BuildServiceServer<IDummyService>(dummyServiceImpl);
        }

        /*
         * Check that when service client is service/server, all method subjects are subscribed
         */
        [Fact]
        public void CreationTest()
        {
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<string>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any< EventHandler < MsgHandlerEventArgs>>());
            connection.Received().SubscribeAsync("TEST_AGENT.IDummyService.GetId", Arg.Any<EventHandler<MsgHandlerEventArgs>>());
            connection.Received().SubscribeAsync("TEST_AGENT.IDummyService.Log", Arg.Any<EventHandler<MsgHandlerEventArgs>>());
            Check.That(connection.ReceivedCalls().Count()).IsEqualTo(2);
        }

        /*
         * Subscribe to an event and check he connection doesn't subscribe to any subject
         */
        [Fact]
        public void EventSubscriptionTest()
        {
            connection.ClearReceivedCalls();
            serviceImpl.MessageSent += Console.WriteLine;

            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<string>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<EventHandler<MsgHandlerEventArgs>>());
            connection.DidNotReceiveWithAnyArgs().SubscribeAsync(Arg.Any<string>(), Arg.Any<EventHandler<MsgHandlerEventArgs>>());
        }

        /*
         * 
         * 
         */
        [Fact]
        public void EventRaisedTest()
        {
            const string logMessage = "this is a log message";
            connection.ClearReceivedCalls();
            dummyServiceImpl.Log(logMessage);
            List<KeyValuePair<string, object>> args = new List<KeyValuePair<string, object>>();
            args.Add(new KeyValuePair<string, object>("obj", logMessage));
            byte[] payload = serializer.SerializeMethodArguments(args);
            connection.Received().Publish("TEST_AGENT.IDummyService.MessageSent", Arg.Is<byte[]>(bytes => payload.SequenceEqual(bytes)));
            Check.That(connection.ReceivedCalls().Count()).IsEqualTo(1);
        }

        /*
         * Check the method is called when a message is received
         */
        [Fact]
        public void MethodCallWithResultTest()
        {
            IDummyService dummyServiceMock = Substitute.For<IDummyService>();
            IAsyncSubscription asyncSub = Substitute.For<IAsyncSubscription>();
            EventHandler<MsgHandlerEventArgs> callBack = null;
            connection.SubscribeAsync("TEST_AGENT.IDummyService.GetId", Arg.Do<EventHandler<MsgHandlerEventArgs>>(arg => callBack = arg))
                .Returns(asyncSub);
            
            IDummyService serviceMock = factory.BuildServiceServer<IDummyService>(dummyServiceMock);
            if(callBack == null)
            {
                throw new NullReferenceException($"{nameof(callBack)} not initialized !");
            }

            var dummyId = 1234;
            serviceMock.GetId(Arg.Any<string>()).Returns(dummyId);

            List<KeyValuePair<string, object>> args = new List<KeyValuePair<string, object>>();
            args.Add(new KeyValuePair<string, object>("name", "Bob"));
            byte[] payload = serializer.SerializeMethodArguments(args);
            const string ReplySubject = "REQUEST_REPLY";
            Msg msg = new Msg("TEST_AGENT.IDummyService.GetId", ReplySubject, payload);

            var ev = new MsgHandlerEventArgs();
            var msgField = typeof(MsgHandlerEventArgs).GetField("msg", BindingFlags.Instance | BindingFlags.NonPublic);
            msgField.SetValue(ev, msg);

            callBack.Invoke(asyncSub, ev);

            byte[] replyPayload = serializer.SerializeReturnObject(dummyId);
            connection.Received().Publish(ReplySubject, Arg.Is<byte[]>(bytes => replyPayload.SequenceEqual(bytes)));
        }
    }
}
