﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consumer.Tests.Models;
using PactNet;
using Xunit;
using PactNet.Models.Messaging;
using PactNet.Models.Messaging.Consumer.Dsl;

namespace Consumer.Tests
{
    public class RecieverPactTest
    {
        [Fact]
        public void ProducePact()
        {
            IPactMessagingBuilder builder = new PactMessageBuilder();

            builder.ServiceConsumer("Consumer-dotNet")
                .HasPactWith("Provider.Messaging-dotNet");

            MessagedEvent testEvent = new MessagedEvent()
            {
                EventId = Guid.NewGuid(),
                EventType = "Parking Lot Party",
                Timestamp = DateTime.UtcNow,
                Location = new Location()
                {
                    Latitude = new Coordinate() { Degrees = 41, Minutes = 0, Seconds = 47.0 },
                    Longitude = new Coordinate() { Degrees = 24, Minutes = 17, Seconds = 11.1}
                }
            };

            var body = new PactDslJsonBody()
                .Object("partyInvite")
                    .StringType("eventType", testEvent.EventType)
                    .GuidMatcher("eventId", testEvent.EventId)
                    .DateFormat("timestampt", "", testEvent.Timestamp)
                    .Object("location")
                        .Object("latitude")
                            .Int32Type("degrees", testEvent.Location.Latitude.Degrees)
                            .Int32Type("minutes", testEvent.Location.Latitude.Minutes)
                            .DoubleType("seconds", testEvent.Location.Latitude.Seconds)
                        .CloseObject()
                        .Object("longitude")
                            .Int32Type("degrees", testEvent.Location.Longitude.Degrees)
                            .Int32Type("minutes", testEvent.Location.Longitude.Minutes)
                            .DoubleType("seconds", testEvent.Location.Longitude.Seconds)
                        .CloseObject()
                    .CloseObject()            
                .CloseObject();

            Dictionary<string, object> metaData = new Dictionary<string, object>();

            Message m = new Message()
            {
                ProviderState = "or maybe 'scenario'? not sure about this",
                Description = "my.random.topic",
                MetaData = metaData,
                Body = body
            };

            builder.WithContent(m)
              .WithMetaData(metaData);

            //Saves to disk with the default location from new PactConfig()
           builder.Build();
        }
    }
}