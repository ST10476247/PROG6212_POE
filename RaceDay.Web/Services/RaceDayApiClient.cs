using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RaceDay.Web.Common;
using RaceDay.Web.Models.Api;
using RaceDay.Web.Models.ViewModels;

namespace RaceDay.Web.Services
{
    /// <summary>
    /// Default <see cref="IRaceDayApiClient"/> implementation.
    ///
    /// Every outbound request carries the JWT bearer token that was issued by
    /// the API at login and stored in the user's MVC session. This lets the API
    /// resolve the caller's identity and role without the MVC app holding any
    /// database or Azure credentials of its own. API DTOs are mapped to the
    /// view models consumed by the Razor views.
    /// </summary>
    public class RaceDayApiClient : IRaceDayApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

        public RaceDayApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        // ─────────────────────────────────────────────────────────
        //  Authentication
        // ─────────────────────────────────────────────────────────
        public Task<AuthResponse> LoginAsync(LoginRequest request)
            => SendJsonAsync<LoginRequest, AuthResponse>(HttpMethod.Post, "api/auth/login", request);

        public Task<AuthResponse> RegisterParticipantAsync(RegisterParticipantRequest request)
            => SendJsonAsync<RegisterParticipantRequest, AuthResponse>(HttpMethod.Post, "api/auth/register/participant", request);

        public Task<AuthResponse> RegisterOrganiserAsync(RegisterOrganiserRequest request)
            => SendJsonAsync<RegisterOrganiserRequest, AuthResponse>(HttpMethod.Post, "api/auth/register/organiser", request);

        public Task LogoutAsync()
            => SendNoContentAsync(HttpMethod.Post, "api/auth/logout");

        // ─────────────────────────────────────────────────────────
        //  Public event directory
        // ─────────────────────────────────────────────────────────
        public async Task<EventFilterViewModel> GetEventsAsync(EventFilterViewModel filter, string? token = null)
        {
            var sb = new StringBuilder("api/events?");
            sb.Append("page=").Append(filter.Page < 1 ? 1 : filter.Page);
            sb.Append("&pageSize=").Append(filter.PageSize < 1 ? 12 : filter.PageSize);
            if (!string.IsNullOrWhiteSpace(filter.City)) sb.Append("&city=").Append(Uri.EscapeDataString(filter.City));
            if (!string.IsNullOrWhiteSpace(filter.Province)) sb.Append("&province=").Append(Uri.EscapeDataString(filter.Province));
            if (!string.IsNullOrWhiteSpace(filter.EventType)) sb.Append("&eventType=").Append(Uri.EscapeDataString(filter.EventType));
            if (!string.IsNullOrWhiteSpace(filter.Status)) sb.Append("&status=").Append(Uri.EscapeDataString(filter.Status));
            if (!string.IsNullOrWhiteSpace(filter.Search)) sb.Append("&search=").Append(Uri.EscapeDataString(filter.Search));
            if (filter.From.HasValue) sb.Append("&from=").Append(filter.From.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            if (filter.To.HasValue) sb.Append("&to=").Append(filter.To.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            var page = await GetAsync<PagedResult<EventSummary>>(sb.ToString(), token);
            filter.Items = page.Items.Select(MapEventCard).ToList();
            filter.TotalCount = page.TotalCount;
            return filter;
        }

        public async Task<List<EventCardViewModel>> GetFeaturedEventsAsync()
        {
            var events = await GetAsync<List<EventSummary>>("api/events/featured");
            return events.Select(MapEventCard).ToList();
        }

        public async Task<EventDetailsViewModel?> GetEventDetailsAsync(int eventId, string? token = null, int? participantId = null)
        {
            EventDetail? detail;
            try
            {
                detail = await GetAsync<EventDetail>($"api/events/{eventId}", token);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return null;
            }

            var model = MapEventDetails(detail);

            // Determine whether the current athlete has already entered this event.
            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var mine = await GetAsync<List<EnrolmentDto>>("api/enrolments/my", token);
                    var entry = mine.FirstOrDefault(e => e.EventId == eventId);
                    if (entry != null)
                    {
                        model.IsUserEnrolled = true;
                        model.UserEnrolmentId = entry.EnrolmentId;
                        model.UserEnrolmentStatus = MapStatus(entry.Status);
                    }
                }
                catch (ApiException)
                {
                    // Non-fatal — render the event without enrolment context.
                }
            }

            return model;
        }

