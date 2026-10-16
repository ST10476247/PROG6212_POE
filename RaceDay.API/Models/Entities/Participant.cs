using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.Entities
{
    public class Participant
    {
        public int ParticipantId { get; set; }

        public int UserId { get; set; }

        public DateTime DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(30)]
        public string? SAIDOrPassport { get; set; }

        [MaxLength(150)]
        public string? ClubName { get; set; }

        [MaxLength(150)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactNo { get; set; }

        [MaxLength(5)]
        public string? TshirtSize { get; set; } // XS, S, M, L, XL, XXL

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
    }
}
