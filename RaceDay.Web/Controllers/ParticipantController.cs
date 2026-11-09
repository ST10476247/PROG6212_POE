using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Common;
using RaceDay.Web.Filters;
using RaceDay.Web.Models.ViewModels;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [AuthorizeRole("Participant")]
    public class ParticipantController : Controller
    {
        private readonly IRaceDayApiClient _api;

        public ParticipantController(IRaceDayApiClient api)
        {
            _api = api;
        }

        #region Athlete Home / Dashboard

        public async Task<IActionResult> Home()
        {
            var user = HttpContext.Session.GetCurrentUser()!;
            var enrolments = await _api.GetMyEnrolmentsAsync(user.Token);
            var results = await _api.GetMyResultsAsync(user.Token);

            var nextEvent = enrolments
                .Where(e => e.IsEventUpcoming && !e.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.EventDate)
                .FirstOrDefault();

            ViewBag.NextEvent = nextEvent;
            ViewBag.TotalEnrolled = enrolments.Count;
            ViewBag.TotalCompleted = results.TotalRacesCompleted;
            ViewBag.TotalKm = results.TotalKilometersRaced;
            ViewBag.RecentEnrolments = enrolments.Take(3).ToList();
            ViewBag.RecentResults = results.Results.Take(3).ToList();

            var filter = new EventFilterViewModel { PageSize = 6 };
            var upcoming = await _api.GetEventsAsync(filter, user.Token);
            ViewBag.UpcomingEvents = upcoming.Items;

            return View();
        }

        #endregion

        #region Enter Event (Enrolment)

        [HttpGet]
        public async Task<IActionResult> Enrol(int eventId, int? categoryId = null)
        {
            var user = HttpContext.Session.GetCurrentUser()!;
            var ev = await _api.GetEventDetailsAsync(eventId, user.Token, user.ParticipantId);

            if (ev == null)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction("Index", "Events");
            }

            if (ev.IsUserEnrolled)
            {
                TempData["Warning"] = "You are already enrolled for this event!";
                return RedirectToAction("MyEvents");
            }

            // Preload user's profile to pre-fill emergency contacts and tshirt size
            var profile = await _api.GetProfileAsync(user.Token);

            var model = new EnrolViewModel
            {
                EventId = ev.EventId,
                EventTitle = ev.Title,
                City = ev.City,
                Province = ev.Province,
                StartDate = ev.StartDate,
                BannerUrl = ev.BannerUrl,
                AvailableCategories = ev.Categories,
                CategoryId = categoryId ?? ev.Categories.FirstOrDefault()?.CategoryId ?? 0,
                ClubName = profile?.ClubName,
                EmergencyContactName = profile?.EmergencyContactName ?? "",
                EmergencyContactNo = profile?.EmergencyContactNo ?? "",
                TshirtSize = profile?.TshirtSize ?? "M",
                AcceptTerms = false
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(EnrolViewModel model)
        {
            var user = HttpContext.Session.GetCurrentUser()!;

            if (!ModelState.IsValid)
            {
                var ev = await _api.GetEventDetailsAsync(model.EventId, user.Token);
                model.AvailableCategories = ev?.Categories ?? new();
                return View(model);
            }

            var (success, enrolmentId, bibNumber, error) = await _api.EnrolAsync(model, user.Token);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Enrolment failed. Please try again.");
                var ev = await _api.GetEventDetailsAsync(model.EventId, user.Token);
                model.AvailableCategories = ev?.Categories ?? new();
                return View(model);
            }

            TempData["Success"] = $"Congratulations! You are officially enrolled in '{model.EventTitle}'. Your assigned Race Bib is #{bibNumber}!";
            return RedirectToAction("MyEvents");
        }

        #endregion

        #region Personal Enrolments View

        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            var user = HttpContext.Session.GetCurrentUser()!;
            var enrolments = await _api.GetMyEnrolmentsAsync(user.Token);
            return View(enrolments);
        }

        #endregion

        #region My Results Page

        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var user = HttpContext.Session.GetCurrentUser()!;
            var vm = await _api.GetMyResultsAsync(user.Token);
            return View(vm);
        }

        #endregion

        #region Profile & Azure Profile Picture Upload

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = HttpContext.Session.GetCurrentUser()!;
            var profile = await _api.GetProfileAsync(user.Token);

            if (profile == null)
            {
                TempData["Error"] = "Could not load profile.";
                return RedirectToAction("Home");
            }

            var editModel = new EditProfileViewModel
            {
                FullName = profile.FullName,
                PhoneNumber = profile.PhoneNumber,
                Province = profile.Province,
                ClubName = profile.ClubName,
                EmergencyContactName = profile.EmergencyContactName,
                EmergencyContactNo = profile.EmergencyContactNo,
                TshirtSize = profile.TshirtSize ?? "M",
                CompanyName = profile.CompanyName,
                CompanyRegNo = profile.CompanyRegNo,
                ContactEmail = profile.ContactEmail,
                ExistingProfilePictureUrl = profile.ProfilePictureUrl
            };

            ViewBag.Profile = profile;
            ViewBag.ChangePassword = new ChangePasswordViewModel();
            return View(editModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(EditProfileViewModel model)
        {
            var user = HttpContext.Session.GetCurrentUser()!;

            if (!ModelState.IsValid)
            {
                var profile = await _api.GetProfileAsync(user.Token);
                ViewBag.Profile = profile;
                ViewBag.ChangePassword = new ChangePasswordViewModel();
                return View("Profile", model);
            }

            var (success, error) = await _api.UpdateProfileAsync(model, user.Token);
            if (!success)
            {
                TempData["Error"] = error ?? "Failed to update profile.";
            }
            else
            {
                // Refresh session full name
                HttpContext.Session.SetString(SessionKeys.FullName, model.FullName);
                TempData["Success"] = "Profile details updated successfully!";
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = HttpContext.Session.GetCurrentUser()!;

            if (!ModelState.IsValid)
            {
                var profile = await _api.GetProfileAsync(user.Token);
                var editModel = new EditProfileViewModel
                {
                    FullName = profile?.FullName ?? "",
                    PhoneNumber = profile?.PhoneNumber,
                    Province = profile?.Province,
                    ClubName = profile?.ClubName,
                    EmergencyContactName = profile?.EmergencyContactName,
                    EmergencyContactNo = profile?.EmergencyContactNo,
                    TshirtSize = profile?.TshirtSize ?? "M",
                    ExistingProfilePictureUrl = profile?.ProfilePictureUrl
                };
                ViewBag.Profile = profile;
                ViewBag.ChangePassword = model;
                return View("Profile", editModel);
            }

            var (success, error) = await _api.ChangePasswordAsync(model, user.Token);
            if (!success)
            {
                TempData["Error"] = error ?? "Could not change password. Please check your current password.";
            }
            else
            {
                TempData["Success"] = "Your password has been changed successfully.";
            }

            return RedirectToAction("Profile");
        }

        #endregion
    }
}

