using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Common;
using RaceDay.Web.Filters;
using RaceDay.Web.Models.ViewModels;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    /// <summary>
    /// Full Organiser experience: dashboard, event management, category
    /// management, enrolment roster and results entry. The entire controller is
    /// locked to the Organiser role.
    /// </summary>
    [AuthorizeRole("Organiser")]
    public class OrganiserController : Controller
    {
        private readonly IRaceDayApiClient _api;

        public OrganiserController(IRaceDayApiClient api) => _api = api;

        private SessionUser CurrentUser => HttpContext.Session.GetCurrentUser();

        // ─────────────────────────────────────────────────────────
        //  Dashboard
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = CurrentUser;
            if (!user.OrganiserId.HasValue)
            {
                TempData["Error"] = "Your organiser profile could not be found.";
                return View(new OrganiserDashboardViewModel());
            }

            try
            {
                var model = await _api.GetOrganiserDashboardAsync(user.OrganiserId.Value, user.Token);
                return View(model);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return View(new OrganiserDashboardViewModel());
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Event management
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Events()
        {
            var user = CurrentUser;
            var events = new List<OrganiserEventSummaryViewModel>();

            if (user.OrganiserId.HasValue)
            {
                try
                {
                    events = await _api.GetOrganiserEventsAsync(user.OrganiserId.Value, user.Token);
                }
                catch (ApiException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }

            return View(events);
        }

        [HttpGet]
        public IActionResult CreateEvent() => View(new CreateEventViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                int newId = await _api.CreateEventAsync(model, CurrentUser.Token);
                TempData["Success"] = $"Event '{model.Title}' was created. Now add your race categories.";
                return RedirectToAction(nameof(Categories), new { eventId = newId });
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            try
            {
                var ev = await _api.GetEventDetailsAsync(id, CurrentUser.Token);
                if (ev == null)
                    return NotFound();

                var model = new EditEventViewModel
                {
                    EventId = ev.EventId,
                    Title = ev.Title,
                    EventType = ev.EventType,
                    Description = ev.Description ?? string.Empty,
                    StartDate = ev.StartDate,
                    EndDate = ev.EndDate,
                    City = ev.City,
                    Province = ev.Province,
                    RouteUrl = ev.RouteUrl,
                    EntryFeeZar = ev.EntryFeeZar,
                    MaxEntries = ev.MaxEntries,
                    RegistrationOpen = ev.RegistrationOpen,
                    RegistrationClose = ev.RegistrationClose,
                    Status = ev.Status,
                    ElevationGainMeters = ev.ElevationGainMeters,
                    KeyLandmarks = ev.KeyLandmarks,
                    TerrainType = ev.TerrainType,
                    BannerUrl = ev.BannerUrl,
                    ExistingBannerUrl = ev.BannerUrl
                };

                return View(model);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Events));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(EditEventViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _api.UpdateEventAsync(model.EventId, model, CurrentUser.Token);
                TempData["Success"] = $"Event '{model.Title}' was updated successfully.";
                return RedirectToAction(nameof(Events));
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                await _api.DeleteEventAsync(id, CurrentUser.Token);
                TempData["Success"] = "Event deleted successfully.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Events));
        }

        // ─────────────────────────────────────────────────────────
        //  Category management
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            try
            {
                var ev = await _api.GetEventDetailsAsync(eventId, CurrentUser.Token);
                if (ev == null)
                    return NotFound();

                ViewBag.EventId = ev.EventId;
                ViewBag.EventTitle = ev.Title;
                ViewBag.Categories = await _api.GetCategoriesAsync(eventId);

                return View(new CreateCategoryViewModel { EventId = ev.EventId, EventTitle = ev.Title });
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Events));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(int eventId, CreateCategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please correct the category details and try again.";
                    return RedirectToAction(nameof(Categories), new { eventId });
                }

                await _api.AddCategoryAsync(eventId, model, CurrentUser.Token);
                TempData["Success"] = $"Category '{model.CategoryName}' was added to the event.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Categories), new { eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int categoryId, int eventId)
        {
            try
            {
                await _api.DeleteCategoryAsync(categoryId, CurrentUser.Token);
                TempData["Success"] = "Category removed.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Categories), new { eventId });
        }

        // ─────────────────────────────────────────────────────────
        //  Enrolment roster
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Enrolments(int eventId)
        {
            try
            {
                var model = await _api.GetEventEnrolmentsAsync(eventId, CurrentUser.Token);
                return View(model);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Events));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEnrolmentStatus(int enrolmentId, int eventId, string status)
        {
            try
            {
                await _api.UpdateEnrolmentStatusAsync(enrolmentId, status, CurrentUser.Token);
                TempData["Success"] = "Enrolment updated.";
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Enrolments), new { eventId });
        }

        // ─────────────────────────────────────────────────────────
        //  Results entry
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Results(int eventId)
        {
            try
            {
                var model = await _api.GetEventResultsEntryAsync(eventId, CurrentUser.Token);
                return View(model);
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Events));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchCaptureResults(int eventId, List<ParticipantResultCaptureRowViewModel> entrants)
        {
            try
            {
                var (inserted, updated, skipped, error) =
                    await _api.CaptureResultsAsync(eventId, entrants ?? new List<ParticipantResultCaptureRowViewModel>(), CurrentUser.Token);

                if (!string.IsNullOrWhiteSpace(error))
                {
                    TempData["Warning"] = $"{inserted} recorded, {updated} updated, {skipped} skipped. Some rows could not be saved: {error}";
                }
                else
                {
                    TempData["Success"] = $"{inserted} result(s) recorded and {updated} updated. {skipped} row(s) left unchanged.";
                }
            }
            catch (ApiException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Results), new { eventId });
        }
    }
}
