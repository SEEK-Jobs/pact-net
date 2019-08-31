using System;
using System.Collections.Generic;
using System.Linq;
using PactNet.PactMessage.Models;

namespace PactNet.PactVerification
{
    public class MessageInvoker : IMessageInvoker
    {
        private readonly IDictionary<string, Action> _providerStates;
        private readonly IDictionary<string, Func<object>> _messagePublishers;

        public MessageInvoker(IDictionary<string, Action> providerStates, IDictionary<string, Func<object>> messagePublishers)
        {
            _providerStates = providerStates;
            _messagePublishers = messagePublishers;
        }

        public object Invoke(MessagePactDescription description)
        {
            if (description?.ProviderStates != null && description.ProviderStates.Any(x => x.Name != null))
            {
                SetUpProviderStates(description.ProviderStates);
            }
            return GetMessageInteraction(description);
        }

        private void SetUpProviderStates(IEnumerable<ProviderState> messageDescriptionProviderStates)
        {
            foreach (var providerState in messageDescriptionProviderStates)
            {
                if (!_providerStates.TryGetValue(providerState.Name, out var actualAction))
                {
                    throw new PactFailureException($"The provider state that was supplyed: {providerState.Name} could not be found.");
                }

                actualAction();
            }
        }

        private object GetMessageInteraction(MessagePactDescription messageDescription)
        {
            if (!_messagePublishers.TryGetValue(messageDescription.Description, out var actualPublisher))
            {
                throw new PactFailureException($"The publisher action for this message description was not supplyed: {messageDescription.Description}");
            }

            return actualPublisher();
        }
    }
}
