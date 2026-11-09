using System;
using System.Collections.Generic;

namespace RaceDay.Web.Models.Api
{
    // ─────────────────────────────────────────────────────────────
    //  Authentication
    // ─────────────────────────────────────────────────────────────
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterParticipantRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Province { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-20);
        public string? Gender { get; set; } = "Other";
        public string? SAIDOrPassport { get; set; }
        public string? ClubName { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNo { get; set; }
        public string? TshirtSize { get; set; }
    }

    public class RegisterOrganiserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Province { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyRegNo { get; set; }
        public string? ContactEmail { get; set; }
    }

    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int? OrganiserId { get; set; }
        public int? ParticipantId { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    //  Events
    // ─────────────────────────────────────────────────────────────
    public class EventSummary
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public decimal EntryFeeZar { get; set; }
        public int MaxEntries { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }
        public int CategoriesCount { get; set; }
        public int EntrantsCount { get; set; }
    }

    public class EventDetail : EventSummary
    {
        public string? RouteUrl { get; set; }
        public int ElevationGainMeters { get; set; }
        public string? KeyLandmarks { get; set; }
        public string? TerrainType { get; set; }
        public DateTime? RegistrationOpen { get; set; }
        public DateTime? RegistrationClose { get; set; }
        public List<CategoryDto> Categories { get; set; } = new();
        public List<WeatherSnapshot> WeatherSnapshots { get; set; } = new();
    }

    /// <summary>Used for both create and update event requests.</summary>
    public class EventRequest
    {
        public string Title { get; set; } = string.Empty;
        public string EventType { get; set; } = "Running";
        public string? Description { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(30);
        public DateTime? EndDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string? RouteUrl { get; set; }
        public decimal EntryFeeZar { get; set; }
        public int MaxEntries { get; set; } = 1000;
        public DateTime? RegistrationOpen { get; set; }
        public DateTime? RegistrationClose { get; set; }
        public string Status { get; set; } = "Open";
        public int ElevationGainMeters { get; set; }
        public string? KeyLandmarks { get; set; }
        public string? TerrainType { get; set; } = "Road / Tarmac";
        public string? BannerUrl { get; set; }
    }

    public class EventQuery
    {
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? EventType { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    //  Categories
    // ─────────────────────────────────────────────────────────────
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public string? Gender { get; set; }
        public int? MinAgeYears { get; set; }
        public int? MaxAgeYears { get; set; }
        public TimeSpan? StartTime { get; set; }
        public decimal? CutoffHours { get; set; }
        public decimal EntryFeeZar { get; set; }
        public int MaxCapacity { get; set; }
        public int EnrolledCount { get; set; }
    }

    public class CategoryRequest
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public string? Gender { get; set; } = "All";
        public int? MinAgeYears { get; set; }
        public int? MaxAgeYears { get; set; }
        public TimeSpan? StartTime { get; set; }
        public decimal? CutoffHours { get; set; }
        public decimal EntryFeeZar { get; set; }
        public int MaxCapacity { get; set; } = 5000;
    }

    // ─────────────────────────────────────────────────────────────
    //  Enrolments
    // ─────────────────────────────────────────────────────────────
    public class CreateEnrolmentRequest
    {
        public int CategoryId { get; set; }
        public string? EmergencyNotes { get; set; }
    }

