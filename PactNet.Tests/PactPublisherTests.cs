using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using PactNet.Models;
using PactNet.Tests.Fakes;
using System.Threading.Tasks;
using Xunit;

namespace PactNet.Tests
{
    public class PactPublisherTests
    {
        private static string PactFilePath = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Samples{Path.DirectorySeparatorChar}HttpEventApi{Path.DirectorySeparatorChar}Consumer.Tests{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}event_api_consumer-event_api.json";
        private const string ConsumerVersion = "1.0.2";
        private const string BrokerBaseUriHttp = "http://test.pact.dius.com.au";
        private const string BrokerBaseUriHttps = "https://test.pact.dius.com.au";

        private static readonly PactUriOptions AuthOptions = new PactUriOptions().SetBasicAuthentication("username", "password");

        private FakeHttpMessageHandler _fakeHttpMessageHandler;

        private PactPublisher GetSubject(string brokerBaseUri, PactUriOptions brokerUriOptions = null)
        {
            _fakeHttpMessageHandler = new FakeHttpMessageHandler();

            return new PactPublisher(brokerBaseUri, brokerUriOptions, _fakeHttpMessageHandler);
        }

        [Fact]
        public async Task PublishToBrokerAsync_WithNullPactFileUri_ThrowArgumentNullException()
        {
            var pactPublisher = GetSubject(BrokerBaseUriHttp);

            await Assert.ThrowsAsync<ArgumentNullException>(() => pactPublisher.PublishToBrokerAsync(null, "1.0.0"));
        }

        [Fact]
        public async Task PublishToBrokerAsync_WithEmptyPactFileUri_ThrowArgumentNullException()
        {
            var pactPublisher = GetSubject(BrokerBaseUriHttp);

            await Assert.ThrowsAsync<ArgumentNullException>(() => pactPublisher.PublishToBrokerAsync(string.Empty, "1.0.0"));
        }

        [Fact]
        public async Task PublishToBrokerAsync_WithNullConsumerVersion_ThrowArgumentNullException()
        {
            var pactPublisher = GetSubject(BrokerBaseUriHttp);

            await Assert.ThrowsAsync<ArgumentNullException>(() => pactPublisher.PublishToBrokerAsync(PactFilePath, null));
        }

        [Fact]
        public async Task PublishToBrokerAsync_WithEmptyConsumerVersion_ThrowArgumentNullException()
        {
            var pactPublisher = GetSubject(BrokerBaseUriHttp);

            await Assert.ThrowsAsync<ArgumentNullException>(() => pactPublisher.PublishToBrokerAsync(PactFilePath, string.Empty));
        }

        [Fact]
        public async Task PublishToBrokerAsync_WithNoAuthentication_PublishesPact()
        {
            var pactPublisher = GetSubject(BrokerBaseUriHttp);
            var pactFileText = File.ReadAllText(PactFilePath);
            var pactDetails = JsonConvert.DeserializeObject<PactDetails>(pactFileText);

            await pactPublisher.PublishToBrokerAsync(PactFilePath, ConsumerVersion);

            var requestsReceived = _fakeHttpMessageHandler.RequestsReceived;
            Assert.Equal(1, requestsReceived.Count());
            this.AssertPactPublishRequest(requestsReceived.ElementAt(0), _fakeHttpMessageHandler.RequestContentReceived.ElementAt(0), BrokerBaseUriHttp, pactDetails, pactFileText, ConsumerVersion);
        }

        [Fact]
        public async Task PublishToBrokerAsync_WithAuthentication_PublishesPact()
        {
            var pactPublisher = GetSubject(BrokerBaseUriHttps, AuthOptions);
            var pactFileText = File.ReadAllText(PactFilePath);
            var pactDetails = JsonConvert.DeserializeObject<PactDetails>(pactFileText);

            await pactPublisher.PublishToBrokerAsync(PactFilePath, ConsumerVersion);

            var requestsReceived = _fakeHttpMessageHandler.RequestsReceived;
            Assert.Equal(1, requestsReceived.Count());
            this.AssertPactPublishRequest(requestsReceived.ElementAt(0), _fakeHttpMessageHandler.RequestContentReceived.ElementAt(0), BrokerBaseUriHttps, pactDetails, pactFileText, ConsumerVersion, AuthOptions);
        }

        [Fact]
        public async Task PublishToBrokerAsync_WhenCalledWithoutTags_PublishesPactWithoutTags()
        {
            var pactPublisher = GetSubject(BrokerBaseUriHttps);
            var pactFileText = File.ReadAllText(PactFilePath);
            var pactDetails = JsonConvert.DeserializeObject<PactDetails>(pactFileText);

            await pactPublisher.PublishToBrokerAsync(PactFilePath, ConsumerVersion);

            var requestsReceived = _fakeHttpMessageHandler.RequestsReceived;
            Assert.Equal(1, requestsReceived.Count());
            this.AssertPactPublishRequest(requestsReceived.ElementAt(0), _fakeHttpMessageHandler.RequestContentReceived.ElementAt(0), BrokerBaseUriHttps, pactDetails, pactFileText, ConsumerVersion);
        }

