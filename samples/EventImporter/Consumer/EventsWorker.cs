using System;
using System.Collections.Generic;
using System.Linq;
using Consumer.Models;

namespace Consumer
{
    public class EventsWorker
    {
        private readonly IEventProcessor _eventHandler;

        public EventsWorker(IEventProcessor eventHandler)
        {
            _eventHandler = eventHandler;
        }

        public void ProcessMessages(List<Event> events)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            var operationSuccessful = events.Aggregate(true, (current, @event) => current & _eventHandler.ProcessEvent(@event));

            if (!operationSuccessful)
            {
                throw new ProcessEventException();
            }
        }
    }
}
