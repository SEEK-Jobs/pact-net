using System;
using System.Collections.Generic;
using System.Linq;
using PactNet.Matchers;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using System.Threading.Tasks;
using Xunit;

namespace Consumer.Tests
{
    public class EventsApiConsumerTests : IClassFixture<ConsumerEventApiPact>
    {
        private readonly IMockProviderService _mockProviderService;
        private readonly string _mockProviderServiceBaseUri;

        public EventsApiConsumerTests(ConsumerEventApiPact data)
        {
            _mockProviderService = data.MockProviderService;
            _mockProviderServiceBaseUri = data.MockProviderServiceBaseUri;
            _mockProviderService.ClearInteractions();
        }

        [Fact]
        public async Task GetAllEvents_WithNoAuthorizationToken_ShouldFail()
        {
            //Arrange
            _mockProviderService.Given("there are events with ids '45D80D13-D5A2-48D7-8353-CBB4C0EAABF5', '83F9262F-28F1-4703-AB1A-8CFD9E8249C9' and '3E83A96B-2A0C-49B1-9959-26DF23F83AEB'")
                .UponReceiving("a request to retrieve all events with no authorization")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = "/events",
                    Headers = new Dictionary<string, object>
                    {
                        { "Accept", "application/json" },
                    }
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 401,
                    Headers = new Dictionary<string, object>
                    {
                        { "Content-Type", "application/json; charset=utf-8" }
                    },
                    Body = new 
                    {
                        message = "Authorization has been denied for this request."
                    }
                });

            var consumer = new EventsApiClient(_mockProviderServiceBaseUri);
            
            //Act //Assert
            await Assert.ThrowsAnyAsync<Exception>(() => consumer.GetAllEvents());
            
