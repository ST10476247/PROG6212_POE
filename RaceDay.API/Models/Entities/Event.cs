using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models.Entities
{
    public class Event
    {
        public int EventId { get; set; }

        public int OrganiserId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string EventType { get; set; } = "Running"; // Running, Cycling, Walking

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Province { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? RouteUrl { get; set; }

        [Column(TypeName = "decimal(9,2)")]
        public decimal EntryFeeZar { get; set; }

        public int MaxEntries { get; set; } = 1000;

        public DateTime? RegistrationOpen { get; set; }

        public DateTime? RegistrationClose { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Open"; // Draft, Open, Closed, Completed, Cancelled

        public int ElevationGainMeters { get; set; }

        [MaxLength(300)]
        public string? KeyLandmarks { get; set; }

        [MaxLength(100)]
        public string? TerrainType { get; set; } = "Road / Tarmac";

        [MaxLength(500)]
        public string? BannerUrl { get; set; }

        // Navigation
        public Organiser Organiser { get; set; } = null!;
        public ICollection<EventCategory> Categories { get; set; } = new List<EventCategory>();
        public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
        public ICollection<WeatherSnapshot> WeatherSnapshots { get; set; } = new List<WeatherSnapshot>();
    }
}
