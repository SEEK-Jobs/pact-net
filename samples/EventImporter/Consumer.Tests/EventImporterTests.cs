using System;
using System.Collections.Generic;

using Consumer.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using PactNet;
using PactNet.Matchers;
using PactNet.Native;

using Xunit;
using Xunit.Abstractions;

namespace Consumer.Tests
{
    public class EventImporterTests
    {
        private readonly IPactMessageBuilderV3 _pactMessage;

        private readonly PactConfig _config = new PactConfig
        {
            LogDir = "../../../logs/",
            PactDir = "../../../pacts/",
            DefaultJsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        };

        public EventImporterTests(ITestOutputHelper output)
        {
            _config.Outputters = new[]
            {
                new XUnitOutput(output)
            };

            IPactV3 pactV3 = Pact.V3("Event API Consumer V3 Message", "Event API V3 Message", _config);
            _pactMessage = pactV3.UsingNativeBackendForMessage();
        }

        [Fact]
        public void GetAllEvents_FromQueue_Should_CreatePact_WithMessages()
        {
            var expected = new List<Event>
            {
                new Event(){
                    EventId = Guid.Parse("45D80D13-D5A2-48D7-8353-CBB4C0EAABF5"),
                    Timestamp = DateTime.Parse("2014-06-30T01:37:41.0660548"),
                    EventType = "SearchView"
                }
            };

            var worker = new EventsWorker(new FakeEventProcessor());

            _pactMessage
                .ExpectsToReceive("receiving events from the queue")
                .Given("A list of events is pushed to the queue")
                .WithMetadata("key", "valueKey")
                .WithContent(Match.Type(expected))
                .Verify<List<Event>>(events => worker.ProcessMessages(events));

            _pactMessage.Build();
        }
    }
}
