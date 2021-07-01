using System;

namespace Provider.Domain.Models
{
    public class Event
    {
        public Guid EventId { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }

        public Event()
        {
            
        }

        public Event(string id)
        {
            EventId = Guid.Parse(id);
            Timestamp = DateTime.Now;
            EventType = "SearchView";
        }
    }
}
