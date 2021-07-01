using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Provider.Domain.Handlers;

namespace Provider.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventHandler _eventHandler;

        public EventsController(IEventRepository eventRepository, IEventHandler eventHandler)
        {
            _eventRepository = eventRepository;
            _eventHandler = eventHandler;
        }

        /// <summary>
        /// ImportAllEvents to queue
        /// </summary>
        /// <returns>no content</returns>
        [Authorize]
        [HttpPost]
        public IActionResult ImportToQueue()
        {
            var events = _eventRepository.GetAllEvents();

            _eventHandler.ImportAllEvents(events);

            return NoContent();
        }
    }
}
