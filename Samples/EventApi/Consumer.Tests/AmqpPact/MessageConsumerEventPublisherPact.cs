using System;
using System.IO;
using PactNet;
using PactNet.PactMessage;

namespace Consumer.Tests.AmqpPact
{
    public class MessageConsumerEventPublisherPact : IDisposable
    {
        public IMessagePactBuilder MessagePactBuilder { get; }
        public IMessagePact MessagePact { get; }

        public MessageConsumerEventPublisherPact()
        {
            MessagePactBuilder = new MessagePactBuilder(new PactConfig
            {
                SpecificationVersion = "2.0.0",
                LogDir = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}logs{Path.DirectorySeparatorChar}",
                PactDir = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}"
            })
                .ServiceConsumer("Event API Message Consumer")
                .HasPactWith("Event API");

            MessagePact = MessagePactBuilder.InitializePactMessage();
        }

        public void Dispose()
        {
            MessagePactBuilder.Build();
        }
    }
}