            _mockProviderService.VerifyInteractions();
        }

        [Fact]
        public async Task GetAllEvents_WhenCalled_ReturnsAllEvents()
        {
            var test = new[]
            {
                new
                {
                    eventId = Guid.Parse("45D80D13-D5A2-48D7-8353-CBB4C0EAABF5"),
                    timestamp = "2014-06-30T01:37:41.0660548",
                    eventType = "SearchView"
                },
                new
                {
                    eventId = Guid.Parse("83F9262F-28F1-4703-AB1A-8CFD9E8249C9"),
                    timestamp = "2014-06-30T01:37:52.2618864",
                    eventType = "DetailsView"
                },
                new
                {
                    eventId = Guid.Parse("3E83A96B-2A0C-49B1-9959-26DF23F83AEB"),
                    timestamp = "2014-06-30T01:38:00.8518952",
                    eventType = "SearchView"
                }
            };

            var res = new ProviderServiceResponse
            {
                Status = 200,
                Headers = new Dictionary<string, object>
                {
                    {"Content-Type", "application/json; charset=utf-8"}
                },
                Body = test
            };

            //Arrange
            var testAuthToken = "SomeValidAuthToken";

            _mockProviderService.Given("there are events with ids '45D80D13-D5A2-48D7-8353-CBB4C0EAABF5', '83F9262F-28F1-4703-AB1A-8CFD9E8249C9' and '3E83A96B-2A0C-49B1-9959-26DF23F83AEB'")
                .UponReceiving("a request to retrieve all events")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = "/events",
                    Headers = new Dictionary<string, object>
                    {
                        { "Accept", "application/json" },
                        { "Authorization", $"Bearer {testAuthToken}" }
                    }
                })
                .WillRespondWith(res);

            var consumer = new EventsApiClient(_mockProviderServiceBaseUri, testAuthToken);

            //Act
            var events = await consumer.GetAllEvents();

            //Assert
            Assert.NotEmpty(events);
            Assert.Equal(3, events.Count());


            _mockProviderService.VerifyInteractions();
        }

        [Fact]
        public async Task CreateEvent_WhenCalledWithEvent_Succeeds()
        {
            //Arrange
            var eventId = Guid.Parse("1F587704-2DCC-4313-A233-7B62B4B469DB");
            var dateTime = new DateTime(2011, 07, 01, 01, 41, 03);
            DateTimeFactory.Now = () => dateTime;

            _mockProviderService.UponReceiving("a request to create a new event")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Post,
                    Path = "/events",
                    Headers = new Dictionary<string, object>
                    {
                        { "Content-Type", "application/json; charset=utf-8" }
                    },
                    Body = new
                    {
                        eventId,
                        timestamp = dateTime.ToString("O"),
                        eventType = "DetailsView"
                    }
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 201
                });

            var consumer = new EventsApiClient(_mockProviderServiceBaseUri);

            //Act / Assert
            await consumer.CreateEvent(eventId);

            _mockProviderService.VerifyInteractions();
        }

        [Fact]
        public async Task IsAlive_WhenApiIsAlive_ReturnsTrue()
        {
            //Arrange
            _mockProviderService.UponReceiving("a request to check the api status")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Headers = new Dictionary<string, object> { { "Accept", "application/json" } },
                    Path = "/stats/status"
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 200,
                    Headers = new Dictionary<string, object> { { "Content-Type", "application/json; charset=utf-8" } },
                    Body = new
                    {
                        alive = true,
                        _links = new
                        {
                            uptime = new
                            {
                                href = "/stats/uptime"
                            }
                        }
                    }
                });

            var consumer = new EventsApiClient(_mockProviderServiceBaseUri);

            //Act
            var result = await consumer.IsAlive();

            //Assert
            Assert.Equal(true, result);

            _mockProviderService.VerifyInteractions();
        }

        [Fact]
        public async Task UpSince_WhenApiIsAliveAndWeRetrieveUptime_ReturnsUpSinceDate()
        {
            //Arrange
            var upSinceDate = new DateTime(2014, 6, 27, 23, 51, 12, DateTimeKind.Utc);

            _mockProviderService.UponReceiving("a request to check the api status")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Headers = new Dictionary<string, object> { { "Accept", "application/json" } },
                    Path = "/stats/status"
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 200,
                    Headers = new Dictionary<string, object> { { "Content-Type", "application/json; charset=utf-8" } },
                    Body = new
                    {
                        alive = true,
                        _links = new
                        {
                            uptime = new
                            {
                                href = "/stats/uptime"
                            }
                        }
                    }
                });

            _mockProviderService
                .UponReceiving("a request to check the api uptime")
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Headers = new Dictionary<string, object> { { "Accept", "application/json" } },
                    Path = "/stats/uptime"
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 200,
                    Headers = new Dictionary<string, object> { { "Content-Type", "application/json; charset=utf-8" } },
                    Body = new
                    {
                        upSince = upSinceDate
                    }
                });

            var consumer = new EventsApiClient(_mockProviderServiceBaseUri);

            //Act
            var result = await consumer.UpSince();

            //Assert
            Assert.Equal(upSinceDate.ToString("O"), result.Value.ToString("O"));

            _mockProviderService.VerifyInteractions();
        }

        [Fact]
        public async Task GetEventById_WhenTheEventExists_ReturnsEvent()
        {
            //Arrange
            var guidRegex = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
            var eventId = Guid.Parse("83F9262F-28F1-4703-AB1A-8CFD9E8249C9");
            var eventType = "DetailsView";
            var eventTimestamp = DateTime.UtcNow;
            _mockProviderService.Given(string.Format("there is an event with id '{0}'", eventId))
                .UponReceiving(string.Format("a request to retrieve event with id '{0}'", eventId))
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = Match.Regex($"/events/{eventId}", $"^\\/events\\/{guidRegex}$"),
                    Headers = new Dictionary<string, object>
                    {
                        { "Accept", "application/json" }
                    }
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 200,
                    Headers = new Dictionary<string, object>
                    {
                        { "Content-Type", "application/json; charset=utf-8" },
                        { "Server", Match.Type("RubyServer") }
                    },
                    Body = new
                    {
                        eventId = Match.Regex(eventId.ToString(), $"^{guidRegex}$"),
                        eventType = Match.Type(eventType),
                        timestamp = Match.Regex(eventTimestamp.ToString("o"), "^(-?(?:[1-9][0-9]*)?[0-9]{4})-(1[0-2]|0[1-9])-(3[0-1]|0[1-9]|[1-2][0-9])T(2[0-3]|[0-1][0-9]):([0-5][0-9]):([0-5][0-9])(\\.[0-9]+)?(Z|[+-](?:2[0-3]|[0-1][0-9]):[0-5][0-9])?$")
                    }
                });

            var consumer = new EventsApiClient(_mockProviderServiceBaseUri);

            //Act
            var result = await consumer.GetEventById(eventId);

            //Assert
            Assert.Equal(eventId, result.EventId);
            Assert.Equal(eventType, result.EventType);
            Assert.Equal(eventTimestamp, result.Timestamp);

            _mockProviderService.VerifyInteractions();
        }

        [Fact]
        public async Task GetEventsByType_WhenOneEventWithTheTypeExists_ReturnsEvent()
        {
            //Arrange
            const string eventType = "DetailsView";
            _mockProviderService.Given(string.Format("there is one event with type '{0}'", eventType))
                .UponReceiving(string.Format("a request to retrieve events with type '{0}'", eventType))
                .With(new ProviderServiceRequest
                {
                    Method = HttpVerb.Get,
                    Path = "/events",
                    Query = Match.Regex($"type={eventType}", "^type=(DetailsView|SearchView)$"),
                    Headers = new Dictionary<string, object>
                    {
                        { "Accept", "application/json" }
                    }
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = 200,
                    Headers = new Dictionary<string, object>
                    {
                        { "Content-Type", "application/json; charset=utf-8" }
                    },
                    Body = new []
                    {
                        new
                        {
                            eventType = eventType
                        }
                    }
                });

            var consumer = new EventsApiClient(_mockProviderServiceBaseUri);

            //Act
            var result = await consumer.GetEventsByType(eventType);

            //Assert
            Assert.Equal(eventType, result.First().EventType);

            _mockProviderService.VerifyInteractions();
        }
    }
}
