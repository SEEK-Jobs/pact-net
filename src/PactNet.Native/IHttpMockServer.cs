using System;

namespace PactNet.Native
{
    /// <summary>
    /// Mock server
    /// </summary>
    internal interface IHttpMockServer
    {
        /// <summary>
        /// Create the mock server for the given pact handle
        /// </summary>
        /// <param name="pact">Pact handle</param>
        /// <param name="addrStr">Host and port for the mock server</param>
        /// <param name="tls">Enable TLS</param>
        /// <returns>Mock server port</returns>
        /// <exception cref="InvalidOperationException">Failed to start mock server</exception>
        int CreateMockServerForPact(PactHandle pact, string addrStr, bool tls);

        /// <summary>
        /// Get a string representing the mismatches following interaction testing
        /// </summary>
        /// <param name="mockServerPort">Mock server port</param>
        /// <returns>Mismatch string</returns>
        string MockServerMismatches(int mockServerPort);

        /// <summary>
        /// Get a string representing the mock server logs following interaction testing
        /// </summary>
        /// <param name="mockServerPort">Mock server port</param>
        /// <returns>Log string</returns>
        string MockServerLogs(int mockServerPort);

        /// <summary>
        /// Clean up the mock server following consumer testing
        /// </summary>
        /// <param name="mockServerPort">Mock server port</param>
        /// <returns>Cleanup successful</returns>
        bool CleanupMockServer(int mockServerPort);

        /// <summary>
        /// Write the pact file to disk
        /// </summary>
        /// <param name="mockServerPort">Mock server port</param>
        /// <param name="directory">Directory of the pact file</param>
        /// <param name="overwrite">Overwrite the existing pact file?</param>
        /// <returns>Status code</returns>
        /// <exception cref="InvalidOperationException">Failed to write pact file</exception>
        void WritePactFile(int mockServerPort, string directory, bool overwrite);

        /// <summary>
        /// Create a new pact
        /// </summary>
        /// <param name="consumerName">Consumer name</param>
        /// <param name="providerName">Provider name</param>
        /// <returns>Pact handle</returns>
        PactHandle NewPact(string consumerName, string providerName);

        /// <summary>
        /// Set the pact specification version
        /// </summary>
        /// <param name="pact">Pact handle</param>
        /// <param name="version">Specification version</param>
        /// <returns>Success</returns>
        bool WithSpecification(PactHandle pact, PactSpecification version);

        /// <summary>
        /// Create a new interaction on the given pact
        /// </summary>
        /// <param name="pact">Pact handle</param>
        /// <param name="description">Interaction description</param>
        /// <returns></returns>
        InteractionHandle NewInteraction(PactHandle pact, string description);

        /// <summary>
        /// Add a provider state to the interaction
        /// </summary>
        /// <param name="interaction">Interaction</param>
        /// <param name="description">Provider state description</param>
        /// <returns>Success</returns>
        bool Given(InteractionHandle interaction, string description);

        /// <summary>
        /// Add a provider state with a parameter to the interaction
        /// </summary>
        /// <param name="interaction">Interaction</param>
        /// <param name="description">Provider state description</param>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <returns>Success</returns>
        bool GivenWithParam(InteractionHandle interaction, string description, string name, string value);

        /// <summary>
        /// Add a request to the interaction
        /// </summary>
        /// <param name="interaction">Interaction</param>
        /// <param name="method">Request method</param>
        /// <param name="path">Request path</param>
        /// <returns>Success</returns>
        bool WithRequest(InteractionHandle interaction, string method, string path);

        /// <summary>
        /// Add a query string parameter to the interaction
        /// </summary>
        /// <param name="interaction">Interaction</param>
        /// <param name="name">Query string parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <param name="index">Parameter index (for if the same name is used multiple times)</param>
        /// <returns>Success</returns>
        bool WithQueryParameter(InteractionHandle interaction, string name, string value, uint index);

