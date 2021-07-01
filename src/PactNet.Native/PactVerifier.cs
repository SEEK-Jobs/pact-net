using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PactNet.Native
{
    /// <summary>
    /// Pact verifier
    /// </summary>
    public class PactVerifier : IPactVerifier
    {
        private readonly IVerifierProvider verifier;
        private readonly PactVerifierConfig config;
        protected internal readonly IList<string> verifierArgs = new List<string>();

        /// <summary>
        /// Initialises a new instance of the <see cref="PactVerifier"/> class.
        /// </summary>
        public PactVerifier() : this(new PactVerifierConfig())
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="PactVerifier"/> class.
        /// </summary>
        /// <param name="config">Verifier configuration</param>
        public PactVerifier(PactVerifierConfig config) : this(new NativePactVerifier(), config)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="PactVerifier"/> class.
        /// </summary>
        /// <param name="verifier">Verifier provider</param>
        /// <param name="config">Config</param>
        internal PactVerifier(IVerifierProvider verifier, PactVerifierConfig config)
        {
            this.verifier = verifier;
            this.config = config;
        }

        /// <summary>
        /// Set the provider details
        /// </summary>
        /// <param name="providerName">Name of the provider</param>
        /// <param name="pactUri">URI of the running service</param>
        /// <returns>Fluent builder</returns>
        public IPactVerifier ServiceProvider(string providerName, Uri pactUri)
        {
            verifierArgs.Add("--provider-name");
            verifierArgs.Add(providerName);

            verifierArgs.Add("--hostname");
            verifierArgs.Add(pactUri.Host);

            verifierArgs.Add("--port");
            verifierArgs.Add(pactUri.Port.ToString());

            SetBasePath(pactUri);

            return this;
        }

        /// <summary>
        /// Set the base path
        /// </summary>
        /// <param name="pactUri"></param>
        protected internal virtual void SetBasePath(Uri pactUri)
        {
            if (pactUri.AbsolutePath != "/")
            {
                verifierArgs.Add("--base-path");
                verifierArgs.Add(pactUri.AbsolutePath);
            }
        }

        /// <summary>
        /// Set the consumer name
        /// </summary>
        /// <param name="consumerName">Consumer name</param>
        /// <returns>Fluent builder</returns>
        public IPactVerifier HonoursPactWith(string consumerName)
        {
            verifierArgs.Add("--filter-consumer");
            verifierArgs.Add(consumerName);
            return this;
        }

        /// <summary>
        /// Verify a pact file directly
        /// </summary>
        /// <param name="pactFile">Pact file path</param>
        /// <returns>Fluent builder</returns>
        public IPactVerifier FromPactFile(FileInfo pactFile)
        {
            verifierArgs.Add("--file");
            verifierArgs.Add(pactFile.FullName);
            return this;
        }

        /// <summary>
        /// Verify a pact from a URI
        /// </summary>
        /// <param name="pactUri">Pact file URI</param>
        /// <returns>Fluent builder</returns>
        public IPactVerifier FromPactUri(Uri pactUri)
        {
            verifierArgs.Add("--url");
            verifierArgs.Add(pactUri.ToString());
            return this;
        }

        /// <summary>
        /// Use the pact broker to retrieve pact files
        /// </summary>
        /// <param name="brokerBaseUri">Base URI for the broker</param>
        /// <param name="uriOptions">Options for calling the pact broker</param>
        /// <param name="enablePending">Enable pending pacts?</param>
        /// <param name="consumerVersionTags">Consumer tag versions to retrieve</param>
        /// <param name="includeWipPactsSince">Include WIP pacts since the given filter</param>
        /// <returns>Fluent builder</returns>
        public IPactVerifier FromPactBroker(Uri brokerBaseUri,
                                        PactUriOptions uriOptions = null,
                                        bool enablePending = false,
                                        IEnumerable<string> consumerVersionTags = null,
                                        string includeWipPactsSince = null)
        {
            verifierArgs.Add("--broker-url");
            verifierArgs.Add(brokerBaseUri.ToString());

            if (uriOptions != null)
            {
                if (!string.IsNullOrWhiteSpace(uriOptions.Username))
                {
                    verifierArgs.Add($"--user");
                    verifierArgs.Add(uriOptions.Username);
                }

                if (!string.IsNullOrWhiteSpace(uriOptions.Password))
                {
                    verifierArgs.Add("--password");
                    verifierArgs.Add(uriOptions.Password);
                }

                if (!string.IsNullOrWhiteSpace(uriOptions.Token))
                {
                    verifierArgs.Add("--token");
                    verifierArgs.Add(uriOptions.Token);
                }
            }

            if (enablePending)
            {
                verifierArgs.Add("--enable-pending");
            }

            if (consumerVersionTags != null && consumerVersionTags.Any())
            {
                string versions = string.Join(",", consumerVersionTags);
                verifierArgs.Add("--consumer-version-tags");
                verifierArgs.Add(versions);
            }

            if (!string.IsNullOrWhiteSpace(includeWipPactsSince))
            {
                verifierArgs.Add("--include-wip-pacts-since");
                verifierArgs.Add(includeWipPactsSince);
            }

            return this;
        }

        /// <summary>
        /// Set up the provider state setup URI so the service can configure states
        /// </summary>
        /// <param name="providerStateUri">Provider state URI</param>
        /// <returns>Fluent builder</returns>
        public IPactVerifier WithProviderStateUrl(Uri providerStateUri)
        {
            verifierArgs.Add("--state-change-url");
            verifierArgs.Add(providerStateUri.ToString());
            return this;
        }

        /// <summary>
        /// Filter the interactions to only those matching the given description and/or provider state
        /// </summary>
        /// <param name="description">Interaction description. All interactions are verified if this is null</param>
        /// <param name="providerState">Provider state description. All provider states are verified if this is null</param>
        /// <returns>Fluent builder</returns>
        public IPactVerifier WithFilter(string description = null, string providerState = null)
        {
            // TODO: Allow env vars to specify description and provider state filters like the old version did
            if (!string.IsNullOrWhiteSpace(description))
            {
                verifierArgs.Add("--filter-description");
                verifierArgs.Add(description);
            }

            if (!string.IsNullOrWhiteSpace(providerState))
            {
                verifierArgs.Add("--filter-state");
                verifierArgs.Add(providerState);
            }

            return this;
        }

        /// <summary>
        /// Publish results to the pact broker
        /// </summary>
        /// <param name="providerVersion">Provider version</param>
        /// <param name="providerTags">Optional tags to add to the verification</param>
        /// <returns>Fluent builder</returns>
        public IPactVerifier WithPublishedResults(string providerVersion, IEnumerable<string> providerTags = null)
        {
            if (string.IsNullOrWhiteSpace(providerVersion))
            {
                throw new ArgumentException("Can't publish verification results without the provider version being set");
            }

            verifierArgs.Add("--publish");
            verifierArgs.Add("--provider-version");
            verifierArgs.Add(providerVersion);

            if (providerTags != null && providerTags.Any())
            {
                string tags = string.Join(",", providerTags);
                verifierArgs.Add("--provider-tags");
                verifierArgs.Add(tags);
            }

            return this;
        }

        /// <summary>
        /// Verify provider interactions
        /// </summary>
        public void Verify()
        {
            // TODO: make verifier log level configurable
            verifierArgs.Add("--loglevel");
            verifierArgs.Add("trace");

            string formatted = string.Join(Environment.NewLine, verifierArgs);

            config.WriteLine("Invoking the pact verifier with args:");
            config.WriteLine(formatted);

            verifier.Verify(formatted);
        }
    }
}
