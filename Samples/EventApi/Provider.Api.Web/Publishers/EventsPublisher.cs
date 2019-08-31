using Newtonsoft.Json;
using Provider.Api.Web.Models;

namespace Provider.Api.Web.Publishers
{
    public class EventsPublisher
    {
        public void PublishEventUpdated(Event myEvent)
        {
            var eventUpdatedMessage = GetEventUpdatedMessage(myEvent);

            //Publish event
        }

        public Event GetEventUpdatedMessage(Event myUpdatedEvent)
        {
            return myUpdatedEvent;
        }

    }
}