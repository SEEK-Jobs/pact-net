using System.Collections.Generic;

using Provider.Domain.Models;

namespace Provider.Domain.Handlers
{
    public interface IEventHandler
    {
        void ImportAllEvents(IReadOnlyCollection<Event> events);
    }
}
