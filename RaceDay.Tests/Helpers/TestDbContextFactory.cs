using System;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models.Entities;

namespace RaceDay.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static RaceDayDbContext Create(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayDbContext(options);
            context.Database.EnsureCreated();
            Seed(context);
            return context;
        }

        private static void Seed(RaceDayDbContext context)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("TestPass123!");

            // Organiser User
            var uOrg = new User
            {
                UserId = 1,
                Email = "organiser@raceday.co.za",
                PasswordHash = hash,
                Role = "Organiser",
                FullName = "Sipho Khumalo",
                PhoneNumber = "+27 82 123 4567",
                Province = "Gauteng",
                IsActive = true
            };

            // Participant User
            var uPart = new User
            {
                UserId = 2,
                Email = "runner@raceday.co.za",
                PasswordHash = hash,
                Role = "Participant",
                FullName = "Nomsa Dlamini",
                PhoneNumber = "+27 73 987 6543",
                Province = "KwaZulu-Natal",
                IsActive = true
            };

            context.Users.AddRange(uOrg, uPart);
            context.SaveChanges();

            var org = new Organiser
            {
                OrganiserId = 1,
                UserId = 1,
                CompanyName = "KZN Athletics Promoters",
                ContactEmail = "sipho@kznathletics.co.za",
                IsVerified = true
            };

            var part = new Participant
            {
                ParticipantId = 1,
                UserId = 2,
                DateOfBirth = new DateTime(1994, 3, 10),
                Gender = "Female",
                SAIDOrPassport = "9403100099081",
                ClubName = "Durban Athletic Club",
                EmergencyContactName = "Jabu Dlamini",
                EmergencyContactNo = "+27 73 111 2222",
                TshirtSize = "M"
            };

            context.Organisers.Add(org);
            context.Participants.Add(part);
            context.SaveChanges();

            var ev = new Event
            {
                EventId = 1,
                OrganiserId = 1,
                Title = "Durban City 10km & 21km",
                EventType = "Running",
                Description = "Fast, flat coastal road race along the Durban beachfront promenade.",
                StartDate = DateTime.UtcNow.AddMonths(1),
                City = "Durban",
                Province = "KwaZulu-Natal",
                EntryFeeZar = 250.00m,
                MaxEntries = 5000,
                Status = "Open",
                ElevationGainMeters = 45,
                TerrainType = "Road / Beachfront"
            };

            context.Events.Add(ev);
            context.SaveChanges();

            var cat1 = new EventCategory
            {
                CategoryId = 1,
                EventId = 1,
                CategoryName = "21.1km Half Marathon",
                DistanceKm = 21.10m,
                Gender = "All",
                MinAgeYears = 16,
                MaxAgeYears = 80,
                StartTime = new TimeSpan(6, 0, 0),
                CutoffHours = 3.50m,
                EntryFeeZar = 250.00m,
                MaxCapacity = 3000
            };

            var cat2 = new EventCategory
            {
                CategoryId = 2,
                EventId = 1,
                CategoryName = "10km Promenade Run",
                DistanceKm = 10.00m,
                Gender = "All",
                MinAgeYears = 12,
                MaxAgeYears = 85,
                StartTime = new TimeSpan(7, 0, 0),
                CutoffHours = 2.00m,
                EntryFeeZar = 180.00m,
                MaxCapacity = 2000
            };

            context.EventCategories.AddRange(cat1, cat2);
            context.SaveChanges();
        }
    }
}
