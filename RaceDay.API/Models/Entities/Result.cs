using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.Entities
{
    public class Result
    {
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }

        public int? FinishTimeSec { get; set; } // Total seconds, e.g. 40680 = 11h 18m 00s

        public int? ChipTimeSec { get; set; }

        public int? GunPosition { get; set; }

        public int? CategoryPosition { get; set; }

        public int? GenderPosition { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Finished"; // Finished, DNF, DNS, Disqualified

        [MaxLength(1000)]
        public string? Splits { get; set; } // JSON or text, e.g., "10k:00:54:12, 21k:01:58:30"

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public EventEnrolment Enrolment { get; set; } = null!;
    }
}
