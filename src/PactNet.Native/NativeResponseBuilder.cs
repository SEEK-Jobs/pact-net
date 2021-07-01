using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace PactNet.Native
{
    /// <summary>
    /// Mock response builder
    /// </summary>
    public class NativeResponseBuilder : IResponseBuilderV2, IResponseBuilderV3
    {
        private readonly IHttpMockServer server;
        private readonly InteractionHandle interaction;
        private readonly JsonSerializerSettings defaultSettings;
        private readonly Dictionary<string, uint> headerCounts;

        /// <summary>
        /// Initialises a new instance of the <see cref="NativeResponseBuilder"/> class.
        /// </summary>
        /// <param name="server">Mock server</param>
        /// <param name="interaction">Interaction handle</param>
        /// <param name="defaultSettings">Default JSON serializer settings</param>
        internal NativeResponseBuilder(IHttpMockServer server, InteractionHandle interaction, JsonSerializerSettings defaultSettings)
        {
            this.server = server;
            this.interaction = interaction;
            this.defaultSettings = defaultSettings;
            this.headerCounts = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        }

        #region IResponseBuilderV2 explicit implementation

        /// <summary>
        /// Set response status code
        /// </summary>
        /// <param name="status">Response status code</param>
        /// <returns>Fluent builder</returns>
        IResponseBuilderV2 IResponseBuilderV2.WithStatus(HttpStatusCode status)
            => this.WithStatus(status);

        /// <summary>
        /// Set response status code
        /// </summary>
        /// <param name="status">Response status code</param>
        /// <returns>Fluent builder</returns>
        IResponseBuilderV2 IResponseBuilderV2.WithStatus(ushort status)
            => this.WithStatus(status);

        /// <summary>
        /// Add a response header
        /// </summary>
        /// <param name="key">Header key</param>
        /// <param name="value">Header value</param>
        /// <returns>Fluent builder</returns>
        IResponseBuilderV2 IResponseBuilderV2.WithHeader(string key, string value)
            => this.WithHeader(key, value);

        /// <summary>
        /// Set a body which is serialised as JSON
        /// </summary>
        /// <param name="body">Request body</param>
        /// <returns>Fluent builder</returns>
        IResponseBuilderV2 IResponseBuilderV2.WithJsonBody(dynamic body)
            => this.WithJsonBody(body);

        /// <summary>
        /// Set a body which is serialised as JSON
        /// </summary>
        /// <param name="body">Request body</param>
        /// <param name="settings">Custom JSON serializer settings</param>
        /// <returns>Fluent builder</returns>
        IResponseBuilderV2 IResponseBuilderV2.WithJsonBody(dynamic body, JsonSerializerSettings settings)
            => this.WithJsonBody(body, settings);

        #endregion

        #region IResponseBuilderV3 explicit implementation

        /// <summary>
        /// Set response status code
        /// </summary>
        /// <param name="status">Response status code</param>
        /// <returns>Fluent builder</returns>
        IResponseBuilderV3 IResponseBuilderV3.WithStatus(HttpStatusCode status)
            => this.WithStatus(status);

        /// <summary>
        /// Set response status code
        /// </summary>
        /// <param name="status">Response status code</param>
        /// <returns>Fluent builder</returns>
        IResponseBuilderV3 IResponseBuilderV3.WithStatus(ushort status)
            => this.WithStatus(status);

        /// <summary>
        /// Add a response header
        /// </summary>
        /// <param name="key">Header key</param>
        /// <param name="value">Header value</param>
        /// <returns>Fluent builder</returns>
        IResponseBuilderV3 IResponseBuilderV3.WithHeader(string key, string value)
            => this.WithHeader(key, value);

        /// <summary>
        /// Set a body which is serialised as JSON
        /// </summary>
        /// <param name="body">Request body</param>
        /// <returns>Fluent builder</returns>
        IResponseBuilderV3 IResponseBuilderV3.WithJsonBody(dynamic body)
            => this.WithJsonBody(body);

        /// <summary>
        /// Set a body which is serialised as JSON
        /// </summary>
        /// <param name="body">Request body</param>
        /// <param name="settings">Custom JSON serializer settings</param>
        /// <returns>Fluent builder</returns>
        IResponseBuilderV3 IResponseBuilderV3.WithJsonBody(dynamic body, JsonSerializerSettings settings)
            => this.WithJsonBody(body, settings);

        #endregion

        /// <summary>
        /// Set response status code
        /// </summary>
        /// <param name="status">Response status code</param>
        /// <returns>Fluent builder</returns>
        internal NativeResponseBuilder WithStatus(HttpStatusCode status)
        {
            ushort converted = (ushort)status;

            this.server.ResponseStatus(this.interaction, converted);
            return this;
        }

        /// <summary>
        /// Set response status code
        /// </summary>
        /// <param name="status">Response status code</param>
        /// <returns>Fluent builder</returns>
        internal NativeResponseBuilder WithStatus(ushort status)
        {
            this.server.ResponseStatus(this.interaction, status);
            return this;
        }

        /// <summary>
        /// Add a response header
        /// </summary>
        /// <param name="key">Header key</param>
        /// <param name="value">Header value</param>
        /// <returns>Fluent builder</returns>
        internal NativeResponseBuilder WithHeader(string key, string value)
        {
            uint index = this.headerCounts.ContainsKey(key) ? this.headerCounts[key] + 1 : 0;
            this.headerCounts[key] = index;

            this.server.WithResponseHeader(this.interaction, key, value, index);
            return this;
        }

        /// <summary>
        /// Set a body which is serialised as JSON
        /// </summary>
        /// <param name="body">Request body</param>
        /// <returns>Fluent builder</returns>
        internal NativeResponseBuilder WithJsonBody(dynamic body)
            => this.WithJsonBody(body, this.defaultSettings);

        /// <summary>
        /// Set a body which is serialised as JSON
        /// </summary>
        /// <param name="body">Request body</param>
        /// <param name="settings">Custom JSON serializer settings</param>
        /// <returns>Fluent builder</returns>
        internal NativeResponseBuilder WithJsonBody(dynamic body, JsonSerializerSettings settings)
        {
            string serialised = JsonConvert.SerializeObject(body, settings);

            this.server.WithResponseBody(this.interaction, "application/json", serialised);
            return this;
        }
    }
}
