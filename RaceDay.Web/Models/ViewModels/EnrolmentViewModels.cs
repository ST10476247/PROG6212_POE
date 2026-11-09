using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models.ViewModels
{
    public class EnrolViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string? BannerUrl { get; set; }

        [Required(ErrorMessage = "Please select a race category")]
        [Display(Name = "Race Distance / Category")]
        public int CategoryId { get; set; }

        public List<CategoryViewModel> AvailableCategories { get; set; } = new();

        [Display(Name = "T-Shirt Size")]
        public string TshirtSize { get; set; } = "M";

        [Display(Name = "Club / Athletic Affiliation")]
        public string? ClubName { get; set; }

        [Required(ErrorMessage = "Emergency contact name is required")]
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Emergency contact number is required")]
        [Phone]
        [Display(Name = "Emergency Contact Phone")]
        public string EmergencyContactNo { get; set; } = string.Empty;

        [Display(Name = "Medical Aid Name & Policy Number")]
        public string? MedicalAidDetails { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the RaceDay indemnity and waiver.")]
        public bool AcceptTerms { get; set; }
    }

    public class EnrolmentCardViewModel
    {
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string EventType { get; set; } = "Running";
        public DateTime EventDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }

        public int BibNumber { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = "Confirmed"; // Confirmed, Pending, Cancelled
        public decimal FeePaidZar { get; set; }
        public string? Notes { get; set; }

        public bool HasResult { get; set; }
        public int? ResultId { get; set; }

        public bool IsEventUpcoming => EventDate > DateTime.UtcNow;
    }

    public class EventEnrolmentListViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public int MaxEntries { get; set; }
        public int TotalEnrolments { get; set; }

        public List<CategoryViewModel> Categories { get; set; } = new();
        public List<ParticipantEnrolmentRowViewModel> Enrolments { get; set; } = new();

        public int ConfirmedCount { get; set; }
        public int PendingCount { get; set; }
        public int CancelledCount { get; set; }
    }

    public class ParticipantEnrolmentRowViewModel
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ClubName { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public int BibNumber { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = "Confirmed";
        public decimal FeePaidZar { get; set; }
        public string? TshirtSize { get; set; }
        public string? EmergencyContact { get; set; }
    }

    public class UpdateStatusViewModel
    {
        public int EnrolmentId { get; set; }
        public string Status { get; set; } = "Confirmed";
    }
}

