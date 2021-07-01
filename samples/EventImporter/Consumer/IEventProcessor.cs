using Consumer.Models;

namespace Consumer
{
    public interface IEventProcessor
    {
        bool ProcessEvent(Event @event);
    }
}
