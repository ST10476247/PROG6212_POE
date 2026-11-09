using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Models.ViewModels
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Province { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Participant specific
        public int? ParticipantId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? SAIDOrPassport { get; set; }
        public string? ClubName { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNo { get; set; }
        public string? TshirtSize { get; set; }

        // Organiser specific
        public int? OrganiserId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyRegNo { get; set; }
        public string? ContactEmail { get; set; }
        public bool IsVerified { get; set; }

        public bool IsParticipant => Role.Equals("Participant", StringComparison.OrdinalIgnoreCase);
        public bool IsOrganiser => Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase);
    }

    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(150, MinimumLength = 3)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Province { get; set; }

        // Participant fields
        public string? ClubName { get; set; }
        public string? EmergencyContactName { get; set; }

        [Phone]
        public string? EmergencyContactNo { get; set; }

        public string? TshirtSize { get; set; }

        // Organiser fields
        public string? CompanyName { get; set; }
        public string? CompanyRegNo { get; set; }
        public string? ContactEmail { get; set; }

        // Profile Picture Upload
        [Display(Name = "Profile Picture (Uploaded via API to Azure Blob Storage)")]
        public IFormFile? ProfilePictureFile { get; set; }
        public string? ExistingProfilePictureUrl { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm new password is required")]
        [Compare("NewPassword", ErrorMessage = "New passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

