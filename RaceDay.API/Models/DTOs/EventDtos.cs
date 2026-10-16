using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.DTOs
{
    public class EventSummaryDto
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

    public class EventDetailDto : EventSummaryDto
    {
        public string? RouteUrl { get; set; }
        public int ElevationGainMeters { get; set; }
        public string? KeyLandmarks { get; set; }
        public string? TerrainType { get; set; }
        public DateTime? RegistrationOpen { get; set; }
        public DateTime? RegistrationClose { get; set; }

        public List<CategoryDto> Categories { get; set; } = new();
        public List<WeatherSnapshotDto> WeatherSnapshots { get; set; } = new();
    }

    public class CreateEventDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string EventType { get; set; } = "Running"; // Running, Cycling, Walking

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Province { get; set; } = string.Empty;

        public string? RouteUrl { get; set; }

        [Range(0, 100000)]
        public decimal EntryFeeZar { get; set; }

        [Range(1, 100000)]
        public int MaxEntries { get; set; } = 1000;

        public DateTime? RegistrationOpen { get; set; }

        public DateTime? RegistrationClose { get; set; }

        public string Status { get; set; } = "Open";

        public int ElevationGainMeters { get; set; }

        public string? KeyLandmarks { get; set; }

        public string? TerrainType { get; set; } = "Road / Tarmac";

        public string? BannerUrl { get; set; }
    }

    public class UpdateEventDto : CreateEventDto
    {
    }

    public class EventQueryParameters
    {
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? EventType { get; set; }
        public string? Status { get; set; }
        public string? Search { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
