using System;
using PactNet.Matchers;
using PactNet.Matchers.Type;
using PactNet.PactMessage;
using PactNet.PactMessage.Models;
using Xunit;

namespace PactNet.Tests.IntegrationTests
{
    public class PactMessageIntegrationTests : IClassFixture<IntegrationTestsMyConsumerPact>
    {
        private readonly IMessagePactBuilder _messagePactBuilder;
        private readonly IMessagePact _messagePact;

        public PactMessageIntegrationTests(IntegrationTestsMyConsumerPact integrationTestsMyConsumer)
        {
            _messagePactBuilder = integrationTestsMyConsumer.MessagePactBuilder;
            _messagePact = integrationTestsMyConsumer.MessagePact;
        }

        [Fact]
        public void Build_NoMessages_VerificationSucceeds()
        {
            _messagePact.VerifyConsumer<string>(MessageHandler);

            _messagePactBuilder.Build();
        }

        [Fact]
        public void Build_SubscriberCannotHandleMessage_PactFailureExceptionIsThrown()
        {
            Assert.Throws<PactFailureException>(() => _messagePact.
                ExpectedToReceive("A message that the subscriber can't handle")
                .With(new Message
                {
                    Contents = new
                    {
                        name = "Test"
                    }
                })
                .VerifyConsumer<string>(FailedMessageHandler));
        }

        private static void FailedMessageHandler(string obj)
        {
            throw new InvalidOperationException();
        }

        private static void MessageHandler(string message)
        {
            Assert.True(message != null);
        }
    }
}
