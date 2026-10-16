using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.Entities
{
    public class EventEnrolment
    {
        public int EnrolmentId { get; set; }

        public int ParticipantId { get; set; }

        public int EventId { get; set; }

        public int CategoryId { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(15)]
        public string? BibNumber { get; set; }

        [Required, MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Refunded, Cancelled

        [MaxLength(50)]
        public string? PaymentReference { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Registered"; // Registered, Started, DidNotStart, Withdrawn

        [MaxLength(500)]
        public string? EmergencyNotes { get; set; }

        // Navigation
        public Participant Participant { get; set; } = null!;
        public Event Event { get; set; } = null!;
        public EventCategory Category { get; set; } = null!;
        public Result? Result { get; set; }
    }
}
