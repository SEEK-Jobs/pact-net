using System.Collections.Generic;

using Provider.Domain.Models;

namespace Provider.Api
{
    public interface IEventRepository
    {
        IReadOnlyCollection<Event> GetAllEvents();
    }
}
