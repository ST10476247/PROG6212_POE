using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models.Entities;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Organiser> Organisers => Set<Organiser>();
        public DbSet<Participant> Participants => Set<Participant>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<EventCategory> EventCategories => Set<EventCategory>();
        public DbSet<EventEnrolment> EventEnrolments => Set<EventEnrolment>();
        public DbSet<Result> Results => Set<Result>();
        public DbSet<WeatherSnapshot> WeatherSnapshots => Set<WeatherSnapshot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // 2. Organisers (1:1 with User)
            modelBuilder.Entity<Organiser>(entity =>
            {
                entity.HasKey(o => o.OrganiserId);
                entity.HasIndex(o => o.UserId).IsUnique();

                entity.HasOne(o => o.User)
                      .WithOne(u => u.OrganiserProfile)
                      .HasForeignKey<Organiser>(o => o.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 3. Participants (1:1 with User)
            modelBuilder.Entity<Participant>(entity =>
            {
                entity.HasKey(p => p.ParticipantId);
                entity.HasIndex(p => p.UserId).IsUnique();

                entity.HasOne(p => p.User)
                      .WithOne(u => u.ParticipantProfile)
                      .HasForeignKey<Participant>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 4. Events (1:N with Organiser)
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.EventId);

                entity.HasOne(e => e.Organiser)
                      .WithMany(o => o.Events)
                      .HasForeignKey(e => e.OrganiserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 5. EventCategories (1:N with Event)
            modelBuilder.Entity<EventCategory>(entity =>
            {
                entity.HasKey(c => c.CategoryId);
                entity.HasIndex(c => new { c.EventId, c.CategoryName }).IsUnique();

                entity.HasOne(c => c.Event)
                      .WithMany(e => e.Categories)
                      .HasForeignKey(c => c.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 6. EventEnrolments (N:M between Participant and Category)
            modelBuilder.Entity<EventEnrolment>(entity =>
            {
                entity.HasKey(en => en.EnrolmentId);
                entity.HasIndex(en => new { en.ParticipantId, en.CategoryId }).IsUnique();

                entity.HasOne(en => en.Participant)
                      .WithMany(p => p.Enrolments)
                      .HasForeignKey(en => en.ParticipantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(en => en.Event)
                      .WithMany(e => e.Enrolments)
                      .HasForeignKey(en => en.EventId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(en => en.Category)
                      .WithMany(c => c.Enrolments)
                      .HasForeignKey(en => en.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 7. Results (1:1 with EventEnrolment)
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(r => r.ResultId);
                entity.HasIndex(r => r.EnrolmentId).IsUnique();

                entity.HasOne(r => r.Enrolment)
                      .WithOne(en => en.Result)
                      .HasForeignKey<Result>(r => r.EnrolmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 8. WeatherSnapshots (1:N with Event)
            modelBuilder.Entity<WeatherSnapshot>(entity =>
            {
                entity.HasKey(w => w.SnapshotId);

                entity.HasOne(w => w.Event)
                      .WithMany(e => e.WeatherSnapshots)
                      .HasForeignKey(w => w.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
