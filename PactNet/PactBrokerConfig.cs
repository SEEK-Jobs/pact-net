using System.Collections.Generic;

namespace PactNet
{
    internal class PactBrokerConfig
    {
        public string ProviderName { get; }
        public string BrokerBaseUri { get; }
        public bool EnablePending { get; }
        public IEnumerable<string> ConsumerVersionTags { get; }
        public IEnumerable<string> ProviderVersionTags { get; }
        public IEnumerable<VersionTagSelector> ConsumerVersionSelectors { get; }
        public string IncludeWipPactsSince { get; }

        public PactBrokerConfig(string providerName, string brokerBaseUri, bool enablePending,
            IEnumerable<string> consumerVersionTags, IEnumerable<string> providerVersionTags, 
            IEnumerable<VersionTagSelector> consumerVersionSelectors, string includeWipPactsSince)
        {
            ProviderName = providerName;
            BrokerBaseUri = brokerBaseUri;
            EnablePending = enablePending;
            ConsumerVersionTags = consumerVersionTags;
            ProviderVersionTags = providerVersionTags;
            ConsumerVersionSelectors = consumerVersionSelectors;
            IncludeWipPactsSince = includeWipPactsSince;
        }
    }
}