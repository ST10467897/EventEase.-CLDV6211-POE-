using Microsoft.EntityFrameworkCore;

namespace EventEaseLocal.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Venue>(entity =>
            {
                entity.HasIndex(v => v.VenueName).IsUnique();
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasOne(e => e.Venue)
                      .WithMany(v => v.Events)
                      .HasForeignKey(e => e.VenueId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasOne(b => b.Event)
                      .WithMany(e => e.Bookings)
                      .HasForeignKey(b => b.EventId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Venue)
                      .WithMany(v => v.Bookings)
                      .HasForeignKey(b => b.VenueId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Venue>().HasData(
                new Venue { VenueId = 1, VenueName = "Grand Ballroom", Location = "123 Main Street, Johannesburg", Capacity = 500, ImageUrl = "https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=600" },
                new Venue { VenueId = 2, VenueName = "Skyline Terrace", Location = "45 Rivonia Road, Sandton", Capacity = 200, ImageUrl = "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=600" },
                new Venue { VenueId = 3, VenueName = "The Garden Pavilion", Location = "78 Oak Avenue, Pretoria", Capacity = 150, ImageUrl = "https://images.unsplash.com/photo-1510076857177-7470076d4098?w=600" }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event { EventId = 1, EventName = "Annual Tech Conference", Description = "A premier technology conference featuring industry leaders.", VenueId = 1 },
                new Event { EventId = 2, EventName = "Wedding Reception - Mokoena", Description = "Private wedding reception for the Mokoena family.", VenueId = 2 }
            );

            modelBuilder.Entity<Booking>().HasData(
                new Booking { BookingId = 1, EventId = 1, VenueId = 1, EventDate = new DateTime(2026, 4, 15), StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0) }
            );
        }
    }
}
