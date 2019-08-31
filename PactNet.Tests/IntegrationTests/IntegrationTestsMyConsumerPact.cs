using System.Collections.Generic;
using PactNet.Infrastructure.Outputters;
using PactNet.PactMessage;

namespace PactNet.Tests.IntegrationTests
{
    public class IntegrationTestsMyConsumerPact
    {
        public IMessagePactBuilder MessagePactBuilder { get; }
        public IMessagePact MessagePact { get; }

        public IntegrationTestsMyConsumerPact()
        {
            var pactConfig = new PactConfig
            {
                SpecificationVersion = "3.0.0",
                Outputters = new List<IOutput>
                {
                    new ConsoleOutput()
                }
            };
            MessagePactBuilder = new MessagePactBuilder(pactConfig)
                .HasPactWith("Integration Tests")
                .ServiceConsumer("My Consumer");

            MessagePact = MessagePactBuilder.InitializePactMessage();
        }
    }
}