        // ─────────────────────────────────────────────────────────
        //  Participant experience
        // ─────────────────────────────────────────────────────────
        public async Task<List<EnrolmentCardViewModel>> GetMyEnrolmentsAsync(string? token = null)
        {
            var enrolments = await GetAsync<List<EnrolmentDto>>("api/enrolments/my", token);
            return enrolments.Select(MapEnrolmentCard).ToList();
        }

        public async Task<MyResultsViewModel> GetMyResultsAsync(string? token = null)
        {
            var results = await GetAsync<List<ResultDto>>("api/results/my", token);

            var items = results.Select(MapPersonalResult).ToList();

            var model = new MyResultsViewModel
            {
                Results = items,
                TotalRacesCompleted = items.Count(i => string.Equals(i.Status, "Finished", StringComparison.OrdinalIgnoreCase)),
                TotalKilometersRaced = items.Where(i => string.Equals(i.Status, "Finished", StringComparison.OrdinalIgnoreCase)).Sum(i => i.DistanceKm)
            };

            var best = items.Where(i => i.OverallPosition > 0).Select(i => (int?)i.OverallPosition).Min();
            if (best.HasValue)
            {
                model.BestFinishPosition = $"{best.Value}{PersonalRaceResultItemViewModel.GetOrdinalSuffix(best.Value)}";
            }

            return model;
        }

