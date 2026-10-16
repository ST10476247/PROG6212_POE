using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.Entities
{
    public class User
    {
        public int UserId { get; set; }

        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Role { get; set; } = "Participant"; // Organiser, Participant, Admin

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(50)]
        public string? Province { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // 1:1 Navigation extensions
        public Organiser? OrganiserProfile { get; set; }
        public Participant? ParticipantProfile { get; set; }
    }
}
