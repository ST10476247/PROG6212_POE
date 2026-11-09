using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = true;
        public string? ReturnUrl { get; set; }
    }

    public class RegisterParticipantViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(150, MinimumLength = 3)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public string Province { get; set; } = "Gauteng";

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; } = new DateTime(1995, 1, 1);

        public string Gender { get; set; } = "Male";

        [RegularExpression(@"^[0-9]{13}$", ErrorMessage = "South African ID must be 13 digits")]
        public string? SAIDOrPassport { get; set; }

        public string? ClubName { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNo { get; set; }
        public string TshirtSize { get; set; } = "M";
    }

    public class RegisterOrganiserViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(150, MinimumLength = 3)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company / Organization Name is required")]
        [StringLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        public string? CompanyRegNo { get; set; }

        [EmailAddress]
        public string? ContactEmail { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string Province { get; set; } = "Gauteng";
    }

    public class CurrentUserSession
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int? OrganiserId { get; set; }
        public int? ParticipantId { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsOrganiser => Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase);
        public bool IsParticipant => Role.Equals("Participant", StringComparison.OrdinalIgnoreCase);
    }
}

