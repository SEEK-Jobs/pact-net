using System;
using PactNet.Core;
using static System.String;
using System.Collections.Generic;

namespace PactNet
{
    public class PactVerifier : IPactVerifier
    {
        private readonly PactVerifierConfig _config;
        private readonly Func<PactVerifierHostConfig, IPactCoreHost> _pactVerifierHostFactory;

        public Uri ProviderStateSetupUri { get; private set; }
        public Uri ServiceBaseUri { get; private set; }
        public string ConsumerName { get; private set; }
        public string ProviderName { get; private set; }
        public string PactFileUri { get; private set; }
        public PactUriOptions PactUriOptions { get; private set; }

        internal PactVerifier(Func<PactVerifierHostConfig, IPactCoreHost> pactVerifierHostFactory, PactVerifierConfig config)
        {
            _pactVerifierHostFactory = pactVerifierHostFactory;
            _config = config;

            if (config.PublishVerificationResults && IsNullOrEmpty(config.ProviderVersion))
            {
                throw new ArgumentException($"config.{nameof(config.ProviderVersion)} is required when config.{nameof(config.PublishVerificationResults)} is true.");
            }
        }

        public PactVerifier(PactVerifierConfig config) :
            this(
            hostConfig => new PactCoreHost<PactVerifierHostConfig>(hostConfig),
            config ?? new PactVerifierConfig())
        {
        }

        public IPactVerifier ProviderState(string providerStateSetupUri)
        {
            if (IsNullOrEmpty(providerStateSetupUri))
            {
                throw new ArgumentException("Please supply a non null or empty providerStateSetupUri");
            }

            ProviderStateSetupUri = new Uri(providerStateSetupUri);

            return this;
        }

        public IPactVerifier ServiceProvider(string providerName, string baseUri)
        {
            if (IsNullOrEmpty(providerName))
            {
                throw new ArgumentException("Please supply a non null or empty providerName");
            }

            if (!IsNullOrEmpty(ProviderName))
            {
                throw new ArgumentException("ProviderName has already been supplied, please instantiate a new PactVerifier if you want to perform verification for a different provider");
            }

            if (IsNullOrEmpty(baseUri))
            {
                throw new ArgumentException("Please supply a non null or empty baseUri");
            }

            ProviderName = providerName;
            ServiceBaseUri = new Uri(baseUri);

            return this;
        }

        public IPactVerifier HonoursPactWith(string consumerName)
        {
            if (IsNullOrEmpty(consumerName))
            {
                throw new ArgumentException("Please supply a non null or empty consumerName");
            }

            if (!IsNullOrEmpty(ConsumerName))
            {
                throw new ArgumentException("ConsumerName has already been supplied, please instantiate a new PactVerifier if you want to perform verification for a different consumer");
            }

            ConsumerName = consumerName;

            return this;
        }

        public IPactVerifier PactUri(string fileUri, PactUriOptions options = null)
        {
            if (IsNullOrEmpty(fileUri))
            {
                throw new ArgumentException("Please supply a non null or empty fileUri");
            }

            PactFileUri = fileUri;
            PactUriOptions = options;

            return this;
        }

        public void Verify(string description = null, string providerState = null)
        {
            if (ServiceBaseUri == null)
            {
                throw new InvalidOperationException(
                    "baseUri has not been set, please supply a service baseUri using the ServiceProvider method.");
            }

            if (PactFileUri == null)
            {
                throw new InvalidOperationException(
                    "PactFileUri has not been set, please supply a uri using the PactUri method.");
            }

            IDictionary<string, string> env = null;
            if (!IsNullOrEmpty(description) || !IsNullOrEmpty(providerState))
            {
                env = new Dictionary<string, string>
                {
                    { "PACT_DESCRIPTION", description },
                    { "PACT_PROVIDER_STATE", providerState }
                };
            }

            var pactVerifier = _pactVerifierHostFactory(
                new PactVerifierHostConfig(ServiceBaseUri, PactFileUri, PactUriOptions, ProviderStateSetupUri, _config, env));
            pactVerifier.Start();
            pactVerifier.Stop();
        }
    }
}
