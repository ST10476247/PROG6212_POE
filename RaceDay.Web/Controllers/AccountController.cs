using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Common;
using RaceDay.Web.Models.Api;
using RaceDay.Web.Models.ViewModels;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    /// <summary>
    /// Handles registration, login and logout. All authentication is delegated
    /// to the Part 2 API; the MVC app only stores the returned identity, role
    /// and JWT token in the session.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IRaceDayApiClient _api;

        public AccountController(IRaceDayApiClient api) => _api = api;

        // ─────────────────────────────────────────────────────────
        //  Login
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (HttpContext.Session.GetCurrentUser().IsAuthenticated)
                return RedirectToDashboard();

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var auth = await _api.LoginAsync(new LoginRequest
                {
                    Email = model.Email.Trim(),
                    Password = model.Password
                });

                EstablishSession(auth);
                TempData["Success"] = $"Welcome back, {auth.FullName}!";

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToDashboard();
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Registration (tabbed: Participant / Organiser)
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Register(string? role = null)
        {
            if (HttpContext.Session.GetCurrentUser().IsAuthenticated)
                return RedirectToDashboard();

            ViewBag.ActiveRole = string.Equals(role, "Organiser", StringComparison.OrdinalIgnoreCase)
                ? "Organiser"
                : "Participant";

            return View(new RegisterParticipantViewModel());
        }

        [HttpGet]
        public IActionResult RegisterParticipant()
        {
            if (HttpContext.Session.GetCurrentUser().IsAuthenticated)
                return RedirectToDashboard();

            ViewBag.ActiveRole = "Participant";
            return View("Register", new RegisterParticipantViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterParticipant(RegisterParticipantViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PromoteErrorsToSummary();
                ViewBag.ActiveRole = "Participant";
                return View("Register", model);
            }

            try
            {
                var auth = await _api.RegisterParticipantAsync(new RegisterParticipantRequest
                {
                    Email = model.Email.Trim(),
                    Password = model.Password,
                    FullName = model.FullName.Trim(),
                    PhoneNumber = model.PhoneNumber,
                    Province = model.Province,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    SAIDOrPassport = model.SAIDOrPassport,
                    ClubName = model.ClubName,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactNo = model.EmergencyContactNo,
                    TshirtSize = model.TshirtSize
                });

                EstablishSession(auth);
                TempData["Success"] = $"Your athlete account was created successfully. Welcome to RaceDay, {auth.FullName}!";
                return RedirectToDashboard();
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.ActiveRole = "Participant";
                return View("Register", model);
            }
        }

        [HttpGet]
        public IActionResult RegisterOrganiser()
        {
            if (HttpContext.Session.GetCurrentUser().IsAuthenticated)
                return RedirectToDashboard();

            ViewBag.ActiveRole = "Organiser";
            return View("Register", new RegisterOrganiserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterOrganiser(RegisterOrganiserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PromoteErrorsToSummary();
                ViewBag.ActiveRole = "Organiser";
                return View("Register", model);
            }

            try
            {
                var auth = await _api.RegisterOrganiserAsync(new RegisterOrganiserRequest
                {
                    Email = model.Email.Trim(),
                    Password = model.Password,
                    FullName = model.FullName.Trim(),
                    PhoneNumber = model.PhoneNumber,
                    Province = model.Province,
                    CompanyName = model.CompanyName.Trim(),
                    CompanyRegNo = model.CompanyRegNo,
                    ContactEmail = string.IsNullOrWhiteSpace(model.ContactEmail)
                        ? model.Email.Trim()
                        : model.ContactEmail!.Trim()
                });

                EstablishSession(auth);
                TempData["Success"] = $"Your organiser account was created successfully. Welcome to RaceDay, {auth.FullName}!";
                return RedirectToDashboard();
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.ActiveRole = "Organiser";
                return View("Register", model);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Logout / Access denied
        // ─────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Ask the API to invalidate its side of the session before we forget the token.
                await _api.LogoutAsync();
            }
            catch (ApiException)
            {
                // Even if the API call fails, the local session is still cleared below.
            }

            HttpContext.Session.ClearUserSession();
            TempData["Success"] = "You have been signed out of RaceDay.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // ─────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────
        private void EstablishSession(AuthResponse auth)
        {
            HttpContext.Session.SetUserSession(
                auth.UserId,
                auth.Email,
                auth.FullName,
                auth.Role,
                auth.OrganiserId,
                auth.ParticipantId,
                auth.Token,
                auth.ProfilePictureUrl);
        }

        /// <summary>
        /// The registration view renders a single ModelOnly validation summary,
        /// so surface field-level validation messages there too.
        /// </summary>
        private void PromoteErrorsToSummary()
        {
            var messages = ModelState
                .Where(kvp => kvp.Key != string.Empty)
                .SelectMany(kvp => kvp.Value?.Errors ?? new())
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .ToList();

            foreach (var message in messages)
                ModelState.AddModelError(string.Empty, message);
        }

        private IActionResult RedirectToDashboard()
        {
            var user = HttpContext.Session.GetCurrentUser();
            return user.IsOrganiser
                ? RedirectToAction("Dashboard", "Organiser")
                : RedirectToAction("Home", "Participant");
        }
    }
}
