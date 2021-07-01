using System.Collections.Generic;

using Provider.Domain.Models;

namespace Provider.Domain.Handlers
{
    public class EventHandler : IEventHandler
    {
        private readonly IEventProducer _eventProducer;

        public EventHandler(IEventProducer eventProducer)
        {
            _eventProducer = eventProducer;
        }

        public void ImportAllEvents(IReadOnlyCollection<Event> events)
        {
            _eventProducer.SendAsync(events);
        }
    }
}
