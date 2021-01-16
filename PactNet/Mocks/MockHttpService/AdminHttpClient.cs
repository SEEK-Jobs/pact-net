using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PactNet.Configuration.Json;
using PactNet.Mocks.MockHttpService.Mappers;
using PactNet.Mocks.MockHttpService.Models;
using static System.String;

namespace PactNet.Mocks.MockHttpService
{
    internal class AdminHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpMethodMapper _httpMethodMapper;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        internal AdminHttpClient(
            Uri baseUri,
            HttpMessageHandler handler,
            JsonSerializerSettings jsonSerializerSettings)
        {
            _httpClient = new HttpClient(handler) { BaseAddress = baseUri };
            _httpMethodMapper = new HttpMethodMapper();
            _jsonSerializerSettings = jsonSerializerSettings ?? JsonConfig.ApiSerializerSettings;
        }

        public AdminHttpClient(Uri baseUri) :
            this(baseUri, new HttpClientHandler(), null)
        {
        }

        public AdminHttpClient(Uri baseUri, JsonSerializerSettings jsonSerializerSettings) :
            this(baseUri, new HttpClientHandler(), jsonSerializerSettings)
        {
        }

        public async Task SendAdminHttpRequest(HttpVerb method, string path)
        {
            await SendAdminHttpRequest<object>(method, path, null);
        }

        public async Task SendAdminHttpRequest<T>(HttpVerb method, string path, T requestContent, IDictionary<string, string> headers = null) where T : class
        {
            var responseContent = Empty;

            var request = new HttpRequestMessage(_httpMethodMapper.Convert(method), path);
            request.Headers.Add(Constants.AdministrativeRequestHeaderKey, "true");

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (requestContent != null)
            {
                var requestContentJson = JsonConvert.SerializeObject(requestContent, _jsonSerializerSettings);
                request.Content = new StringContent(requestContentJson, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request, CancellationToken.None);
            var responseStatusCode = response.StatusCode;

            if (response.Content != null)
            {
                responseContent = await response.Content.ReadAsStringAsync();
            }

            Dispose(request);
            Dispose(response);

            if (responseStatusCode != HttpStatusCode.OK)
            {
                throw new PactFailureException(responseContent);
            }
        }

        private void Dispose(IDisposable disposable)
        {
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}