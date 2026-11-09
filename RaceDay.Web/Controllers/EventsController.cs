using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Common;
using RaceDay.Web.Models.ViewModels;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    /// <summary>
    /// Public event directory and event detail pages. Browsing is open to
    /// everyone; entering an event happens from the Participant area.
    /// </summary>
    public class EventsController : Controller
    {
        private readonly IRaceDayApiClient _api;

        public EventsController(IRaceDayApiClient api) => _api = api;

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? province, string? eventType, string? status, int page = 1)
        {
            var filter = new EventFilterViewModel
            {
                Search = search,
                Province = province,
                EventType = eventType,
                Status = status,
                Page = page < 1 ? 1 : page,
                PageSize = 9
            };

            try
            {
                filter = await _api.GetEventsAsync(filter);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return View(filter);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = HttpContext.Session.GetCurrentUser();
            string? token = string.IsNullOrWhiteSpace(user.Token) ? null : user.Token;

            try
            {
                var model = await _api.GetEventDetailsAsync(id, token, user.ParticipantId);
                if (model == null)
                    return NotFound();

                return View(model);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
