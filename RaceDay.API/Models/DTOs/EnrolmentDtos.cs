using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.DTOs
{
    public class CreateEnrolmentDto
    {
        [Required]
        public int CategoryId { get; set; }

        public string? EmergencyNotes { get; set; }
    }

    public class EnrolmentResponseDto
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string? BibNumber { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? EmergencyNotes { get; set; }

        // Optional result link if finished
        public ResultDto? Result { get; set; }
    }

    public class UpdateEnrolmentStatusDto
    {
        public string? BibNumber { get; set; }
        public string? PaymentStatus { get; set; } // Paid, Pending, Refunded, Cancelled
        public string? Status { get; set; } // Registered, Started, DidNotStart, Withdrawn
        public string? EmergencyNotes { get; set; }
    }
}
