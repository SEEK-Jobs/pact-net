using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using System.Threading.Tasks;
using Xunit;

namespace PactNet.Tests.IntegrationTests
{

    public class PactBuilderFailureIntegrationTests : IClassFixture<FailureIntegrationTestsMyApiPact>
    {
        private readonly IMockProviderService _mockProviderService;
        private readonly Uri _mockProviderServiceBaseUri;

        public PactBuilderFailureIntegrationTests(FailureIntegrationTestsMyApiPact data)
        {
            _mockProviderService = data.MockProviderService;
            _mockProviderServiceBaseUri = data.MockProviderServiceBaseUri;
            _mockProviderService.ClearInteractions();
        }

        [Fact]
        public void WhenRegisteringAnInteractionThatIsNeverSent_ThenPactFailureExceptionIsThrown()
        {
            _mockProviderService
                .UponReceiving("A POST request to create a new thing")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Post,
                    Path = "/things",
                    Headers = new Dictionary<string, object>
                    {
                        { "Content-Type", "application/json; charset=utf-8" }
                    },
                    Body = new
                    {
                        thingId = 1234,
                        type = "Awesome"
                    }
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 201
                });

            Assert.Throws<PactFailureException>(() => _mockProviderService.VerifyInteractions());
        }

        [Fact]
        public async Task WhenRegisteringAnInteractionThatIsSentMultipleTimes_ThenNoExceptionIsThrown()
        {
            _mockProviderService
                .UponReceiving("A GET request to retrieve a thing")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = "/things/1234"
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 200
                });

            var httpClient = new HttpClient { BaseAddress = _mockProviderServiceBaseUri };

            var request1 = new HttpRequestMessage(HttpMethod.Get, "/things/1234");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "/things/1234");

            var response1 = await httpClient.SendAsync(request1);
            var response2 = await httpClient.SendAsync(request2);

            if (response1.StatusCode != HttpStatusCode.OK || response2.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception(string.Format("Wrong status code '{0} and {1}' was returned", response1.StatusCode, response2.StatusCode));
            }

            _mockProviderService.VerifyInteractions();
        }

        [Fact]
        public async Task WhenRegisteringAnInteractionWhereTheRequestDoesNotExactlyMatchTheActualRequest_ThenStatusCodeReturnedIs500AndPactFailureExceptionIsThrown()
        {
            _mockProviderService
                .UponReceiving("A GET request to retrieve things by type")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = "/things",
                    Query = "type=awesome",
                    Headers = new Dictionary<string, object>
                    {
                        { "Accept", "application/json; charset=utf-8" }
                    },
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 200
                });

            var httpClient = new HttpClient { BaseAddress = _mockProviderServiceBaseUri };

            var request = new HttpRequestMessage(HttpMethod.Get, "/things?type=awesome");

            var response = await httpClient.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.InternalServerError)
            {
                throw new Exception(string.Format("Wrong status code '{0}' was returned", response.StatusCode));
            }

            Assert.Throws<PactFailureException>(() => _mockProviderService.VerifyInteractions());
        }
    }
}
