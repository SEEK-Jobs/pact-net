using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Consumer.Models;

using FluentAssertions;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using PactNet;
using PactNet.Matchers;
using PactNet.Native;

using Xunit;
using Xunit.Abstractions;

namespace Consumer.Tests
{
    public class EventsApiConsumerTestsV3
    {
        private const string Token = "SomeValidAuthToken";

        private readonly IPactBuilderV3 pact;

        private readonly PactConfig config = new PactConfig
        {
            LogDir = "../../../logs/",
            PactDir = "../../../pacts/",
            DefaultJsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        };

        public EventsApiConsumerTestsV3(ITestOutputHelper output)
        {
            config.Outputters = new[]
            {
                new XUnitOutput(output)
            };
            IPactV3 pact = Pact.V3("Event API Consumer V3", "Event API V3", config);
            this.pact = pact.UsingNativeBackend();
        }

        [Fact]
        public async Task GetEventById_WhenTheEventExists_ReturnsEvent()
        {
            var expected = new Event
            {
                EventId = Guid.Parse("83F9262F-28F1-4703-AB1A-8CFD9E8249C9"),
                EventType = "DetailsView",
                Timestamp = DateTime.UtcNow
            };

            pact
                .UponReceiving($"a request to retrieve event with id '{expected.EventId}'")
                    .Given($"there is an event with id '{expected.EventId}'")
                    .WithRequest(HttpMethod.Get, $"/events/{expected.EventId}")
                    .WithHeader("accept", "application/json")
                    .WithHeader("authorization", $"Bearer {Token}")
                .WillRespond()
                    .WithStatus(200)
                    .WithHeader("content-type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    eventId = Match.Regex(expected.EventId.ToString(), "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$"),
                    eventType = Match.Type(expected.EventType),
                    timestamp = Match.Regex(expected.Timestamp.ToString("o"), "^(-?(?:[1-9][0-9]*)?[0-9]{4})-(1[0-2]|0[1-9])-(3[0-1]|0[1-9]|[1-2][0-9])T(2[0-3]|[0-1][0-9]):([0-5][0-9]):([0-5][0-9])(\\.[0-9]+)?(Z|[+-](?:2[0-3]|[0-1][0-9]):[0-5][0-9])?$")
                });

            await pact.VerifyAsync(async ctx =>
            {
                var client = new EventsApiClient(ctx.MockServerUri, Token);

                var result = await client.GetEventById(expected.EventId);

                result.Should().BeEquivalentTo(expected);
            });
        }

        [Fact]
        public async Task GetEventsByType_WhenOneEventWithTheTypeExists_ReturnsEvent()
        {
            //Arrange
            const string eventType = "DetailsView";

            pact
                .UponReceiving($"a request to retrieve events with type '{eventType}'")
                    .Given($"there is one event with type '{eventType}'")
                    .WithRequest(HttpMethod.Get, "/events")
                    .WithQuery("type", eventType)
                    .WithHeader("Accept", "application/json")
                    .WithHeader("Authorization", $"Bearer {Token}")
                .WillRespond()
                    .WithStatus(200)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new[]
                    {
                        new
                        {
                            eventType
                        }
                    });

            await pact.VerifyAsync(async ctx =>
            {
                var client = new EventsApiClient(ctx.MockServerUri, Token);

                var result = await client.GetEventsByType(eventType);

                result.Should().OnlyContain(e => e.EventType == eventType);
            });
        }

        #region V3 support

        [Fact]
        public async Task GetAllEvents_Given_Multiple_ProviderStates_WhenCalled_ReturnsAllEvents()
        {
            var expected = new[]
            {
                new Event
                {
                    EventId = Guid.Parse("45D80D13-D5A2-48D7-8353-CBB4C0EAABF5"),
                    Timestamp = DateTime.Parse("2014-06-30T01:37:41.0660548"),
                    EventType = "SearchView"
                },
                new Event
                {
                    EventId = Guid.Parse("83F9262F-28F1-4703-AB1A-8CFD9E8249C9"),
                    Timestamp = DateTime.Parse("2014-06-30T01:37:52.2618864"),
                    EventType = "DetailsView"
                },
                new Event
                {
                    EventId = Guid.Parse("3E83A96B-2A0C-49B1-9959-26DF23F83AEB"),
                    Timestamp = DateTime.Parse("2014-06-30T01:38:00.8518952"),
                    EventType = "SearchView"
                }
            };

            pact
                .UponReceiving("a request to retrieve all events with multiple provider states")
                .Given("there are events with id '45D80D13-D5A2-48D7-8353-CBB4C0EAABF5'")
                .Given("there are events with id '83F9262F-28F1-4703-AB1A-8CFD9E8249C9'")
                .Given("there are events with id '3E83A96B-2A0C-49B1-9959-26DF23F83AEB'")
                .WithRequest(HttpMethod.Get, "/events")
                .WithHeader("Accept", "application/json")
                .WithHeader("Authorization", $"Bearer {Token}")
                .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(expected);

            await pact.VerifyAsync(async ctx =>
            {
                var client = new EventsApiClient(ctx.MockServerUri, Token);

                IEnumerable<Event> events = await client.GetAllEvents();

                events.Should().BeEquivalentTo(expected);
            });
        }

        [Fact]
        public async Task GetAllEvents_Given_Multiple_ProviderStates_WithParams_WhenCalled_ReturnsAllEvents()
        {
            var expected = new[]
            {
                new Event
                {
                    EventId = Guid.Parse("45D80D13-D5A2-48D7-8353-CBB4C0EAABF5"),
                    Timestamp = DateTime.Parse("2014-06-30T01:37:41.0660548"),
                    EventType = "SearchView"
                },
                new Event
                {
                    EventId = Guid.Parse("83F9262F-28F1-4703-AB1A-8CFD9E8249C9"),
                    Timestamp = DateTime.Parse("2014-06-30T01:37:52.2618864"),
                    EventType = "DetailsView"
                },
                new Event
                {
                    EventId = Guid.Parse("3E83A96B-2A0C-49B1-9959-26DF23F83AEB"),
                    Timestamp = DateTime.Parse("2014-06-30T01:38:00.8518952"),
                    EventType = "SearchView"
                }
            };

            var providerStateWithParams = new Dictionary<string, string>
            {
                ["eventId1"] = expected[0].EventId.ToString(),
                ["eventId2"] = expected[1].EventId.ToString(),
                ["eventId3"] = expected[2].EventId.ToString()
            };

            pact
                .UponReceiving("a request to retrieve all events with multiple provider states and params")
                .Given("there are events with ids", providerStateWithParams)
                .WithRequest(HttpMethod.Get, "/events")
                .WithHeader("Accept", "application/json")
                .WithHeader("Authorization", $"Bearer {Token}")
                .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(expected);

            await pact.VerifyAsync(async ctx =>
            {
                var client = new EventsApiClient(ctx.MockServerUri, Token);

                IEnumerable<Event> events = await client.GetAllEvents();

                events.Should().BeEquivalentTo(expected);
            });
        }

        #endregion
    }
}
