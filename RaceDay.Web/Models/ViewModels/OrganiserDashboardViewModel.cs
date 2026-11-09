using System;
using System.Collections.Generic;

namespace RaceDay.Web.Models.ViewModels
{
    public class OrganiserDashboardViewModel
    {
        public int OrganiserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;

        // Metric Counters
        public int TotalEventsCreated { get; set; }
        public int TotalEnrolmentsAcrossAllEvents { get; set; }
        public int UpcomingEventsCount { get; set; }
        public int CompletedEventsCount { get; set; }
        public decimal TotalRevenueZar { get; set; }

        // Events list with enrolments and dates
        public List<OrganiserEventSummaryViewModel> Events { get; set; } = new();

        // Upcoming events highlight
        public List<OrganiserEventSummaryViewModel> UpcomingEvents { get; set; } = new();
    }

    public class OrganiserEventSummaryViewModel
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string EventType { get; set; } = "Running";
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public decimal EntryFeeZar { get; set; }
        public int MaxEntries { get; set; }
        public string Status { get; set; } = "Open";
        public string? BannerUrl { get; set; }
        public int CategoriesCount { get; set; }
        public int TotalEnrolments { get; set; }
        public int ConfirmedEnrolments { get; set; }
        public decimal RevenueZar => ConfirmedEnrolments * EntryFeeZar;
        public bool IsUpcoming => StartDate > DateTime.UtcNow;
        public double CapacityPercentage => MaxEntries > 0 ? Math.Min(100.0, ((double)TotalEnrolments / MaxEntries) * 100.0) : 0.0;
    }
}