        /// <summary>
        /// Set a request header
        /// </summary>
        /// <param name="interaction">Interaction</param>
        /// <param name="name">header name</param>
        /// <param name="value">Header value</param>
        /// <param name="index">Header index (for if the same header is added multiple times)</param>
        /// <returns>Success</returns>
        bool WithRequestHeader(InteractionHandle interaction, string name, string value, uint index);

        /// <summary>
        /// Set a response header
        /// </summary>
        /// <param name="interaction">Interaction</param>
        /// <param name="name">header name</param>
        /// <param name="value">Header value</param>
        /// <param name="index">Header index (for if the same header is added multiple times)</param>
        /// <returns>Success</returns>
        bool WithResponseHeader(InteractionHandle interaction, string name, string value, uint index);

        /// <summary>
        /// Set the response status
        /// </summary>
        /// <param name="interaction">Interaction</param>
        /// <param name="status">Status code</param>
        /// <returns>Success</returns>
        bool ResponseStatus(InteractionHandle interaction, ushort status);

        /// <summary>
        /// Set the request body
        /// </summary>
        /// <param name="interaction">Interaction</param>
        /// <param name="contentType">Context type</param>
        /// <param name="body">Serialised body</param>
        /// <returns>Success</returns>
        bool WithRequestBody(InteractionHandle interaction, string contentType, string body);

        /// <summary>
        /// Set the response body
        /// </summary>
        /// <param name="interaction">Interaction</param>
        /// <param name="contentType">Context type</param>
        /// <param name="body">Serialised body</param>
        /// <returns>Success</returns>
        bool WithResponseBody(InteractionHandle interaction, string contentType, string body);

        /// <summary>
        /// Write the pact message file
        /// </summary>
        /// <param name="pact">the pact</param>
        /// <param name="directory">the output folder</param>
        /// <param name="overwrite">overwrite</param>
        /// <returns></returns>
        public void WriteMessagePactFile(MessagePactHandle pact, string directory, bool overwrite);

        /// <summary>
        /// ??
        /// </summary>
        /// <param name="pact"></param>
        /// <param name="namespace"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WithMessagePactMetadata(MessagePactHandle pact, string @namespace, string name, string value);

        /// <summary>
        /// Create a new message pact
        /// </summary>
        /// <param name="consumerName">Consumer name</param>
        /// <param name="providerName">Provider name</param>
        /// <returns>Pact handle</returns>
        public MessagePactHandle NewMessagePact(string consumerName, string providerName);

        /// <summary>
        /// Create a new message on the given pact
        /// </summary>
        /// <param name="pact">Pact handle</param>
        /// <param name="description">Interaction description</param>
        /// <returns></returns>
        public MessageHandle NewMessage(MessagePactHandle pact, string description);

        /// <summary>
        /// Set the description of the message description
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="description">message description</param>
        /// <returns>Success</returns>
        public bool MessageExpectsToReceive(MessageHandle message, string description);

        /// <summary>
        /// Add a provider state to the message
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="description">Provider state description</param>
        /// <returns>Success</returns>
        public bool MessageGiven(MessageHandle message, string description);

        /// <summary>
        /// Add a provider state with a parameter to the interaction
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="description">Provider state description</param>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <returns>Success</returns>
        public bool MessageGivenWithParam(MessageHandle message, string description, string name, string value);

        /// <summary>
        /// Set the metadata of the message
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="key">the key</param>
        /// <param name="value">the value</param>
        /// <returns>Success</returns>
        public bool MessageWithMetadata(MessageHandle message, string key, string value);

        /// <summary>
        /// Set the contents of the message
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="contentType">the content type</param>
        /// <param name="body">the body of the message</param>
        /// <param name="size">the size of the message</param>
        /// <returns>Success</returns>
        public bool MessageWithContents(MessageHandle message, string contentType, string body, uint size);

        /// <summary>
        /// returns the message without the matchers
        /// </summary>
        /// <param name="message">message</param>
        /// <returns>Success</returns>
        public string MessageReify(MessageHandle message);
    }
}
