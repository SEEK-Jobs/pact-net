using System;
using System.Collections.Generic;
using PactNet.PactMessage.Models;

namespace PactNet.PactMessage
{
    public interface IMessagePact 
    {
        IMessagePact ExpectedToReceive(string description);
        IMessagePact Given(IEnumerable<ProviderState> providerStates);
        IMessagePact With(Message message);
        void VerifyConsumer<T>(Action<T> messageHandler);
        IList<MessageInteraction> MessageInteractions { get; }
    }
}
