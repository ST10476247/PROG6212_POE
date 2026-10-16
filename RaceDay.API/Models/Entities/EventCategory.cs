using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models.Entities
{
    public class EventCategory
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(6,2)")]
        public decimal DistanceKm { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; } = "All"; // All, Male, Female

        public int? MinAgeYears { get; set; }

        public int? MaxAgeYears { get; set; }

        public TimeSpan? StartTime { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? CutoffHours { get; set; }

        [Column(TypeName = "decimal(9,2)")]
        public decimal EntryFeeZar { get; set; }

        public int MaxCapacity { get; set; } = 5000;

        // Navigation
        public Event Event { get; set; } = null!;
        public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
    }
}
