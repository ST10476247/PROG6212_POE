using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.DTOs
{
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

    public class CreateCategoryDto
    {
        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required, Range(0.1, 1000.0)]
        public decimal DistanceKm { get; set; }

        public string? Gender { get; set; } = "All";

        public int? MinAgeYears { get; set; }

        public int? MaxAgeYears { get; set; }

        public TimeSpan? StartTime { get; set; }

        public decimal? CutoffHours { get; set; }

        [Required, Range(0, 100000)]
        public decimal EntryFeeZar { get; set; }

        [Range(1, 100000)]
        public int MaxCapacity { get; set; } = 5000;
    }

    public class UpdateCategoryDto : CreateCategoryDto
    {
    }
}
