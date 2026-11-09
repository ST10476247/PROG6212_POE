using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models.ViewModels
{
    public class MyResultsViewModel
    {
        public List<PersonalRaceResultItemViewModel> Results { get; set; } = new();
        public int TotalRacesCompleted { get; set; }
        public decimal TotalKilometersRaced { get; set; }
        public string? BestFinishPosition { get; set; }
    }

    public class PersonalRaceResultItemViewModel
    {
        public int ResultId { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string EventType { get; set; } = "Running";
        public DateTime EventDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public int BibNumber { get; set; }

        public TimeSpan GunTime { get; set; }
        public TimeSpan? ChipTime { get; set; }
        public string FormattedFinishTime => GunTime.ToString(@"hh\:mm\:ss");
        public string? FormattedChipTime => ChipTime.HasValue ? ChipTime.Value.ToString(@"hh\:mm\:ss") : null;

        public int OverallPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public int? GenderPosition { get; set; }
        public int TotalFinishers { get; set; }

        public string DisplayPosition => $"{OverallPosition}{GetOrdinalSuffix(OverallPosition)} out of {TotalFinishers} finishers";
        public string? DisplayCategoryPosition => CategoryPosition.HasValue ? $"{CategoryPosition}{GetOrdinalSuffix(CategoryPosition.Value)} in Category" : null;

        public string PacePerKm
        {
            get
            {
                if (DistanceKm <= 0 || GunTime.TotalMinutes <= 0) return "--:--";
                double paceMins = GunTime.TotalMinutes / (double)DistanceKm;
                int mins = (int)paceMins;
                int secs = (int)((paceMins - mins) * 60);
                return $"{mins}:{secs:D2} /km";
            }
        }

        public string Status { get; set; } = "Finished";

        public static string GetOrdinalSuffix(int num)
        {
            if (num <= 0) return "";
            if (num % 100 >= 11 && num % 100 <= 13) return "th";
            return (num % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }
    }

    public class EventResultsEntryViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string Status { get; set; } = "Completed";

        public List<ParticipantResultCaptureRowViewModel> Entrants { get; set; } = new();
        public int TotalEntrants { get; set; }
        public int RecordedResultsCount { get; set; }
    }

    public class ParticipantResultCaptureRowViewModel
    {
        public int EnrolmentId { get; set; }
        public int? ResultId { get; set; }
        public int BibNumber { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }

        [RegularExpression(@"^([0-9]{1,2}:)?[0-5]?[0-9]:[0-5][0-9]$", ErrorMessage = "Format must be HH:mm:ss or mm:ss")]
        public string? FinishTimeString { get; set; } // "03:45:22"

        public int? Position { get; set; }
        public string Status { get; set; } = "Finished"; // Finished, DNF, DNS, DQ
        public bool IsPublished { get; set; }
    }

    public class SingleResultCaptureViewModel
    {
        public int EventId { get; set; }
        public int EnrolmentId { get; set; }
        public int BibNumber { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Finish time is required")]
        [RegularExpression(@"^([0-9]{1,2}:)?[0-5]?[0-9]:[0-5][0-9]$", ErrorMessage = "Format must be HH:mm:ss (e.g. 03:45:22)")]
        [Display(Name = "Finish Time (HH:mm:ss)")]
        public string FinishTime { get; set; } = "00:00:00";

        [Required(ErrorMessage = "Finishing position is required")]
        [Range(1, 100000, ErrorMessage = "Position must be 1 or higher")]
        [Display(Name = "Finishing Position (e.g. 1 for 1st place)")]
        public int OverallPosition { get; set; } = 1;

        [Range(1, 100000)]
        [Display(Name = "Category Position (Optional)")]
        public int? CategoryPosition { get; set; }

        public string Status { get; set; } = "Finished"; // Finished, DNF, DNS, DQ
    }
}