    public class EnrolmentDto
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal EntryFeeZar { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string? BibNumber { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? EmergencyNotes { get; set; }

        public string? PhoneNumber { get; set; }
        public string? ClubName { get; set; }
        public string? TshirtSize { get; set; }
        public string? EmergencyContact { get; set; }

        public ResultDto? Result { get; set; }
    }

    public class UpdateEnrolmentStatusRequest
    {
        public string? BibNumber { get; set; }
        public string? PaymentStatus { get; set; }
        public string? Status { get; set; }
        public string? EmergencyNotes { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    //  Results
    // ─────────────────────────────────────────────────────────────
    public class ResultDto
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string? BibNumber { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }
        public decimal DistanceKm { get; set; }
        public int TotalFinishers { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? FinishTimeSec { get; set; }
        public int? ChipTimeSec { get; set; }
        public string? FinishTimeFormatted { get; set; }
        public string? ChipTimeFormatted { get; set; }
        public int? GunPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public int? GenderPosition { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Splits { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    public class CreateResultRequest
    {
        public int EnrolmentId { get; set; }
        public int? FinishTimeSec { get; set; }
        public int? ChipTimeSec { get; set; }
        public int? GunPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public int? GenderPosition { get; set; }
        public string Status { get; set; } = "Finished";
        public string? Splits { get; set; }
    }

    /// <summary>Body for PUT /api/results/{id} (same shape as the create request).</summary>
    public class UpdateResultItemRequest
    {
        public int EnrolmentId { get; set; }
        public int? FinishTimeSec { get; set; }
        public int? ChipTimeSec { get; set; }
        public int? GunPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public int? GenderPosition { get; set; }
        public string Status { get; set; } = "Finished";
        public string? Splits { get; set; }
    }

    /// <summary>Body item for POST /api/events/{eventId}/results/batch (matched by bib number).</summary>
    public class BatchResultItemRequest
    {
        public string BibNumber { get; set; } = string.Empty;
        public int? FinishTimeSec { get; set; }
        public int? ChipTimeSec { get; set; }
        public int? GunPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public int? GenderPosition { get; set; }
        public string Status { get; set; } = "Finished";
        public string? Splits { get; set; }
    }

    /// <summary>Response from the batch results import endpoint.</summary>
    public class BatchResultResponse
    {
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    // ─────────────────────────────────────────────────────────────
    //  Weather
    // ─────────────────────────────────────────────────────────────
    public class WeatherSnapshot
    {
        public int SnapshotId { get; set; }
        public int EventId { get; set; }
        public DateTime SnapshotTime { get; set; }
        public decimal? TemperatureC { get; set; }
        public int? HumidityPct { get; set; }
        public decimal? WindKmh { get; set; }
        public string? Condition { get; set; }
        public string? Source { get; set; }
        public string? RaceDayAdvice { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    //  Profile
    // ─────────────────────────────────────────────────────────────
    public class UserProfile
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Province { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrganiserDetail? OrganiserProfile { get; set; }
        public ParticipantDetail? ParticipantProfile { get; set; }
    }

    public class OrganiserDetail
    {
        public int OrganiserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyRegNo { get; set; }
        public string ContactEmail { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }

    public class ParticipantDetail
    {
        public int ParticipantId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? SAIDOrPassport { get; set; }
        public string? ClubName { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNo { get; set; }
        public string? TshirtSize { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Province { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyRegNo { get; set; }
        public string? ContactEmail { get; set; }
        public string? ClubName { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNo { get; set; }
        public string? TshirtSize { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    //  Utilities / misc
    // ─────────────────────────────────────────────────────────────
    public class UploadResponse
    {
        public string? Url { get; set; }
        public string? Message { get; set; }
    }

    public class MessageResponse
    {
        public string? Message { get; set; }
    }

    public class CreateEventResponse
    {
        public int EventId { get; set; }
        public string? Message { get; set; }
        public string? Title { get; set; }
    }

    public class ParticipantHistory
    {
        public int ParticipantId { get; set; }
        public string AthleteName { get; set; } = string.Empty;
        public string? ClubName { get; set; }
        public int TotalFinishes { get; set; }
        public decimal TotalKmCompleted { get; set; }
        public List<HistoryItem> History { get; set; } = new();
    }

    public class HistoryItem
    {
        public string EventTitle { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public DateTime EventDate { get; set; }
        public int? FinishTimeSec { get; set; }
        public string? FinishTime { get; set; }
        public int? GunPosition { get; set; }
        public int? CategoryPosition { get; set; }
    }
}
