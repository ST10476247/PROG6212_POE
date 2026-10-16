using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models.Entities
{
    public class WeatherSnapshot
    {
        public int SnapshotId { get; set; }

        public int EventId { get; set; }

        public DateTime SnapshotTime { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(4,1)")]
        public decimal? TemperatureC { get; set; }

        public int? HumidityPct { get; set; }

        [Column(TypeName = "decimal(5,1)")]
        public decimal? WindKmh { get; set; }

        [MaxLength(50)]
        public string? Condition { get; set; } // Sunny, Partly Cloudy, Light Breeze, Hot, Rain

        [MaxLength(50)]
        public string? Source { get; set; } = "SA Weather Service";

        [MaxLength(500)]
        public string? RaceDayAdvice { get; set; }

        // Navigation
        public Event Event { get; set; } = null!;
    }
}
