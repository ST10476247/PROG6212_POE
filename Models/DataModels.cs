using System;
using System.Collections.Generic;

namespace RaceDay.Models
{
    public class Organiser
    {
        public int OrganiserID { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Event
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventType { get; set; } = "Running"; // Running, Cycling, Walking
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty; // e.g. Pietermaritzburg to Durban
        public string Province { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Upcoming"; // Upcoming, Live, Completed
        public string BannerUrl { get; set; } = string.Empty;
        public List<Category> Categories { get; set; } = new();
        public RouteInfo Route { get; set; } = new();
        public WeatherInfo WeatherForecast { get; set; } = new();
    }

    public class Category
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal EntryFeeZAR { get; set; }
        public int MaxCapacity { get; set; }
        public TimeSpan StartTime { get; set; }
        public decimal CutoffHours { get; set; }
    }

    public class Participant
    {
        public int ParticipantID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string SAIDOrPassport { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public string EmergencyPhone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class Entry
    {
        public int EntryID { get; set; }
        public int ParticipantID { get; set; }
        public int CategoryID { get; set; }
        public string BibNumber { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public string PaymentStatus { get; set; } = "Paid"; // Paid, Pending, Cancelled
        public string PaymentReference { get; set; } = string.Empty;
        public string MedicalNotes { get; set; } = "None";

        // Navigation helpers for API output
        public string? ParticipantName { get; set; }
        public string? CategoryName { get; set; }
        public string? EventName { get; set; }
    }

    public class Result
    {
        public int ResultID { get; set; }
        public int EntryID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string BibNumber { get; set; } = string.Empty;
        public TimeSpan GunTime { get; set; }
        public TimeSpan ChipTime { get; set; }
        public int OverallRank { get; set; }
        public int CategoryRank { get; set; }
        public int GenderRank { get; set; }
        public string Status { get; set; } = "Finished"; // Finished, DNF, DNS
    }

    public class RouteInfo
    {
        public int ElevationGainMeters { get; set; }
        public string KeyLandmarks { get; set; } = string.Empty; // e.g., "Polly Shorts, Botha's Hill, Inchanga"
        public string TerrainType { get; set; } = "Road / Tarmac";
    }

    public class WeatherInfo
    {
        public string LocationName { get; set; } = string.Empty;
        public int TempCelsius { get; set; }
        public string Condition { get; set; } = "Sunny"; // Sunny, Partly Cloudy, Light Breeze, Hot
        public int HumidityPercent { get; set; }
        public int WindSpeedKmH { get; set; }
        public string RaceDayAdvice { get; set; } = string.Empty;
    }
}
