using System.Collections.Generic;

using Provider.Api;
using Provider.Domain.Models;

namespace Provider.Infrastructure
{
    public class InMemoryEventRepository : IEventRepository
    {
        public IReadOnlyCollection<Event> GetAllEvents()
        {
            return GetAllEventsFromRepo();
        }

        private static IReadOnlyCollection<Event> GetAllEventsFromRepo()
        {
            return new List<Event>
            {
                new Event("45D80D13-D5A2-48D7-8353-CBB4C0EAABF5"),
                new Event("83F9262F-28F1-4703-AB1A-8CFD9E8249C9"),
                new Event("3E83A96B-2A0C-49B1-9959-26DF23F83AEB")
            };
        }
    }
}
