using System;
using System.Linq;
using BCrypt.Net;
using RaceDay.API.Models.Entities;

namespace RaceDay.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(RaceDayDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return; // DB has already been seeded
            }

            string defaultHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

            // 1. Users
            var u1 = new User
            {
                Email = "tebogo@soulcityevents.co.za",
                PasswordHash = defaultHash,
                Role = "Organiser",
                FullName = "Tebogo Moagi",
                PhoneNumber = "+27 82 555 1001",
                Province = "Gauteng",
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                IsActive = true
            };

            var u2 = new User
            {
                Email = "anika@ctcycletours.co.za",
                PasswordHash = defaultHash,
                Role = "Organiser",
                FullName = "Anika van Heerden",
                PhoneNumber = "+27 83 555 2002",
                Province = "Western Cape",
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                IsActive = true
            };

            var u3 = new User
            {
                Email = "sibusiso.ndlovu@gmail.com",
                PasswordHash = defaultHash,
                Role = "Participant",
                FullName = "Sibusiso Ndlovu",
                PhoneNumber = "+27 71 555 3003",
                Province = "KwaZulu-Natal",
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                IsActive = true
            };

            var u4 = new User
            {
                Email = "leighanne.botes@outlook.com",
                PasswordHash = defaultHash,
                Role = "Participant",
                FullName = "Leigh-Anne Botes",
                PhoneNumber = "+27 84 555 4004",
                Province = "Western Cape",
                CreatedAt = DateTime.UtcNow.AddDays(-40),
                IsActive = true
            };

            context.Users.AddRange(u1, u2, u3, u4);
            context.SaveChanges();

            // 2. Organisers
            var org1 = new Organiser
            {
                UserId = u1.UserId,
                CompanyName = "Soul City Events Pty Ltd",
                CompanyRegNo = "2018/123456/07",
                ContactEmail = "info@soulcityevents.co.za",
                IsVerified = true
            };

            var org2 = new Organiser
            {
                UserId = u2.UserId,
                CompanyName = "Cape Town Cycling Tours NPC",
                CompanyRegNo = "2015/987654/08",
                ContactEmail = "entries@ctcycletours.co.za",
                IsVerified = true
            };

            context.Organisers.AddRange(org1, org2);
            context.SaveChanges();

            // 3. Participants
            var p1 = new Participant
            {
                UserId = u3.UserId,
                DateOfBirth = new DateTime(1991, 5, 14),
                Gender = "Male",
                SAIDOrPassport = "9105145800081",
                ClubName = "Hollywoodbets Athletics Club",
                EmergencyContactName = "Thandi Ndlovu",
                EmergencyContactNo = "+27 72 111 2222",
                TshirtSize = "L"
            };

            var p2 = new Participant
            {
                UserId = u4.UserId,
                DateOfBirth = new DateTime(1996, 11, 23),
                Gender = "Female",
                SAIDOrPassport = "9611230099088",
                ClubName = "Nedbank Running Club",
                EmergencyContactName = "Johan Botes",
                EmergencyContactNo = "+27 82 333 4444",
                TshirtSize = "M"
            };

            context.Participants.AddRange(p1, p2);
            context.SaveChanges();

            // 4. Events
            var ev1 = new Event
            {
                OrganiserId = org1.OrganiserId,
                Title = "Comrades Marathon 2026",
                EventType = "Running",
                Description = "The Ultimate Human Race - 89km Down Run between Pietermaritzburg and Durban.",
                StartDate = DateTime.UtcNow.AddMonths(2),
                EndDate = DateTime.UtcNow.AddMonths(2).AddHours(12),
                City = "Durban",
                Province = "KwaZulu-Natal",
                RouteUrl = "https://raceday.co.za/routes/comrades-down-run.gpx",
                EntryFeeZar = 950.00m,
                MaxEntries = 22000,
                RegistrationOpen = DateTime.UtcNow.AddMonths(-3),
                RegistrationClose = DateTime.UtcNow.AddMonths(1),
                Status = "Open",
                ElevationGainMeters = 1180,
                KeyLandmarks = "Polly Shorts, Botha's Hill, Drummond, Cowie's Hill, Kingsmead",
                TerrainType = "Road / Tarmac",
                BannerUrl = "https://images.unsplash.com/photo-1552674605-db6ffd4facb5"
            };

            var ev2 = new Event
            {
                OrganiserId = org2.OrganiserId,
                Title = "Cape Town Cycle Tour 2026",
                EventType = "Cycling",
                Description = "109km scenic road cycling race around the iconic Cape Peninsula.",
                StartDate = DateTime.UtcNow.AddMonths(3),
                EndDate = DateTime.UtcNow.AddMonths(3).AddHours(8),
                City = "Cape Town",
                Province = "Western Cape",
                RouteUrl = "https://raceday.co.za/routes/ct-cycle-tour.gpx",
                EntryFeeZar = 820.00m,
                MaxEntries = 35000,
                RegistrationOpen = DateTime.UtcNow.AddMonths(-2),
                RegistrationClose = DateTime.UtcNow.AddMonths(2),
                Status = "Open",
                ElevationGainMeters = 1250,
                KeyLandmarks = "Suikerbossie, Chapman's Peak, Smitswinkel, Blue Route",
                TerrainType = "Coastal Road / Tarmac",
                BannerUrl = "https://images.unsplash.com/photo-1517649763962-0c623266ddc0"
            };

            var ev3 = new Event
            {
                OrganiserId = org1.OrganiserId,
                Title = "Soweto Marathon 2026",
                EventType = "Running",
                Description = "The People's Race through the vibrant streets and historic heritage of Soweto.",
                StartDate = DateTime.UtcNow.AddMonths(5),
                EndDate = DateTime.UtcNow.AddMonths(5).AddHours(7),
                City = "Johannesburg",
                Province = "Gauteng",
                RouteUrl = "https://raceday.co.za/routes/soweto-marathon.gpx",
                EntryFeeZar = 450.00m,
                MaxEntries = 15000,
                RegistrationOpen = DateTime.UtcNow.AddMonths(-1),
                RegistrationClose = DateTime.UtcNow.AddMonths(4),
                Status = "Open",
                ElevationGainMeters = 430,
                KeyLandmarks = "FNB Stadium, Vilakazi Street, Hector Pieterson Memorial, Walter Sisulu Square",
                TerrainType = "City Road / Tarmac",
                BannerUrl = "https://images.unsplash.com/photo-1461896836934-ffe607ba8211"
            };

            context.Events.AddRange(ev1, ev2, ev3);
            context.SaveChanges();

            // 5. Categories
            var cat1 = new EventCategory
            {
                EventId = ev1.EventId,
                CategoryName = "89km Down Run (Open)",
                DistanceKm = 89.00m,
                Gender = "All",
                MinAgeYears = 20,
                MaxAgeYears = 85,
                StartTime = new TimeSpan(5, 30, 0),
                CutoffHours = 12.00m,
                EntryFeeZar = 950.00m,
                MaxCapacity = 22000
            };

            var cat2 = new EventCategory
            {
                EventId = ev2.EventId,
                CategoryName = "109km Road Classic",
                DistanceKm = 109.00m,
                Gender = "All",
                MinAgeYears = 16,
                MaxAgeYears = 80,
                StartTime = new TimeSpan(6, 15, 0),
                CutoffHours = 7.00m,
                EntryFeeZar = 820.00m,
                MaxCapacity = 30000
            };

            var cat3 = new EventCategory
            {
                EventId = ev2.EventId,
                CategoryName = "42km Short Route",
                DistanceKm = 42.00m,
                Gender = "All",
                MinAgeYears = 14,
                MaxAgeYears = 85,
                StartTime = new TimeSpan(8, 0, 0),
                CutoffHours = 4.00m,
                EntryFeeZar = 550.00m,
                MaxCapacity = 5000
            };

            var cat4 = new EventCategory
            {
                EventId = ev3.EventId,
                CategoryName = "42.2km Standard Marathon",
                DistanceKm = 42.20m,
                Gender = "All",
                MinAgeYears = 20,
                MaxAgeYears = 80,
                StartTime = new TimeSpan(6, 0, 0),
                CutoffHours = 6.00m,
                EntryFeeZar = 450.00m,
                MaxCapacity = 8000
            };

            var cat5 = new EventCategory
            {
                EventId = ev3.EventId,
                CategoryName = "21.1km Half Marathon",
                DistanceKm = 21.10m,
                Gender = "All",
                MinAgeYears = 16,
                MaxAgeYears = 80,
                StartTime = new TimeSpan(6, 30, 0),
                CutoffHours = 3.50m,
                EntryFeeZar = 350.00m,
                MaxCapacity = 5000
            };

            var cat6 = new EventCategory
            {
                EventId = ev3.EventId,
                CategoryName = "10km Community Run/Walk",
                DistanceKm = 10.00m,
                Gender = "All",
                MinAgeYears = 10,
                MaxAgeYears = 90,
                StartTime = new TimeSpan(7, 0, 0),
                CutoffHours = 2.00m,
                EntryFeeZar = 220.00m,
                MaxCapacity = 2000
            };

            context.EventCategories.AddRange(cat1, cat2, cat3, cat4, cat5, cat6);
            context.SaveChanges();

            // 6. Enrolments
            var en1 = new EventEnrolment
            {
                ParticipantId = p1.ParticipantId,
                EventId = ev1.EventId,
                CategoryId = cat1.CategoryId,
                EnrolmentDate = DateTime.UtcNow.AddDays(-20),
                BibNumber = "A-10291",
                PaymentStatus = "Paid",
                PaymentReference = "PAY-COM-10291",
                Status = "Started",
                EmergencyNotes = "No allergies. Asthma inhaler carried."
            };

            var en2 = new EventEnrolment
            {
                ParticipantId = p2.ParticipantId,
                EventId = ev1.EventId,
                CategoryId = cat1.CategoryId,
                EnrolmentDate = DateTime.UtcNow.AddDays(-15),
                BibNumber = "B-20412",
                PaymentStatus = "Paid",
                PaymentReference = "PAY-COM-20412",
                Status = "Registered",
                EmergencyNotes = "Penicillin allergy."
            };

            var en3 = new EventEnrolment
            {
                ParticipantId = p2.ParticipantId,
                EventId = ev2.EventId,
                CategoryId = cat2.CategoryId,
                EnrolmentDate = DateTime.UtcNow.AddDays(-10),
                BibNumber = "C-30119",
                PaymentStatus = "Paid",
                PaymentReference = "PAY-CTC-30119",
                Status = "Registered",
                EmergencyNotes = "None"
            };

            var en4 = new EventEnrolment
            {
                ParticipantId = p1.ParticipantId,
                EventId = ev3.EventId,
                CategoryId = cat4.CategoryId,
                EnrolmentDate = DateTime.UtcNow.AddDays(-5),
                BibNumber = "S-40501",
                PaymentStatus = "Pending",
                PaymentReference = "PAY-SOW-40501",
                Status = "Registered",
                EmergencyNotes = "None"
            };

            context.EventEnrolments.AddRange(en1, en2, en3, en4);
            context.SaveChanges();

            // 7. Results
            var res1 = new Result
            {
                EnrolmentId = en1.EnrolmentId,
                FinishTimeSec = 40680, // 11h 18m 00s
                ChipTimeSec = 40590,   // 11h 16m 30s
                GunPosition = 842,
                CategoryPosition = 210,
                GenderPosition = 712,
                Status = "Finished",
                Splits = "10k:01:05:00, 21k:02:18:00, 42k:04:45:00, 60k:07:15:00, 80k:10:05:00",
                RecordedAt = DateTime.UtcNow.AddDays(-1)
            };

            context.Results.Add(res1);
            context.SaveChanges();

            // 8. WeatherSnapshots
            var w1 = new WeatherSnapshot
            {
                EventId = ev1.EventId,
                SnapshotTime = DateTime.UtcNow.AddHours(-1),
                TemperatureC = 22.5m,
                HumidityPct = 68,
                WindKmh = 14.2m,
                Condition = "Sunny / Mild",
                Source = "SA Weather Service (Durban North)",
                RaceDayAdvice = "Ideal morning conditions. Stay well-hydrated at water tables past Drummond as midday heat rises."
            };

            var w2 = new WeatherSnapshot
            {
                EventId = ev2.EventId,
                SnapshotTime = DateTime.UtcNow.AddHours(-2),
                TemperatureC = 19.0m,
                HumidityPct = 62,
                WindKmh = 28.5m,
                Condition = "Windy / Partly Cloudy",
                Source = "SA Weather Service (Cape Point)",
                RaceDayAdvice = "South-Easterly crosswinds expected along Chapman's Peak. Maintain safe following distances on descents."
            };

            context.WeatherSnapshots.AddRange(w1, w2);
            context.SaveChanges();
        }
    }
}
