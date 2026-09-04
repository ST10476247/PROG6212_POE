using System;
using System.Collections.Generic;
using System.Linq;
using RaceDay.Models;

namespace RaceDay.Services
{
    public class DataStore
    {
        public List<Organiser> Organisers { get; set; } = new();
        public List<Event> Events { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Participant> Participants { get; set; } = new();
        public List<Entry> Entries { get; set; } = new();
        public List<Result> Results { get; set; } = new();

        public DataStore()
        {
            SeedData();
        }

        private void SeedData()
        {
            // Seed Organisers
            var cma = new Organiser
            {
                OrganiserID = 1,
                OrganizationName = "Comrades Marathon Association (CMA)",
                ContactEmail = "info@comrades.com",
                Phone = "+27 33 897 8650",
                Province = "KwaZulu-Natal",
                IsVerified = true
            };
            var pedalTrust = new Organiser
            {
                OrganiserID = 2,
                OrganizationName = "Cape Town Cycle Tour Trust",
                ContactEmail = "entries@cycletour.co.za",
                Phone = "+27 21 681 4300",
                Province = "Western Cape",
                IsVerified = true
            };
            var sowetoClub = new Organiser
            {
                OrganiserID = 3,
                OrganizationName = "Soweto Athletics Club",
                ContactEmail = "race@sowetomarathon.org.za",
                Phone = "+27 11 938 1200",
                Province = "Gauteng",
                IsVerified = true
            };

            Organisers.AddRange(new[] { cma, pedalTrust, sowetoClub });

            // Seed Events
            var comrades = new Event
            {
                EventID = 1,
                OrganiserID = 1,
                EventName = "Comrades Marathon 2026",
                EventType = "Running",
                EventDate = new DateTime(2026, 6, 14),
                Location = "Pietermaritzburg to Durban (Down Run)",
                Province = "KwaZulu-Natal",
                Description = "The Ultimate Human Race! Iconic 89.8km ultra-marathon between Pietermaritzburg City Hall and Kings Park Stadium, Durban.",
                Status = "Upcoming",
                BannerUrl = "https://images.unsplash.com/photo-1530541930197-ff16ac917b0e?auto=format&fit=crop&w=1200&q=80",
                Route = new RouteInfo
                {
                    ElevationGainMeters = 1150,
                    KeyLandmarks = "Polly Shorts, Botha's Hill, Inchanga, Fields Hill, Cowies Hill",
                    TerrainType = "Tarred Road"
                },
                WeatherForecast = new WeatherInfo
                {
                    LocationName = "Durban / PMB",
                    TempCelsius = 22,
                    Condition = "Clear & Warm",
                    HumidityPercent = 65,
                    WindSpeedKmH = 14,
                    RaceDayAdvice = "Hydrate early at Drummond water points. Mild morning temp rising to 24°C at coastal finish."
                }
            };

            var cycleTour = new Event
            {
                EventID = 2,
                OrganiserID = 2,
                EventName = "Cape Town Cycle Tour 2026",
                EventType = "Cycling",
                EventDate = new DateTime(2026, 3, 8),
                Location = "Grand Parade, Cape Town - Cape Peninsula",
                Province = "Western Cape",
                Description = "The world's largest individually timed cycle race. 109km around the breathtaking Cape Peninsula, Suikerbossie, and Chapman's Peak.",
                Status = "Upcoming",
                BannerUrl = "https://images.unsplash.com/photo-1541625602330-2277a4c46182?auto=format&fit=crop&w=1200&q=80",
                Route = new RouteInfo
                {
                    ElevationGainMeters = 1220,
                    KeyLandmarks = "Edinburgh Drive, Smitswinkel, Chapman's Peak Drive, Suikerbossie",
                    TerrainType = "Scenic Coastal Highway"
                },
                WeatherForecast = new WeatherInfo
                {
                    LocationName = "Cape Peninsula",
                    TempCelsius = 24,
                    Condition = "Sunny with South-Easter Breeze",
                    HumidityPercent = 55,
                    WindSpeedKmH = 22,
                    RaceDayAdvice = "Watch out for crosswinds along Chapman's Peak. Keep aerodynamic gear steady."
                }
            };

            var sowetoMarathon = new Event
            {
                EventID = 3,
                OrganiserID = 3,
                EventName = "Soweto Marathon 2026",
                EventType = "Running",
                EventDate = new DateTime(2026, 11, 1),
                Location = "FNB Stadium, Soweto",
                Province = "Gauteng",
                Description = "The People's Race through the heart of Soweto heritage sites including Vilakazi Street, Hector Pieterson Memorial, and Chris Hani Baragwanath.",
                Status = "Upcoming",
                BannerUrl = "https://images.unsplash.com/photo-1452626038306-9aae5e071dd3?auto=format&fit=crop&w=1200&q=80",
                Route = new RouteInfo
                {
                    ElevationGainMeters = 540,
                    KeyLandmarks = "Vilakazi Street, Regina Mundi Church, Orlando Towers",
                    TerrainType = "Urban Township Road"
                },
                WeatherForecast = new WeatherInfo
                {
                    LocationName = "Soweto, Johannesburg",
                    TempCelsius = 26,
                    Condition = "Partly Cloudy",
                    HumidityPercent = 40,
                    WindSpeedKmH = 10,
                    RaceDayAdvice = "High altitude (1700m). Pacing is critical during the Vilakazi Street climb."
                }
            };

            Events.AddRange(new[] { comrades, cycleTour, sowetoMarathon });

            // Seed Categories
            var cat1 = new Category { CategoryID = 1, EventID = 1, CategoryName = "Down Run 89.8km", DistanceKm = 89.8m, EntryFeeZAR = 1200, MaxCapacity = 20000, StartTime = new TimeSpan(5, 30, 0), CutoffHours = 12.0m };
            var cat2 = new Category { CategoryID = 2, EventID = 2, CategoryName = "Main Peninsula 109km", DistanceKm = 109.0m, EntryFeeZAR = 850, MaxCapacity = 35000, StartTime = new TimeSpan(6, 0, 0), CutoffHours = 7.0m };
            var cat3 = new Category { CategoryID = 3, EventID = 2, CategoryName = "Short Route 42km", DistanceKm = 42.0m, EntryFeeZAR = 500, MaxCapacity = 5000, StartTime = new TimeSpan(8, 30, 0), CutoffHours = 4.5m };
            var cat4 = new Category { CategoryID = 4, EventID = 3, CategoryName = "Full Marathon 42.2km", DistanceKm = 42.2m, EntryFeeZAR = 450, MaxCapacity = 10000, StartTime = new TimeSpan(6, 0, 0), CutoffHours = 6.0m };
            var cat5 = new Category { CategoryID = 5, EventID = 3, CategoryName = "Half Marathon 21.1km", DistanceKm = 21.1m, EntryFeeZAR = 350, MaxCapacity = 10000, StartTime = new TimeSpan(6, 30, 0), CutoffHours = 3.5m };

            Categories.AddRange(new[] { cat1, cat2, cat3, cat4, cat5 });
            comrades.Categories.Add(cat1);
            cycleTour.Categories.AddRange(new[] { cat2, cat3 });
            sowetoMarathon.Categories.AddRange(new[] { cat4, cat5 });

            // Seed Participants
            var p1 = new Participant { ParticipantID = 1, FirstName = "Sipho", LastName = "Ndlovu", SAIDOrPassport = "9204125890087", Gender = "Male", DateOfBirth = new DateTime(1992, 4, 12), ClubName = "Hollywoodbets Athletics Club", EmergencyPhone = "+27 82 555 1234", Email = "sipho.ndlovu@example.co.za" };
            var p2 = new Participant { ParticipantID = 2, FirstName = "Anika", LastName = "Van Der Merwe", SAIDOrPassport = "9509200123081", Gender = "Female", DateOfBirth = new DateTime(1995, 9, 20), ClubName = "Nedbank Running Club", EmergencyPhone = "+27 83 444 9876", Email = "anika.vdm@example.co.za" };
            var p3 = new Participant { ParticipantID = 3, FirstName = "Tshepo", LastName = "Mokoena", SAIDOrPassport = "8811055432085", Gender = "Male", DateOfBirth = new DateTime(1988, 11, 5), ClubName = "Orlando Athletics Club", EmergencyPhone = "+27 71 333 4567", Email = "tshepo.m@example.co.za" };

            Participants.AddRange(new[] { p1, p2, p3 });

            // Seed Entries
            var e1 = new Entry { EntryID = 1, ParticipantID = 1, CategoryID = 1, BibNumber = "A-10492", RegistrationDate = DateTime.UtcNow.AddDays(-20), PaymentStatus = "Paid", PaymentReference = "PAY-CMR-9921", ParticipantName = "Sipho Ndlovu", CategoryName = "Down Run 89.8km", EventName = "Comrades Marathon 2026" };
            var e2 = new Entry { EntryID = 2, ParticipantID = 2, CategoryID = 2, BibNumber = "C-4401", RegistrationDate = DateTime.UtcNow.AddDays(-15), PaymentStatus = "Paid", PaymentReference = "PAY-CTC-7712", ParticipantName = "Anika Van Der Merwe", CategoryName = "Main Peninsula 109km", EventName = "Cape Town Cycle Tour 2026" };
            var e3 = new Entry { EntryID = 3, ParticipantID = 3, CategoryID = 4, BibNumber = "S-8821", RegistrationDate = DateTime.UtcNow.AddDays(-5), PaymentStatus = "Paid", PaymentReference = "PAY-SOW-3301", ParticipantName = "Tshepo Mokoena", CategoryName = "Full Marathon 42.2km", EventName = "Soweto Marathon 2026" };

            Entries.AddRange(new[] { e1, e2, e3 });

            // Seed Results (Historical / Demo)
            Results.Add(new Result { ResultID = 1, EntryID = 1, ParticipantName = "Sipho Ndlovu", EventName = "Comrades Marathon 2025", CategoryName = "Up Run 85.9km", BibNumber = "A-10492", GunTime = new TimeSpan(6, 45, 12), ChipTime = new TimeSpan(6, 44, 58), OverallRank = 142, CategoryRank = 38, GenderRank = 120, Status = "Finished (Silver Medal)" });
            Results.Add(new Result { ResultID = 2, EntryID = 2, ParticipantName = "Anika Van Der Merwe", EventName = "Cape Town Cycle Tour 2025", CategoryName = "Main Peninsula 109km", BibNumber = "C-4401", GunTime = new TimeSpan(3, 12, 05), ChipTime = new TimeSpan(3, 10, 44), OverallRank = 85, CategoryRank = 12, GenderRank = 8, Status = "Finished (Sub-3:15)" });
            Results.Add(new Result { ResultID = 3, EntryID = 3, ParticipantName = "Tshepo Mokoena", EventName = "Soweto Marathon 2025", CategoryName = "Full Marathon 42.2km", BibNumber = "S-8821", GunTime = new TimeSpan(2, 58, 40), ChipTime = new TimeSpan(2, 58, 30), OverallRank = 24, CategoryRank = 5, GenderRank = 22, Status = "Finished (Gold Qualifier)" });
        }

        public Entry AddEntry(int participantId, int categoryId, string medicalNotes)
        {
            var p = Participants.FirstOrDefault(x => x.ParticipantID == participantId);
            var c = Categories.FirstOrDefault(x => x.CategoryID == categoryId);
            var ev = c != null ? Events.FirstOrDefault(x => x.EventID == c.EventID) : null;

            int newId = Entries.Count > 0 ? Entries.Max(x => x.EntryID) + 1 : 1;
            string bib = $"RD-{Random.Shared.Next(10000, 99999)}";

            var entry = new Entry
            {
                EntryID = newId,
                ParticipantID = participantId,
                CategoryID = categoryId,
                BibNumber = bib,
                RegistrationDate = DateTime.UtcNow,
                PaymentStatus = "Paid",
                PaymentReference = $"PAY-RD-{Random.Shared.Next(100000, 999999)}",
                MedicalNotes = medicalNotes,
                ParticipantName = p != null ? $"{p.FirstName} {p.LastName}" : "Athlete",
                CategoryName = c?.CategoryName ?? "General Category",
                EventName = ev?.EventName ?? "Race Event"
            };

            Entries.Add(entry);
            return entry;
        }

        public Event AddEvent(Event newEv)
        {
            newEv.EventID = Events.Count > 0 ? Events.Max(x => x.EventID) + 1 : 1;
            Events.Add(newEv);
            return newEv;
        }
    }
}
