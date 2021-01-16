using System;
using System.Collections.Generic;

namespace PactNet
{
    public interface IPactVerifier
    {
        IPactVerifier ProviderState(string providerStateSetupUri);
        IPactVerifier ServiceProvider(string providerName, string baseUri);
        IPactVerifier HonoursPactWith(string consumerName);
        IPactVerifier PactUri(string fileUri, PactUriOptions options = null, IEnumerable<string> providerVersionTags = null);
        IPactVerifier PactBroker(string brokerBaseUri, PactUriOptions uriOptions = null, bool enablePending = false,
            IEnumerable<string> consumerVersionTags = null, IEnumerable<string> providerVersionTags = null, IEnumerable<VersionTagSelector> consumerVersionSelectors = null, string includeWipPactsSince = null);
        void Verify(string description = null, string providerState = null);
    }
}