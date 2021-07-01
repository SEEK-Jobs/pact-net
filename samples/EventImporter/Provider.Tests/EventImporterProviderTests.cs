using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Moq;

using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Native;

using Provider.Api;
using Provider.Api.Controllers;
using Provider.Domain;
using Provider.Domain.Models;

using Xunit;
using Xunit.Abstractions;

using EventHandler = Provider.Domain.Handlers.EventHandler;

namespace Provider.Tests
{
    public class EventImporterProviderTests : IClassFixture<EventApiFixture>
    {
        private readonly EventApiFixture fixture;
        private readonly ITestOutputHelper output;

        public EventImporterProviderTests(EventApiFixture fixture, ITestOutputHelper output)
        {
            this.fixture = fixture;
            this.output = output;

            SetupScenarios();
        }

        [Fact]
        public void EnsureEventApiHonoursPactWithConsumer()
        {
            var config = new PactVerifierConfig
            {
                Outputters = new List<IOutput>
                {
                    new XUnitOutput(output)
                }
            };

            //C
            string pactPath = Path.Combine("..",
                                           "..",
                                           "..",
                                           "..",
                                           "Consumer.Tests",
                                           "pacts",
                                           "Event API Consumer V3 Message-Event API V3 Message.json");

            //Act / Assert
            IPactVerifier pactVerifier = new MessagePactVerifier(config);
            pactVerifier
                .ServiceProvider("Event API V3 Message", fixture.ServerUri)
                .HonoursPactWith("Event API Consumer V3 Message")
                .FromPactFile(new FileInfo(pactPath))
                .WithProviderStateUrl(new Uri(fixture.ServerUri, "/provider-states"))
                .Verify();
        }

        private static void SetupScenarios()
        {
            MessageScenarioBuilder
                .NewScenario
                .WhenReceiving("receiving events from the queue")
                .WillPublishMessage(() =>
                {
                    var mockEventProducer = new Mock<IEventProducer>();
                    List<Event> eventsPushed = null;
                    mockEventProducer
                        .Setup(x => x.SendAsync(It.IsAny<IReadOnlyCollection<Event>>()))
                        .Callback((IReadOnlyCollection<Event> events) => { eventsPushed = events.ToList(); });

                    var controller = new EventsController(new FakeEventRepository(), new EventHandler(mockEventProducer.Object));

                    controller.ImportToQueue();

                    return eventsPushed;
                });
        }

        private class FakeEventRepository : IEventRepository
        {
            public IReadOnlyCollection<Event> GetAllEvents()
            {
                return new List<Event>
                {
                    new Event {
                        EventId = Guid.Parse("45D80D13-D5A2-48D7-8353-CBB4C0EAABF5"),
                        Timestamp = DateTime.Parse("2014-06-30T01:37:41.0660548"),
                        EventType = "SearchView"
                    }
                };
            }
        }
    }
}
