using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.Entities
{
    public class Organiser
    {
        public int OrganiserId { get; set; }

        public int UserId { get; set; }

        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? CompanyRegNo { get; set; }

        [Required, EmailAddress, MaxLength(255)]
        public string ContactEmail { get; set; } = string.Empty;

        public bool IsVerified { get; set; } = true;

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
