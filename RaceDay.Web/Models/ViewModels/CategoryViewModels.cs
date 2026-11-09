using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? GenderRestriction { get; set; }
        public decimal? EntryFeeZar { get; set; }
        public int MaxParticipants { get; set; }
        public TimeSpan? StartTime { get; set; }
        public int EnrolmentsCount { get; set; }

        public string FormattedStartTime => StartTime.HasValue ? DateTime.Today.Add(StartTime.Value).ToString("HH:mm") : "TBA";
        public string AgeBracket => (MinAge.HasValue || MaxAge.HasValue) ? $"{MinAge ?? 0} - {MaxAge?.ToString() ?? "Open"} yrs" : "Open";
    }

    public class CreateCategoryViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category name is required (e.g. 42.2km Marathon, 21.1km Half, 10km Peace Run)")]
        [StringLength(100, MinimumLength = 2)]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Distance is required in kilometers")]
        [Range(0.1, 500.0, ErrorMessage = "Distance must be between 0.1 and 500 km")]
        public decimal DistanceKm { get; set; } = 21.1m;

        [Range(5, 120, ErrorMessage = "Age must be between 5 and 120")]
        public int? MinAge { get; set; } = 16;

        [Range(5, 120, ErrorMessage = "Age must be between 5 and 120")]
        public int? MaxAge { get; set; }

        public string GenderRestriction { get; set; } = "None"; // None, Male, Female

        [Range(0, 10000)]
        public decimal? EntryFeeZar { get; set; } = 250m;

        [Range(1, 100000)]
        public int MaxParticipants { get; set; } = 500;

        [DataType(DataType.Time)]
        public TimeSpan? StartTime { get; set; } = new TimeSpan(6, 0, 0);
    }
}

