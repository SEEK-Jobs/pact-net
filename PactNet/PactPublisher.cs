using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using PactNet.Models;
using System.Threading.Tasks;
using PactNet.Core;

namespace PactNet
{
    public class PactPublisher
    {
        private readonly HttpClient _httpClient;
        private readonly PactUriOptions _brokerUriOptions;

        internal PactPublisher(
            string brokerBaseUri,
            PactUriOptions brokerUriOptions,
            HttpMessageHandler handler)
        {
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(brokerBaseUri) };
            _brokerUriOptions = brokerUriOptions;
        }

        public PactPublisher(string brokerBaseUri, PactUriOptions brokerUriOptions = null) :
            this(brokerBaseUri, brokerUriOptions, new HttpClientHandler())
        {
        }

        public void PublishToBroker(string pactFileUri, string consumerVersion, IEnumerable<string> tags = null)
        {
            Async.RunSync(() => PublishToBrokerAsync(pactFileUri, consumerVersion, tags));
        }

        public async Task PublishToBrokerAsync(string pactFileUri, string consumerVersion, IEnumerable<string> tags = null)
        {
            if (string.IsNullOrEmpty(pactFileUri))
            {
                throw new ArgumentNullException("pactFileUri is null or empty");
            }

            if (string.IsNullOrEmpty(consumerVersion))
            {
                throw new ArgumentNullException("consumerVersion is null or empty");
            }

            var pactFileText = File.ReadAllText(pactFileUri);
            var pactDetails = JsonConvert.DeserializeObject<PactDetails>(pactFileText);

            if (tags != null && tags.Any())
            {
                foreach (var tag in tags)
                {
                    var tagRequest = new HttpRequestMessage(HttpMethod.Put, $"/pacticipants/{Uri.EscapeDataString(pactDetails.Consumer.Name)}/versions/{consumerVersion}/tags/{Uri.EscapeDataString(tag)}");
                    tagRequest.Content = new StringContent("", Encoding.UTF8, "application/json");

                    if (_brokerUriOptions != null)
                    {
                        tagRequest.Headers.Add("Authorization", $"{_brokerUriOptions.AuthorizationScheme} {_brokerUriOptions.AuthorizationValue}");
                    }

                    var tagResponse = await _httpClient.SendAsync(tagRequest, CancellationToken.None);
                    var tagResponseStatusCode = tagResponse.StatusCode;
                    var tagResponseContent = string.Empty;

                    if (tagResponse.Content != null)
                    {
                        tagResponseContent = await tagResponse.Content.ReadAsStringAsync();
                    }

                    Dispose(tagRequest);
                    Dispose(tagResponse);

                    if (!IsSuccessStatusCode(tagResponseStatusCode))
                    {
                        throw new PactFailureException($"Failed to add Pact tag '{tag}' to the broker with http status {tagResponseStatusCode}: {tagResponseContent}");
                    }
                }
            }

            var request = new HttpRequestMessage(HttpMethod.Put, $"/pacts/provider/{Uri.EscapeDataString(pactDetails.Provider.Name)}/consumer/{Uri.EscapeDataString(pactDetails.Consumer.Name)}/version/{consumerVersion}");

            if (_brokerUriOptions != null)
            {
                request.Headers.Add("Authorization", $"{_brokerUriOptions.AuthorizationScheme} {_brokerUriOptions.AuthorizationValue}");
            }

            request.Content = new StringContent(pactFileText, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, CancellationToken.None);
            var responseStatusCode = response.StatusCode;
            var responseContent = string.Empty;

            if (response.Content != null)
            {
                responseContent = await response.Content.ReadAsStringAsync();
            }

            Dispose(request);
            Dispose(response);

            if (!IsSuccessStatusCode(responseStatusCode))
            {
                throw new PactFailureException($"Failed to publish Pact to the broker with http status {responseStatusCode}: {responseContent}");
            }
        }

        private bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            if (statusCode >= HttpStatusCode.OK)
            {
                return statusCode <= (HttpStatusCode)299;
            }
            return false;
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