        public async Task<(bool Success, int EnrolmentId, string BibNumber, string? Error)> EnrolAsync(EnrolViewModel model, string? token = null)
        {
            try
            {
                var enrolment = await SendJsonAsync<CreateEnrolmentRequest, EnrolmentDto>(
                    HttpMethod.Post,
                    "api/enrolments",
                    new CreateEnrolmentRequest
                    {
                        CategoryId = model.CategoryId,
                        EmergencyNotes = string.IsNullOrWhiteSpace(model.MedicalAidDetails) ? null : model.MedicalAidDetails
                    },
                    token);

                return (true, enrolment.EnrolmentId, enrolment.BibNumber ?? string.Empty, null);
            }
            catch (ApiException ex)
            {
                return (false, 0, string.Empty, ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Profile
        // ─────────────────────────────────────────────────────────
        public async Task<ProfileViewModel?> GetProfileAsync(string? token = null)
        {
            try
            {
                var profile = await GetAsync<UserProfile>("api/profile/me", token);
                return MapProfile(profile);
            }
            catch (ApiException ex) when (ex.StatusCode == 404)
            {
                return null;
            }
        }

        public async Task<(bool Success, string? Error)> UpdateProfileAsync(EditProfileViewModel model, string? token = null)
        {
            try
            {
                string? pictureUrl = model.ExistingProfilePictureUrl;

                if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
                {
                    using var stream = model.ProfilePictureFile.OpenReadStream();
                    var uploaded = await PostFileForUrlAsync(
                        "api/profile/picture", stream,
                        model.ProfilePictureFile.FileName, model.ProfilePictureFile.ContentType,
                        "profilePictureUrl", token);

                    if (!string.IsNullOrWhiteSpace(uploaded))
                        pictureUrl = uploaded;
                }

                await SendJsonAsync(HttpMethod.Put, "api/profile/me", new UpdateProfileRequest
                {
                    FullName = model.FullName?.Trim(),
                    PhoneNumber = model.PhoneNumber,
                    Province = model.Province,
                    ProfilePictureUrl = pictureUrl,
                    CompanyName = model.CompanyName,
                    CompanyRegNo = model.CompanyRegNo,
                    ContactEmail = model.ContactEmail,
                    ClubName = model.ClubName,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactNo = model.EmergencyContactNo,
                    TshirtSize = model.TshirtSize
                }, token);

                return (true, null);
            }
            catch (ApiException ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordViewModel model, string? token = null)
        {
            try
            {
                await SendJsonAsync(HttpMethod.Put, "api/profile/change-password", new ChangePasswordRequest
                {
                    CurrentPassword = model.CurrentPassword,
                    NewPassword = model.NewPassword
                }, token);

                return (true, null);
            }
            catch (ApiException ex)
            {
                return (false, ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Organiser experience
        // ─────────────────────────────────────────────────────────
        public async Task<OrganiserDashboardViewModel> GetOrganiserDashboardAsync(int organiserId, string? token = null)
        {
            var events = await GetOrganiserEventsAsync(organiserId, token);

            var model = new OrganiserDashboardViewModel
            {
                OrganiserId = organiserId,
                Events = events,
                UpcomingEvents = events.Where(e => e.IsUpcoming).OrderBy(e => e.StartDate).Take(5).ToList(),
                TotalEventsCreated = events.Count,
                TotalEnrolmentsAcrossAllEvents = events.Sum(e => e.TotalEnrolments),
                UpcomingEventsCount = events.Count(e => e.IsUpcoming),
                CompletedEventsCount = events.Count(e => string.Equals(e.Status, "Completed", StringComparison.OrdinalIgnoreCase)),
                TotalRevenueZar = events.Sum(e => e.RevenueZar)
            };

            var profile = await GetProfileAsync(token);
            if (profile != null)
            {
                model.CompanyName = profile.CompanyName ?? profile.FullName;
                model.ContactEmail = profile.ContactEmail ?? profile.Email;
            }

            return model;
        }

        public async Task<List<OrganiserEventSummaryViewModel>> GetOrganiserEventsAsync(int organiserId, string? token = null)
        {
            var events = await GetAsync<List<EventSummary>>($"api/organisers/{organiserId}/events", token);
            return events.Select(MapOrganiserEvent).ToList();
        }

        public async Task<int> CreateEventAsync(CreateEventViewModel model, string? token = null)
        {
            string? bannerUrl = model.BannerUrl;
            if (model.BannerImageFile != null && model.BannerImageFile.Length > 0)
            {
                bannerUrl = await UploadEventBannerAsync(model.BannerImageFile, token) ?? bannerUrl;
            }

            var created = await SendJsonAsync<EventRequest, CreateEventResponse>(HttpMethod.Post, "api/events", new EventRequest
            {
                Title = model.Title.Trim(),
                EventType = model.EventType,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                City = model.City.Trim(),
                Province = model.Province.Trim(),
                RouteUrl = model.RouteUrl,
                EntryFeeZar = model.EntryFeeZar,
                MaxEntries = model.MaxEntries,
                RegistrationOpen = model.RegistrationOpen,
                RegistrationClose = model.RegistrationClose,
                Status = model.Status,
                ElevationGainMeters = model.ElevationGainMeters,
                KeyLandmarks = model.KeyLandmarks,
                TerrainType = model.TerrainType,
                BannerUrl = bannerUrl
            }, token);

            return created.EventId;
        }

        public async Task UpdateEventAsync(int eventId, EditEventViewModel model, string? token = null)
        {
            string? bannerUrl = string.IsNullOrWhiteSpace(model.BannerUrl) ? model.ExistingBannerUrl : model.BannerUrl;
            if (model.BannerImageFile != null && model.BannerImageFile.Length > 0)
            {
                bannerUrl = await UploadEventBannerAsync(model.BannerImageFile, token) ?? bannerUrl;
            }

            await SendJsonAsync(HttpMethod.Put, $"api/events/{eventId}", new EventRequest
            {
                Title = model.Title.Trim(),
                EventType = model.EventType,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                City = model.City.Trim(),
                Province = model.Province.Trim(),
                RouteUrl = model.RouteUrl,
                EntryFeeZar = model.EntryFeeZar,
                MaxEntries = model.MaxEntries,
                RegistrationOpen = model.RegistrationOpen,
                RegistrationClose = model.RegistrationClose,
                Status = model.Status,
                ElevationGainMeters = model.ElevationGainMeters,
                KeyLandmarks = model.KeyLandmarks,
                TerrainType = model.TerrainType,
                BannerUrl = bannerUrl
            }, token);
        }

        public Task DeleteEventAsync(int eventId, string? token = null)
            => SendNoContentAsync(HttpMethod.Delete, $"api/events/{eventId}", token);

        public async Task<List<CategoryViewModel>> GetCategoriesAsync(int eventId)
        {
            var categories = await GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories");
            return categories.Select(c => MapCategory(c, string.Empty)).ToList();
        }

        public Task AddCategoryAsync(int eventId, CreateCategoryViewModel model, string? token = null)
            => SendJsonAsync(HttpMethod.Post, $"api/events/{eventId}/categories", new CategoryRequest
            {
                CategoryName = model.CategoryName.Trim(),
                DistanceKm = model.DistanceKm,
                Gender = string.IsNullOrWhiteSpace(model.GenderRestriction) || model.GenderRestriction == "None"
                    ? "All"
                    : model.GenderRestriction,
                MinAgeYears = model.MinAge,
                MaxAgeYears = model.MaxAge,
                StartTime = model.StartTime,
                EntryFeeZar = model.EntryFeeZar ?? 0m,
                MaxCapacity = model.MaxParticipants
            }, token);

        public Task DeleteCategoryAsync(int categoryId, string? token = null)
            => SendNoContentAsync(HttpMethod.Delete, $"api/categories/{categoryId}", token);

        public async Task<EventEnrolmentListViewModel> GetEventEnrolmentsAsync(int eventId, string? token = null)
        {
            var ev = await GetAsync<EventDetail>($"api/events/{eventId}", token);
            var enrolments = await GetAsync<List<EnrolmentDto>>($"api/events/{eventId}/enrolments", token);
            var categories = await GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories", token);

            var rows = enrolments.Select(MapEnrolmentRow).ToList();

            return new EventEnrolmentListViewModel
            {
                EventId = ev.EventId,
                EventTitle = ev.Title,
                EventDate = ev.StartDate,
                City = ev.City,
                Province = ev.Province,
                MaxEntries = ev.MaxEntries,
                TotalEnrolments = enrolments.Count,
                Categories = categories.Select(c => MapCategory(c, ev.Title)).ToList(),
                Enrolments = rows,
                ConfirmedCount = rows.Count(r => string.Equals(r.Status, "Confirmed", StringComparison.OrdinalIgnoreCase)),
                PendingCount = rows.Count(r => string.Equals(r.Status, "Pending", StringComparison.OrdinalIgnoreCase)),
                CancelledCount = rows.Count(r => string.Equals(r.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            };
        }

        public Task UpdateEnrolmentStatusAsync(int enrolmentId, string status, string? token = null)
            => SendJsonAsync(HttpMethod.Put, $"api/enrolments/{enrolmentId}", new UpdateEnrolmentStatusRequest
            {
                Status = MapUiStatusToApi(status)
            }, token);

        public async Task<EventResultsEntryViewModel> GetEventResultsEntryAsync(int eventId, string? token = null)
        {
            var ev = await GetAsync<EventDetail>($"api/events/{eventId}", token);
            var enrolments = await GetAsync<List<EnrolmentDto>>($"api/events/{eventId}/enrolments", token);
            var results = await GetAsync<List<ResultDto>>($"api/events/{eventId}/results", token);

            var rows = enrolments.Select(en =>
            {
                var result = results.FirstOrDefault(r => r.EnrolmentId == en.EnrolmentId);
                return new ParticipantResultCaptureRowViewModel
                {
                    EnrolmentId = en.EnrolmentId,
                    ResultId = result?.ResultId,
                    BibNumber = ParseBibNumber(en.BibNumber),
                    ParticipantName = en.ParticipantName,
                    CategoryName = en.CategoryName,
                    DistanceKm = en.DistanceKm,
                    FinishTimeString = result?.FinishTimeSec.HasValue == true
                        ? FormatSeconds(result!.FinishTimeSec!.Value)
                        : null,
                    Position = result?.GunPosition,
                    Status = result != null ? MapResultStatus(result.Status) : "Finished",
                    IsPublished = result != null
                };
            }).ToList();

            return new EventResultsEntryViewModel
            {
                EventId = ev.EventId,
                EventTitle = ev.Title,
                EventDate = ev.StartDate,
                City = ev.City,
                Status = ev.Status,
                Entrants = rows,
                TotalEntrants = rows.Count,
                RecordedResultsCount = rows.Count(r => r.IsPublished)
            };
        }

        public async Task<(int Inserted, int Updated, int Skipped, string? Error)> CaptureResultsAsync(int eventId, List<ParticipantResultCaptureRowViewModel> entrants, string? token = null)
        {
            int inserted = 0, updated = 0, skipped = 0;
            var errors = new List<string>();

            foreach (var row in entrants)
            {
                bool hasTime = !string.IsNullOrWhiteSpace(row.FinishTimeString);
                bool isNonFinish = !string.Equals(row.Status, "Finished", StringComparison.OrdinalIgnoreCase);

                // Nothing entered and no change requested — leave the row untouched.
                if (!row.ResultId.HasValue && !hasTime && !isNonFinish)
                {
                    skipped++;
                    continue;
                }

                int? seconds = ParseFinishTimeSeconds(row.FinishTimeString);

                try
                {
                    if (row.ResultId.HasValue)
                    {
                        await SendJsonAsync(HttpMethod.Put, $"api/results/{row.ResultId.Value}", new UpdateResultItemRequest
                        {
                            EnrolmentId = row.EnrolmentId,
                            FinishTimeSec = seconds,
                            GunPosition = row.Position,
                            Status = string.IsNullOrWhiteSpace(row.Status) ? "Finished" : row.Status
                        }, token);
                        updated++;
                    }
                    else
                    {
                        await SendJsonAsync(HttpMethod.Post, "api/results", new CreateResultRequest
                        {
                            EnrolmentId = row.EnrolmentId,
                            FinishTimeSec = seconds,
                            GunPosition = row.Position,
                            Status = string.IsNullOrWhiteSpace(row.Status) ? "Finished" : row.Status
                        }, token);
                        inserted++;
                    }
                }
                catch (ApiException ex)
                {
                    errors.Add($"{row.ParticipantName}: {ex.Message}");
                }
            }

            return (inserted, updated, skipped, errors.Count > 0 ? string.Join(" | ", errors) : null);
        }

        // ─────────────────────────────────────────────────────────
        //  Cloud file uploads (API → Azure Blob Storage)
        // ─────────────────────────────────────────────────────────
        public async Task<string?> UploadEventBannerAsync(Stream stream, string fileName, string contentType, string? token = null)
        {
            var upload = await PostFileAsync<UploadResponse>("api/upload/banner", stream, fileName, contentType, token);
            return string.IsNullOrWhiteSpace(upload.Url) ? null : upload.Url;
        }

        public async Task<string?> UploadProfilePictureAsync(Stream stream, string fileName, string contentType, string? token = null)
        {
            string url = await PostFileForUrlAsync("api/profile/picture", stream, fileName, contentType, "profilePictureUrl", token);
            return string.IsNullOrWhiteSpace(url) ? null : url;
        }

        private async Task<string?> UploadEventBannerAsync(IFormFile file, string? token)
        {
            using var stream = file.OpenReadStream();
            return await UploadEventBannerAsync(stream, file.FileName, file.ContentType, token);
        }

        // ─────────────────────────────────────────────────────────
        //  Mapping helpers — API DTO → view model
        // ─────────────────────────────────────────────────────────
        private static EventCardViewModel MapEventCard(EventSummary e) => new()
        {
            EventId = e.EventId,
            OrganiserId = e.OrganiserId,
            OrganiserName = e.OrganiserName,
            Title = e.Title,
            EventType = e.EventType,
            Description = e.Description,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            City = e.City,
            Province = e.Province,
            EntryFeeZar = e.EntryFeeZar,
            MaxEntries = e.MaxEntries,
            Status = e.Status,
            BannerUrl = e.BannerUrl,
            CategoriesCount = e.CategoriesCount,
            EntrantsCount = e.EntrantsCount
        };

        private static EventDetailsViewModel MapEventDetails(EventDetail d) => new()
        {
            EventId = d.EventId,
            OrganiserId = d.OrganiserId,
            OrganiserName = d.OrganiserName,
            Title = d.Title,
            EventType = d.EventType,
            Description = d.Description,
            StartDate = d.StartDate,
            EndDate = d.EndDate,
            City = d.City,
            Province = d.Province,
            RouteUrl = d.RouteUrl,
            EntryFeeZar = d.EntryFeeZar,
            MaxEntries = d.MaxEntries,
            RegistrationOpen = d.RegistrationOpen,
            RegistrationClose = d.RegistrationClose,
            Status = d.Status,
            ElevationGainMeters = d.ElevationGainMeters,
            KeyLandmarks = d.KeyLandmarks,
            TerrainType = d.TerrainType,
            BannerUrl = d.BannerUrl,
            Categories = d.Categories.Select(c => MapCategory(c, d.Title)).ToList(),
            EntrantsCount = d.EntrantsCount
        };

        private static CategoryViewModel MapCategory(CategoryDto c, string eventTitle) => new()
        {
            CategoryId = c.CategoryId,
            EventId = c.EventId,
            EventTitle = eventTitle,
            CategoryName = c.CategoryName,
            DistanceKm = c.DistanceKm,
            MinAge = c.MinAgeYears,
            MaxAge = c.MaxAgeYears,
            GenderRestriction = c.Gender,
            EntryFeeZar = c.EntryFeeZar,
            MaxParticipants = c.MaxCapacity,
            StartTime = c.StartTime,
            EnrolmentsCount = c.EnrolledCount
        };

        private static EnrolmentCardViewModel MapEnrolmentCard(EnrolmentDto d) => new()
        {
            EnrolmentId = d.EnrolmentId,
            EventId = d.EventId,
            EventTitle = d.EventTitle,
            EventType = string.IsNullOrWhiteSpace(d.EventType) ? "Running" : d.EventType,
            EventDate = d.EventDate,
            City = d.City,
            Province = d.Province,
            BannerUrl = d.BannerUrl,
            CategoryId = d.CategoryId,
            CategoryName = d.CategoryName,
            DistanceKm = d.DistanceKm,
            BibNumber = ParseBibNumber(d.BibNumber),
            EnrolmentDate = d.EnrolmentDate,
            Status = MapStatus(d.Status),
            FeePaidZar = string.Equals(d.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase) ? d.EntryFeeZar : 0m,
            Notes = d.EmergencyNotes,
            HasResult = d.Result != null,
            ResultId = d.Result?.ResultId
        };

        private static ParticipantEnrolmentRowViewModel MapEnrolmentRow(EnrolmentDto d) => new()
        {
            EnrolmentId = d.EnrolmentId,
            ParticipantId = d.ParticipantId,
            ParticipantName = d.ParticipantName,
            ParticipantEmail = d.ParticipantEmail,
            PhoneNumber = d.PhoneNumber,
            ClubName = d.ClubName,
            CategoryName = d.CategoryName,
            DistanceKm = d.DistanceKm,
            BibNumber = ParseBibNumber(d.BibNumber),
            EnrolmentDate = d.EnrolmentDate,
            Status = MapStatus(d.Status),
            FeePaidZar = string.Equals(d.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase) ? d.EntryFeeZar : 0m,
            TshirtSize = d.TshirtSize,
            EmergencyContact = d.EmergencyContact
        };

        private static PersonalRaceResultItemViewModel MapPersonalResult(ResultDto r) => new()
        {
            ResultId = r.ResultId,
            EventId = r.EventId,
            EventTitle = r.EventTitle,
            EventType = string.IsNullOrWhiteSpace(r.EventType) ? "Running" : r.EventType,
            EventDate = r.EventDate,
            City = r.City,
            Province = r.Province,
            BannerUrl = r.BannerUrl,
            CategoryName = r.CategoryName,
            DistanceKm = r.DistanceKm,
            BibNumber = ParseBibNumber(r.BibNumber),
            GunTime = TimeSpan.FromSeconds(r.FinishTimeSec ?? 0),
            ChipTime = r.ChipTimeSec.HasValue ? TimeSpan.FromSeconds(r.ChipTimeSec.Value) : null,
            OverallPosition = r.GunPosition ?? 0,
            CategoryPosition = r.CategoryPosition,
            GenderPosition = r.GenderPosition,
            TotalFinishers = r.TotalFinishers,
            Status = MapResultStatus(r.Status)
        };

        private static ProfileViewModel MapProfile(UserProfile p) => new()
        {
            UserId = p.UserId,
            Email = p.Email,
            FullName = p.FullName,
            Role = p.Role,
            PhoneNumber = p.PhoneNumber,
            Province = p.Province,
            ProfilePictureUrl = p.ProfilePictureUrl,
            CreatedAt = p.CreatedAt,
            OrganiserId = p.OrganiserProfile?.OrganiserId,
            CompanyName = p.OrganiserProfile?.CompanyName,
            CompanyRegNo = p.OrganiserProfile?.CompanyRegNo,
            ContactEmail = p.OrganiserProfile?.ContactEmail,
            IsVerified = p.OrganiserProfile?.IsVerified ?? false,
            ParticipantId = p.ParticipantProfile?.ParticipantId,
            DateOfBirth = p.ParticipantProfile?.DateOfBirth,
            Gender = p.ParticipantProfile?.Gender,
            SAIDOrPassport = p.ParticipantProfile?.SAIDOrPassport,
            ClubName = p.ParticipantProfile?.ClubName,
            EmergencyContactName = p.ParticipantProfile?.EmergencyContactName,
            EmergencyContactNo = p.ParticipantProfile?.EmergencyContactNo,
            TshirtSize = p.ParticipantProfile?.TshirtSize
        };

        private static OrganiserEventSummaryViewModel MapOrganiserEvent(EventSummary e) => new()
        {
            EventId = e.EventId,
            Title = e.Title,
            EventType = e.EventType,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            City = e.City,
            Province = e.Province,
            EntryFeeZar = e.EntryFeeZar,
            MaxEntries = e.MaxEntries,
            Status = e.Status,
            BannerUrl = e.BannerUrl,
            CategoriesCount = e.CategoriesCount,
            TotalEnrolments = e.EntrantsCount,
            ConfirmedEnrolments = e.EntrantsCount
        };

        // ─────────────────────────────────────────────────────────
        //  Small conversion helpers
        // ─────────────────────────────────────────────────────────
        private static int ParseBibNumber(string? bib)
        {
            if (string.IsNullOrWhiteSpace(bib)) return 0;
            var digits = new string(bib.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var value) ? value : 0;
        }

        private static string MapStatus(string? apiStatus) => apiStatus?.Trim() switch
        {
            "Registered" => "Pending",
            "Pending" => "Pending",
            "Withdrawn" => "Cancelled",
            "Cancelled" => "Cancelled",
            _ => "Confirmed"
        };

        private static string MapUiStatusToApi(string? uiStatus) => uiStatus?.Trim() switch
        {
            "Confirmed" => "Started",
            "Pending" => "Registered",
            "Cancelled" => "Withdrawn",
            _ => "Registered"
        };

        private static string MapResultStatus(string? apiStatus) => apiStatus?.Trim() switch
        {
            "DNF" => "DNF",
            "DNS" => "DNS",
            "DQ" => "DQ",
            "Disqualified" => "DQ",
            _ => "Finished"
        };

        private static string FormatSeconds(int seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        private static int? ParseFinishTimeSeconds(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var parts = value.Trim().Split(':');
            int h = 0, m, s;

            if (parts.Length == 3)
            {
                if (!int.TryParse(parts[0], out h)) return null;
                if (!int.TryParse(parts[1], out m)) return null;
                if (!int.TryParse(parts[2], out s)) return null;
            }
            else if (parts.Length == 2)
            {
                if (!int.TryParse(parts[0], out m)) return null;
                if (!int.TryParse(parts[1], out s)) return null;
            }
            else
            {
                return null;
            }

            return (h * 3600) + (m * 60) + s;
        }

        // ─────────────────────────────────────────────────────────
        //  Core plumbing
        // ─────────────────────────────────────────────────────────
        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, string? token = null)
        {
            string? bearer = !string.IsNullOrWhiteSpace(token)
                ? token
                : _httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.Token);

            if (!string.IsNullOrWhiteSpace(bearer))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
            }

            return await _httpClient.SendAsync(request);
        }

        private async Task<T> GetAsync<T>(string url, string? token = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await SendAsync(request, token);

            if (!response.IsSuccessStatusCode)
                throw await BuildExceptionAsync(response);

            var value = await response.Content.ReadFromJsonAsync<T>(JsonOpts);
            if (value is null)
                throw new ApiException((int)response.StatusCode, "The RaceDay API returned an unexpected empty response.");
            return value;
        }

        private async Task<TResponse> SendJsonAsync<TRequest, TResponse>(HttpMethod method, string url, TRequest payload, string? token = null)
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(payload, options: JsonOpts)
            };
            using var response = await SendAsync(request, token);

            if (!response.IsSuccessStatusCode)
                throw await BuildExceptionAsync(response);

            var value = await response.Content.ReadFromJsonAsync<TResponse>(JsonOpts);
            if (value is null)
                throw new ApiException((int)response.StatusCode, "The RaceDay API returned an unexpected empty response.");
            return value;
        }

        private async Task SendJsonAsync<TRequest>(HttpMethod method, string url, TRequest payload, string? token = null)
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(payload, options: JsonOpts)
            };
            using var response = await SendAsync(request, token);

            if (!response.IsSuccessStatusCode)
                throw await BuildExceptionAsync(response);
        }

        private async Task SendNoContentAsync(HttpMethod method, string url, string? token = null)
        {
            using var request = new HttpRequestMessage(method, url);
            using var response = await SendAsync(request, token);

            // 404 from logout (expired session) is treated as a successful sign-out.
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
                throw await BuildExceptionAsync(response);
        }

        private async Task<T> PostFileAsync<T>(string url, Stream stream, string fileName, string contentType, string? token = null)
        {
            using var request = BuildMultipartRequest(url, stream, fileName, contentType);
            using var response = await SendAsync(request, token);

            if (!response.IsSuccessStatusCode)
                throw await BuildExceptionAsync(response);

            var value = await response.Content.ReadFromJsonAsync<T>(JsonOpts);
            if (value is null)
                throw new ApiException((int)response.StatusCode, "The RaceDay API returned an unexpected empty response.");
            return value;
        }

        private async Task<string> PostFileForUrlAsync(string url, Stream stream, string fileName, string contentType, string urlProperty, string? token = null)
        {
            using var request = BuildMultipartRequest(url, stream, fileName, contentType);
            using var response = await SendAsync(request, token);

            if (!response.IsSuccessStatusCode)
                throw await BuildExceptionAsync(response);

            string body = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty(urlProperty, out var prop) && prop.ValueKind == JsonValueKind.String)
                    return prop.GetString() ?? string.Empty;
                if (doc.RootElement.TryGetProperty("url", out var urlProp) && urlProp.ValueKind == JsonValueKind.String)
                    return urlProp.GetString() ?? string.Empty;
            }
            catch (JsonException)
            {
                // Ignore malformed body — fall through and return empty string.
            }
            return string.Empty;
        }

        private static HttpRequestMessage BuildMultipartRequest(string url, Stream stream, string fileName, string contentType)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
            content.Add(fileContent, "file", fileName);

            return new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        }

        private static async Task<ApiException> BuildExceptionAsync(HttpResponseMessage response)
        {
            int status = (int)response.StatusCode;
            string message = $"The RaceDay API returned {(int)response.StatusCode} ({response.ReasonPhrase}).";

            try
            {
                string body = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(body))
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.String)
                    {
                        message = root.GetString() ?? message;
                    }
                    else if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                            message = m.GetString() ?? message;
                        else if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
                            message = t.GetString() ?? message;
                        else if (root.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var prop in errs.EnumerateObject())
                            {
                                if (prop.Value.ValueKind == JsonValueKind.Array && prop.Value.GetArrayLength() > 0)
                                {
                                    message = prop.Value[0].GetString() ?? message;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Non-JSON error body — keep the generic message.
            }
            catch (Exception)
            {
                // Reading the body failed — keep the generic message.
            }

            return new ApiException(status, message);
        }
    }
}
