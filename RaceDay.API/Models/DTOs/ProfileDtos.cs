using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.DTOs
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Province { get; set; }
        public DateTime CreatedAt { get; set; }

        public OrganiserDetailDto? OrganiserProfile { get; set; }
        public ParticipantDetailDto? ParticipantProfile { get; set; }
    }

    public class OrganiserDetailDto
    {
        public int OrganiserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyRegNo { get; set; }
        public string ContactEmail { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }

    public class ParticipantDetailDto
    {
        public int ParticipantId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? SAIDOrPassport { get; set; }
        public string? ClubName { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNo { get; set; }
        public string? TshirtSize { get; set; }
    }

    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Province { get; set; }

        // Organiser updates
        public string? CompanyName { get; set; }
        public string? CompanyRegNo { get; set; }
        public string? ContactEmail { get; set; }

        // Participant updates
        public string? ClubName { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNo { get; set; }
        public string? TshirtSize { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
