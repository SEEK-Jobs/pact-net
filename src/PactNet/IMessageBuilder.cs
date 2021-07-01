using System;
using System.Collections.Generic;

namespace PactNet
{
    /// <summary>
    /// Build up a mock message for a v3 pact
    /// </summary>
    public interface IPactMessageBuilderV3
    {
        /// <summary>
        /// Add a new message to the pact
        /// </summary>
        /// <param name="description">Message description</param>
        /// <returns>Fluent builder</returns>
        IPactMessageBuilderV3 ExpectsToReceive(string description);

        /// <summary>
        /// Add a provider state
        /// </summary>
        /// <param name="providerState">Provider state description</param>
        /// <returns>Fluent builder</returns>
        IPactMessageBuilderV3 Given(string providerState);

        /// <summary>
        /// Add a provider state with one or more parameters
        /// </summary>
        /// <param name="providerState">Provider state description</param>
        /// <param name="parameters">Provider state parameters</param>
        /// <returns>Fluent builder</returns>
        IPactMessageBuilderV3 Given(string providerState, IDictionary<string, string> parameters);

        /// <summary>
        /// Set the metadata
        /// </summary>
        /// <param name="key">the metadata key</param>
        /// <param name="value">the metadata value</param>
        /// <returns>Fluent builder</returns>
        IPactMessageBuilderV3 WithMetadata(string key, string value);

        /// <summary>
        /// Set the content
        /// </summary>
        /// <param name="content">Dynamic content</param>
        /// <returns>Fluent builder</returns>
        IPactMessageBuilderV3 WithContent(dynamic content);

        /// <summary>
        /// Build the pact file
        /// </summary>
        void Build();

        /// <summary>
        /// Verify a message is read and handled correctly
        /// </summary>
        /// <param name="handler">The method using the message</param>
        void Verify<T>(Action<T> handler);
    }
}
