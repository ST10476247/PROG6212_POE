using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models.ViewModels;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    /// <summary>
    /// Public landing page and shared informational pages. No authentication
    /// is required to browse upcoming events.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IRaceDayApiClient _api;

        public HomeController(IRaceDayApiClient api) => _api = api;

        public async Task<IActionResult> Index(string? search, string? province, string? eventType)
        {
            var model = new EventFilterViewModel
            {
                Search = search,
                Province = province,
                EventType = eventType,
                Page = 1,
                PageSize = 6
            };

            try
            {
                model = await _api.GetEventsAsync(model);
            }
            catch (ApiException)
            {
                // The API may be temporarily unavailable — still show the landing page.
                TempData["Error"] = "Unable to reach the RaceDay service right now. Please try again shortly.";
            }

            try
            {
                ViewBag.FeaturedEvents = await _api.GetFeaturedEventsAsync();
            }
            catch (ApiException)
            {
                ViewBag.FeaturedEvents = new List<EventCardViewModel>();
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}
