using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using PactNet.Matchers;

using Xunit;
using Xunit.Abstractions;

namespace PactNet.Native.Tests
{
    public class PactExtensionsTests
    {
        private readonly TestData example = new()
        {
            Bool = true,
            Int = 42,
            String = "foo",
            Children = new[]
            {
                new TestData
                {
                    Bool = false,
                    Int = 7,
                    String = "bar"
                }
            }
        };

        private readonly dynamic matcher = new
        {
            Bool = Match.Type(true),
            Int = Match.Type(42),
            String = Match.Type("foo"),
            Children = Match.MinType(new[]
            {
                new
                {
                    Bool = Match.Type(false),
                    Int = Match.Type(7),
                    String = Match.Type("bar")
                }
            }, 1)
        };

        private readonly PactConfig config;

        public PactExtensionsTests(ITestOutputHelper output)
        {
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            config = new PactConfig
            {
                PactDir = Environment.CurrentDirectory,
                DefaultJsonSettings = jsonSettings,
                Outputters = new[]
                {
                    new XUnitOutput(output)
                }
            };

            File.Delete("PactExtensionsTests-Consumer-V2-PactExtensionsTests-Provider.json");
            File.Delete("PactExtensionsTests-Consumer-V3-PactExtensionsTests-Provider.json");
        }

        [Fact]
        public async Task UsingNativeBackend_V2RequestResponse_CreatesExpectedPactFile()
        {
            IPactV2 pact = Pact.V2("PactExtensionsTests-Consumer-V2", "PactExtensionsTests-Provider", config);
            IPactBuilderV2 builder = pact.UsingNativeBackend();

            builder.UponReceiving("a sample request")
                       .Given("a provider state")
                       .WithRequest(HttpMethod.Post, "/things")
                       .WithHeader("X-Request", "request1")
                       .WithHeader("X-Request", "request2")
                       .WithQuery("param", "value1")
                       .WithQuery("param", "value2")
                       .WithJsonBody(matcher)
                   .WillRespond()
                       .WithStatus(HttpStatusCode.Created)
                       .WithHeader("X-Response", "response1")
                       .WithHeader("X-Response", "response2")
                       .WithJsonBody(example);

            await builder.VerifyAsync(async ctx =>
            {
                await PerformRequestAsync(ctx, this.example, this.config.DefaultJsonSettings);
            });

            string actualPact = File.ReadAllText("PactExtensionsTests-Consumer-V2-PactExtensionsTests-Provider.json").TrimEnd();
            string expectedPact = File.ReadAllText("data/v2-consumer-integration.json").TrimEnd();

            actualPact.Should().Be(expectedPact);
        }

        [Fact]
        public async Task UsingNativeBackend_V3RequestResponse_CreatesExpectedPactFile()
        {
            IPactV3 pact = Pact.V3("PactExtensionsTests-Consumer-V3", "PactExtensionsTests-Provider", config);
            IPactBuilderV3 builder = pact.UsingNativeBackend();

            builder.UponReceiving("a sample request")
                       .Given("a provider state")
                       .Given("another provider state")
                       .Given("a provider state with params", new Dictionary<string, string>
                       {
                           ["foo"] = "bar",
                           ["baz"] = "bash"
                       })
                       .WithRequest(HttpMethod.Post, "/things")
                       .WithHeader("X-Request", "request1")
                       .WithHeader("X-Request", "request2")
                       .WithQuery("param", "value1")
                       .WithQuery("param", "value2")
                       .WithJsonBody(matcher)
                   .WillRespond()
                       .WithStatus(HttpStatusCode.Created)
                       .WithHeader("X-Response", "response1")
                       .WithHeader("X-Response", "response2")
                       .WithJsonBody(example);

            await builder.VerifyAsync(async ctx =>
            {
                await PerformRequestAsync(ctx, this.example, this.config.DefaultJsonSettings);
            });

            string actualPact = File.ReadAllText("PactExtensionsTests-Consumer-V3-PactExtensionsTests-Provider.json").TrimEnd();
            string expectedPact = File.ReadAllText("data/v3-consumer-integration.json").TrimEnd();

            actualPact.Should().Be(expectedPact);
        }

        [Fact]
        public void UsingNativeBackendForMessage_V3RequestResponse_CreatesExpectedPactFile()
        {
            IPactV3 pact = Pact.V3("PactExtensionsTests-MessageConsumer-V3", "PactExtensionsTests-MessageProvider", config);
            IPactMessageBuilderV3 builder = pact.UsingNativeBackendForMessage();

            builder.ExpectsToReceive("a sample request")
                .Given("a provider state")
                .Given("another provider state")
                .Given("a provider state with params", new Dictionary<string, string>
                {
                    ["foo"] = "bar",
                    ["baz"] = "bash"
                })
                .WithMetadata("queueId", "1234")
                .WithContent(new TestData { Int = 1, String = "a description" })
                .Verify<TestData>(_ => { });

            builder.Build();

            string actualPact = File.ReadAllText("PactExtensionsTests-MessageConsumer-V3-PactExtensionsTests-MessageProvider.json").TrimEnd();
            string expectedPact = File.ReadAllText("data/v3-message-consumer-integration.json").TrimEnd();

            actualPact.Should().Be(expectedPact);
        }

        private static async Task PerformRequestAsync(IConsumerContext context, TestData body, JsonSerializerSettings jsonSettings)
        {
            var client = new HttpClient
            {
                BaseAddress = context.MockServerUri
            };
            client.DefaultRequestHeaders.Add("X-Request", new[] { "request1", "request2" });

            string content = JsonConvert.SerializeObject(body, jsonSettings);

            HttpResponseMessage response = await client.PostAsync("/things?param=value1&param=value2",
                                                                  new StringContent(content, Encoding.UTF8, "application/json"));

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.GetValues("X-Response").Should().BeEquivalentTo(new[] { "response1", "response2" });

            string responseContent = await response.Content.ReadAsStringAsync();
            TestData responseData = JsonConvert.DeserializeObject<TestData>(responseContent, jsonSettings);
            responseData.Should().BeEquivalentTo(body);
        }

        public class TestData
        {
            public bool Bool { get; set; }
            public int Int { get; set; }
            public string String { get; set; }
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public ICollection<TestData> Children { get; set; }
        }
    }
}
