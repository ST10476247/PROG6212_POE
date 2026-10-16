using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models.DTOs
{
    public class WeatherSnapshotDto
    {
        public int SnapshotId { get; set; }
        public int EventId { get; set; }
        public DateTime SnapshotTime { get; set; }
        public decimal? TemperatureC { get; set; }
        public int? HumidityPct { get; set; }
        public decimal? WindKmh { get; set; }
        public string? Condition { get; set; }
        public string? Source { get; set; }
        public string? RaceDayAdvice { get; set; }
    }

    public class CreateWeatherSnapshotDto
    {
        [Range(-10, 60)]
        public decimal? TemperatureC { get; set; }

        [Range(0, 100)]
        public int? HumidityPct { get; set; }

        [Range(0, 200)]
        public decimal? WindKmh { get; set; }

        [MaxLength(50)]
        public string? Condition { get; set; } = "Sunny";

        [MaxLength(50)]
        public string? Source { get; set; } = "On-site Timing Station";

        [MaxLength(500)]
        public string? RaceDayAdvice { get; set; }
    }

    public class RouteInfoDto
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public int ElevationGainMeters { get; set; }
        public string? KeyLandmarks { get; set; }
        public string? TerrainType { get; set; }
        public string? RouteUrl { get; set; }
    }
}
