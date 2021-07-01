using System.Collections.Generic;

using Provider.Domain.Models;

namespace Provider.Domain
{
    public interface IEventProducer
    {
        void SendAsync(IReadOnlyCollection<Event> events);
    }
}
