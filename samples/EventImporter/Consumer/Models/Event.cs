using System;

namespace Consumer.Models
{
    public class Event
    {
        public Guid EventId { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }
    }
}
