using Consumer.Models;

namespace Consumer.Tests
{
    public class FakeEventProcessor : IEventProcessor
    {
        public bool ProcessEvent(Event @event)
        {
            return true;
        }
    }
}
