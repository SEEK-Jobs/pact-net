using System;

namespace PactNet
{
    /// <summary>
    /// Pact
    /// </summary>
    public class Pact : IPactV2, IPactV3
    {
        /// <summary>
        /// Consumer name
        /// </summary>
        public string Consumer { get; }

        /// <summary>
        /// Provider name
        /// </summary>
        public string Provider { get; }

        /// <summary>
        /// Pact config
        /// </summary>
        public PactConfig Config { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="Pact"/> class.
        /// </summary>
        /// <param name="consumer">Name of the consumer</param>
        /// <param name="provider">Name of the provider</param>
        private Pact(string consumer, string provider)
            : this(
                consumer,
                provider,
                new PactConfig())
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Pact"/> class.
        /// </summary>
        /// <param name="consumer">Name of the consumer</param>
        /// <param name="provider">Name of the provider</param>
        /// <param name="config">Pact config</param>
        private Pact(string consumer, string provider, PactConfig config)
        {
            if (string.IsNullOrWhiteSpace(consumer))
            {
                throw new ArgumentException("Please provide a valid consumer name", nameof(consumer));
            }

            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new ArgumentException("Please provide a valid provider name", nameof(provider));
            }

            Consumer = consumer;
            Provider = provider;
            Config = config;
        }

        /// <summary>
        /// Create a new v2 pact
        /// </summary>
        /// <param name="consumer">Name of the consumer</param>
        /// <param name="provider">Name of the provider</param>
        /// <returns>v2 Pact</returns>
        public static IPactV2 V2(string consumer, string provider)
        {
            return new Pact(consumer, provider);
        }

        /// <summary>
        /// Create a new v2 pact
        /// </summary>
        /// <param name="consumer">Name of the consumer</param>
        /// <param name="provider">Name of the provider</param>
        /// <param name="config">Pact config</param>
        /// <returns>v2 Pact</returns>
        public static IPactV2 V2(string consumer, string provider, PactConfig config)
        {
            return new Pact(consumer, provider, config);
        }

        /// <summary>
        /// Create a new v3 pact
        /// </summary>
        /// <param name="consumer">Name of the consumer</param>
        /// <param name="provider">Name of the provider</param>
        /// <returns>v2 Pact</returns>
        public static IPactV3 V3(string consumer, string provider)
        {
            return new Pact(consumer, provider);
        }

        /// <summary>
        /// Create a new v3 pact
        /// </summary>
        /// <param name="consumer">Name of the consumer</param>
        /// <param name="provider">Name of the provider</param>
        /// <param name="config">Pact config</param>
        /// <returns>v3 Pact</returns>
        public static IPactV3 V3(string consumer, string provider, PactConfig config)
        {
            return new Pact(consumer, provider, config);
        }
    }
}
