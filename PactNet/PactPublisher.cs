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
            if (String.IsNullOrEmpty(pactFileUri))
            {
                throw new ArgumentNullException("pactFileUri is null or empty");
            }

            if (String.IsNullOrEmpty(consumerVersion))
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

                    var tagResponse = _httpClient.SendAsync(tagRequest, CancellationToken.None).Result;
                    var tagResponseStatusCode = tagResponse.StatusCode;
                    var tagResponseContent = String.Empty;

                    if (tagResponse.Content != null)
                    {
                        tagResponseContent = tagResponse.Content.ReadAsStringAsync().Result;
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

            var response = _httpClient.SendAsync(request, CancellationToken.None).Result;
            var responseStatusCode = response.StatusCode;
            var responseContent = String.Empty;

            if (response.Content != null)
            {
                responseContent = response.Content.ReadAsStringAsync().Result;
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