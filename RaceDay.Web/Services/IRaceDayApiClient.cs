using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RaceDay.Web.Models.Api;
using RaceDay.Web.Models.ViewModels;

namespace RaceDay.Web.Services
{
    /// <summary>
    /// Typed abstraction over the RaceDay Part 2 REST API.
    ///
    /// The MVC application performs ALL data operations through this interface.
    /// It never touches the database or Azure Blob Storage directly — every read
    /// and write is delegated to the API, and the API DTOs are mapped to the
    /// view models used by the Razor views.
    /// </summary>
    public interface IRaceDayApiClient
    {
        // ── Authentication ────────────────────────────────────────
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterParticipantAsync(RegisterParticipantRequest request);
        Task<AuthResponse> RegisterOrganiserAsync(RegisterOrganiserRequest request);
        Task LogoutAsync();

        // ── Public event directory ────────────────────────────────
        Task<EventFilterViewModel> GetEventsAsync(EventFilterViewModel filter, string? token = null);
        Task<List<EventCardViewModel>> GetFeaturedEventsAsync();
        Task<EventDetailsViewModel?> GetEventDetailsAsync(int eventId, string? token = null, int? participantId = null);

        // ── Participant experience ────────────────────────────────
        Task<List<EnrolmentCardViewModel>> GetMyEnrolmentsAsync(string? token = null);
        Task<MyResultsViewModel> GetMyResultsAsync(string? token = null);
        Task<(bool Success, int EnrolmentId, string BibNumber, string? Error)> EnrolAsync(EnrolViewModel model, string? token = null);

        // ── Profile ───────────────────────────────────────────────
        Task<ProfileViewModel?> GetProfileAsync(string? token = null);
        Task<(bool Success, string? Error)> UpdateProfileAsync(EditProfileViewModel model, string? token = null);
        Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordViewModel model, string? token = null);

        // ── Organiser experience ──────────────────────────────────
        Task<OrganiserDashboardViewModel> GetOrganiserDashboardAsync(int organiserId, string? token = null);
        Task<List<OrganiserEventSummaryViewModel>> GetOrganiserEventsAsync(int organiserId, string? token = null);
        Task<int> CreateEventAsync(CreateEventViewModel model, string? token = null);
        Task UpdateEventAsync(int eventId, EditEventViewModel model, string? token = null);
        Task DeleteEventAsync(int eventId, string? token = null);

        Task<List<CategoryViewModel>> GetCategoriesAsync(int eventId);
        Task AddCategoryAsync(int eventId, CreateCategoryViewModel model, string? token = null);
        Task DeleteCategoryAsync(int categoryId, string? token = null);

        Task<EventEnrolmentListViewModel> GetEventEnrolmentsAsync(int eventId, string? token = null);
        Task UpdateEnrolmentStatusAsync(int enrolmentId, string status, string? token = null);

        Task<EventResultsEntryViewModel> GetEventResultsEntryAsync(int eventId, string? token = null);
        Task<(int Inserted, int Updated, int Skipped, string? Error)> CaptureResultsAsync(int eventId, List<ParticipantResultCaptureRowViewModel> entrants, string? token = null);

        // ── Cloud file uploads (API → Azure Blob Storage) ─────────
        Task<string?> UploadEventBannerAsync(Stream stream, string fileName, string contentType, string? token = null);
        Task<string?> UploadProfilePictureAsync(Stream stream, string fileName, string contentType, string? token = null);
    }
}