        [Fact]
        public async Task PublishToBrokerAsync_WhenCalledWithTags_PublishesPactWithTags()
        {
            var pactPublisher = GetSubject(BrokerBaseUriHttps, AuthOptions);
            var pactFileText = File.ReadAllText(PactFilePath);
            var tags = new[] { "master", "something-else" };
            var pactDetails = JsonConvert.DeserializeObject<PactDetails>(pactFileText);

            await pactPublisher.PublishToBrokerAsync(PactFilePath, ConsumerVersion, tags);

            var requestsReceived = _fakeHttpMessageHandler.RequestsReceived;
            Assert.Equal(3, requestsReceived.Count());
            this.AssertPactTagRequest(requestsReceived.ElementAt(0), _fakeHttpMessageHandler.RequestContentReceived.ElementAt(0), BrokerBaseUriHttps, pactDetails, ConsumerVersion, tags.ElementAt(0), AuthOptions);
            this.AssertPactTagRequest(requestsReceived.ElementAt(1), _fakeHttpMessageHandler.RequestContentReceived.ElementAt(1), BrokerBaseUriHttps, pactDetails, ConsumerVersion, tags.ElementAt(1), AuthOptions);
            this.AssertPactPublishRequest(requestsReceived.ElementAt(2), _fakeHttpMessageHandler.RequestContentReceived.ElementAt(2), BrokerBaseUriHttps, pactDetails, pactFileText, ConsumerVersion, AuthOptions);
        }

        [Fact]
        public void PublishToBroker_WithNoAuthentication_PublishesPact()
        {
            var pactPublisher = GetSubject(BrokerBaseUriHttp);
            var pactFileText = File.ReadAllText(PactFilePath);
            var pactDetails = JsonConvert.DeserializeObject<PactDetails>(pactFileText);

            pactPublisher.PublishToBroker(PactFilePath, ConsumerVersion);

            var requestsReceived = _fakeHttpMessageHandler.RequestsReceived;
            Assert.Equal(1, requestsReceived.Count());
            this.AssertPactPublishRequest(requestsReceived.ElementAt(0), _fakeHttpMessageHandler.RequestContentReceived.ElementAt(0), BrokerBaseUriHttp, pactDetails, pactFileText, ConsumerVersion);
        }

        private void AssertPactPublishRequest(
            HttpRequestMessage httpRequest,
            string httpRequestContent,
            string brokerBaseUri,
            PactDetails expectedPactDetails,
            string expectedPactFile,
            string expectedConsumerVersion,
            PactUriOptions expectedPactUriOptions = null)
        {
            if (expectedPactUriOptions != null)
            {
                Assert.Equal(expectedPactUriOptions.AuthorizationScheme, httpRequest.Headers.Authorization.Scheme);
                Assert.Equal(expectedPactUriOptions.AuthorizationValue, httpRequest.Headers.Authorization.Parameter);
            }

            string expectedUri = $"{brokerBaseUri}/pacts/provider/{Uri.EscapeDataString(expectedPactDetails.Provider.Name)}/consumer/{Uri.EscapeDataString(expectedPactDetails.Consumer.Name)}/version/{expectedConsumerVersion}";

            Assert.Equal(HttpMethod.Put, httpRequest.Method);
            Assert.Equal(expectedUri, httpRequest.RequestUri.OriginalString);
            Assert.Equal(expectedPactFile, httpRequestContent);
            Assert.Equal("application/json", httpRequest.Content.Headers.ContentType.MediaType);
            Assert.Equal("utf-8", httpRequest.Content.Headers.ContentType.CharSet);
        }

        private void AssertPactTagRequest(
            HttpRequestMessage httpRequest,
            string httpRequestContent,
            string brokerBaseUri,
            PactDetails expectedPactDetails,
            string expectedConsumerVersion,
            string expectedTag,
            PactUriOptions expectedPactUriOptions = null)
        {
            if (expectedPactUriOptions != null)
            {
                Assert.Equal(expectedPactUriOptions.AuthorizationScheme, httpRequest.Headers.Authorization.Scheme);
                Assert.Equal(expectedPactUriOptions.AuthorizationValue, httpRequest.Headers.Authorization.Parameter);
            }

            string expectedUri = $"{brokerBaseUri}/pacticipants/{Uri.EscapeDataString(expectedPactDetails.Consumer.Name)}/versions/{expectedConsumerVersion}/tags/{Uri.EscapeDataString(expectedTag)}";

            Assert.Equal(HttpMethod.Put, httpRequest.Method);
            Assert.Equal(expectedUri, httpRequest.RequestUri.OriginalString);
            Assert.Equal(string.Empty, httpRequestContent);
            Assert.Equal("application/json", httpRequest.Content.Headers.ContentType.MediaType);
            Assert.Equal("utf-8", httpRequest.Content.Headers.ContentType.CharSet);
        }
    }
}
