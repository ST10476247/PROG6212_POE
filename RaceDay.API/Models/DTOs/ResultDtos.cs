using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.DTOs
{
    public class ResultDto
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string? BibNumber { get; set; }
        public string EventTitle { get; set; } = string.Empty;
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

        public static string FormatSeconds(int? seconds)
        {
            if (!seconds.HasValue || seconds.Value <= 0) return "--:--:--";
            var ts = TimeSpan.FromSeconds(seconds.Value);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }

    public class CreateResultDto
    {
        [Required]
        public int EnrolmentId { get; set; }

        public int? FinishTimeSec { get; set; }
        public int? ChipTimeSec { get; set; }
        public int? GunPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public int? GenderPosition { get; set; }

        [Required]
        public string Status { get; set; } = "Finished"; // Finished, DNF, DNS, Disqualified

        public string? Splits { get; set; }
    }

    public class UpdateResultDto : CreateResultDto
    {
    }

    public class BatchResultItemDto
    {
        [Required]
        public string BibNumber { get; set; } = string.Empty;

        public int? FinishTimeSec { get; set; }
        public int? ChipTimeSec { get; set; }
        public int? GunPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public int? GenderPosition { get; set; }
        public string Status { get; set; } = "Finished";
        public string? Splits { get; set; }
    }

    public class BatchResultResponseDto
    {
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
