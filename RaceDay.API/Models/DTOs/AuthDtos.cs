using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.DTOs
{
    public class RegisterOrganiserDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Province { get; set; }

        [Required]
        public string CompanyName { get; set; } = string.Empty;

        public string? CompanyRegNo { get; set; }

        [EmailAddress]
        public string? ContactEmail { get; set; }
    }

    public class RegisterParticipantDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Province { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string? Gender { get; set; } = "Other";

        public string? SAIDOrPassport { get; set; }

        public string? ClubName { get; set; }

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactNo { get; set; }

        public string? TshirtSize { get; set; }
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int? OrganiserId { get; set; }
        public int? ParticipantId { get; set; }
    }

    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
