using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Models.ViewModels
{
    public class EventCardViewModel
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string EventType { get; set; } = "Running";
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public decimal EntryFeeZar { get; set; }
        public int MaxEntries { get; set; }
        public string Status { get; set; } = "Open";
        public string? BannerUrl { get; set; }
        public int CategoriesCount { get; set; }
        public int EntrantsCount { get; set; }

        public bool IsUpcoming => StartDate > DateTime.UtcNow;
        public bool IsRegistrationOpen => Status == "Open" && (MaxEntries <= 0 || EntrantsCount < MaxEntries);
    }

    public class EventDetailsViewModel
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string EventType { get; set; } = "Running";
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string? RouteUrl { get; set; }
        public decimal EntryFeeZar { get; set; }
        public int MaxEntries { get; set; }
        public DateTime? RegistrationOpen { get; set; }
        public DateTime? RegistrationClose { get; set; }
        public string Status { get; set; } = "Open";
        public int ElevationGainMeters { get; set; }
        public string? KeyLandmarks { get; set; }
        public string? TerrainType { get; set; }
        public string? BannerUrl { get; set; }
        public List<CategoryViewModel> Categories { get; set; } = new();
        public int EntrantsCount { get; set; }

        public bool IsUserEnrolled { get; set; }
        public int? UserEnrolmentId { get; set; }
        public string? UserEnrolmentStatus { get; set; }
    }

    public class EventFilterViewModel
    {
        public string? Search { get; set; }
        public string? EventType { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Status { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
        public List<EventCardViewModel> Items { get; set; } = new();
    }

    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Event Title is required")]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event Type is required")]
        public string EventType { get; set; } = "Running"; // Running, Cycling, Walking

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(30);

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "City / Town is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province is required")]
        [StringLength(50)]
        public string Province { get; set; } = "Gauteng";

        [StringLength(500)]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? RouteUrl { get; set; }

        [Required(ErrorMessage = "Entry Fee is required")]
        [Range(0, 10000, ErrorMessage = "Entry fee must be between R0 and R10,000")]
        public decimal EntryFeeZar { get; set; } = 250.00m;

        [Required(ErrorMessage = "Maximum entries is required")]
        [Range(1, 100000, ErrorMessage = "Must be at least 1 entrant")]
        public int MaxEntries { get; set; } = 1000;

        [DataType(DataType.Date)]
        public DateTime? RegistrationOpen { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime? RegistrationClose { get; set; } = DateTime.Today.AddDays(25);

        public string Status { get; set; } = "Open"; // Draft, Open, Closed, Completed, Cancelled

        [Range(0, 10000)]
        public int ElevationGainMeters { get; set; } = 150;

        [StringLength(300)]
        public string? KeyLandmarks { get; set; }

        [StringLength(100)]
        public string? TerrainType { get; set; } = "Road / Tarmac";

        [Display(Name = "Banner Poster Image (Uploaded to Azure Blob Storage)")]
        public IFormFile? BannerImageFile { get; set; }

        public string? BannerUrl { get; set; }
    }

    public class EditEventViewModel : CreateEventViewModel
    {
        public int EventId { get; set; }
        public string? ExistingBannerUrl { get; set; }
    }
}

