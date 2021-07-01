using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using PactNet.Native.Exceptions;
using PactNet.Native.Models;

namespace PactNet.Native
{
    /// <summary>
    /// Mock request builder
    /// </summary>
    public class NativePactMessageBuilder : IPactMessageBuilderV3
    {
        private readonly IMessageMockServer server;
        private readonly MessagePactHandle pact;
        private readonly PactConfig config;
        private readonly MessageHandle message;
        private readonly JsonSerializerSettings defaultSettings;

        /// <summary>
        /// Initialises a new instance of the <see cref="NativePactMessageBuilder"/> class.
        /// </summary>
        /// <param name="server">Mock server</param>
        /// <param name="pact">the pact handle</param>
        /// <param name="config">the pact configuration</param>
        /// <param name="defaultSettings">Default JSON serializer settings</param>
        internal NativePactMessageBuilder(IMessageMockServer server, MessagePactHandle pact, PactConfig config, JsonSerializerSettings defaultSettings = null)
        {
            this.pact = pact;
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            message = server.NewMessage(pact, "default message");
            this.defaultSettings = defaultSettings;
        }

        #region IPactMessageBuilderV3 explicit implementation

        /// <inheritdoc cref="IPactMessageBuilderV3"/>
        IPactMessageBuilderV3 IPactMessageBuilderV3.ExpectsToReceive(string description)
            => ExpectsToReceive(description);

        /// <inheritdoc cref="IPactMessageBuilderV3"/>
        IPactMessageBuilderV3 IPactMessageBuilderV3.Given(string providerState)
            => Given(providerState);

        /// <inheritdoc cref="IPactMessageBuilderV3"/>
        IPactMessageBuilderV3 IPactMessageBuilderV3.Given(string providerState, IDictionary<string, string> parameters)
            => Given(providerState, parameters);

        /// <inheritdoc cref="IPactMessageBuilderV3"/>
        IPactMessageBuilderV3 IPactMessageBuilderV3.WithMetadata(string key, string value)
            => WithMetadata(key, value);

        /// <inheritdoc cref="IPactMessageBuilderV3"/>
        IPactMessageBuilderV3 IPactMessageBuilderV3.WithContent(dynamic content)
            => WithContent(content);

        /// <inheritdoc cref="IPactMessageBuilderV3"/>
        void IPactMessageBuilderV3.Build()
            => Build();

        /// <inheritdoc cref="IPactMessageBuilderV3"/>
        void IPactMessageBuilderV3.Verify<T>(Action<T> handler)
            => Verify(handler);

        #endregion

        #region Internal Methods

        /// <summary>
        /// Add a new message to the pact
        /// </summary>
        /// <param name="description">Message description</param>
        /// <returns>Fluent builder</returns>
        internal IPactMessageBuilderV3 ExpectsToReceive(string description)
        {
            server.MessageExpectsToReceive(message, description);

            return this;
        }

        /// <summary>
        /// Add a provider state
        /// </summary>
        /// <param name="providerState">Provider state description</param>
        /// <returns>Fluent builder</returns>
        internal IPactMessageBuilderV3 Given(string providerState)
        {
            server.MessageGiven(message, providerState);

            return this;
        }

        /// <summary>
        /// Add a provider state with one or more parameters
        /// </summary>
        /// <param name="providerState">Provider state description</param>
        /// <param name="parameters">Provider state parameters</param>
        /// <returns>Fluent builder</returns>
        internal IPactMessageBuilderV3 Given(string providerState, IDictionary<string, string> parameters)
        {
            foreach (var param in parameters)
            {
                server.MessageGivenWithParam(message, providerState, param.Key, param.Value);
            }

            return this;
        }

        /// <summary>
        /// Set the metadata
        /// </summary>
        /// <param name="key">key of the metadata</param>
        /// <param name="value">value of the metadata</param>
        /// <returns>Fluent builder</returns>
        internal IPactMessageBuilderV3 WithMetadata(string key, string value)
        {
            server.MessageWithMetadata(message, key, value);

            return this;
        }

        /// <summary>
        /// Set the content
        /// </summary>
        /// <param name="content">Dynamic content</param>
        /// <returns>Fluent builder</returns>
        internal IPactMessageBuilderV3 WithContent(dynamic content)
        {
            server.MessageWithContents(message, "application/json", JsonConvert.SerializeObject(content), 100);

            return this;
        }

        /// <summary>
        /// Build the pact file
        /// </summary>
        internal void Build()
        {
            server.WriteMessagePactFile(pact, config.PactDir, true);
        }

        /// <summary>
        /// Verify a message is read and handled correctly
        /// </summary>
        /// <param name="handler">The method using the message</param>
        internal void Verify<T>(Action<T> handler)
        {
            try
            {
                var content = JsonConvert.DeserializeObject<NativeMessage>(server.MessageReify(message));

                var messageReified = JsonConvert.DeserializeObject<T>(content.Contents.ToString(), defaultSettings);

                handler(messageReified);
            }
            catch (Exception e)
            {
                throw new PactMessageConsumerVerificationException(
                    $"The message {message} could not be verified by the consumer handler", e);
            }
        }

        #endregion Internal Methods
    }
}
