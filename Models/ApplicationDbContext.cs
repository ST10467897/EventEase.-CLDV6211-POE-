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
                new Venue { VenueId = 3, VenueName = "The Garden Pavilion", Location = "78 Oak Avenue, Pretoria", Capacity = 150, ImageUrl = "https://images.unsplash.com/photo-1510076857177-7470076d4098?w=600" },
                new Venue { VenueId = 4, VenueName = "Ocean View Hall", Location = "12 Beach Road, Durban", Capacity = 300, ImageUrl = "https://images.unsplash.com/photo-1544928147-79a2dbc1f389?w=600" },
                new Venue { VenueId = 5, VenueName = "Mountain Lodge", Location = "5 Peak Drive, Drakensberg", Capacity = 80, ImageUrl = "https://images.unsplash.com/photo-1587061949409-02df41d5e562?w=600" },
                new Venue { VenueId = 6, VenueName = "The Vineyard Estate", Location = "90 Wine Route, Stellenbosch", Capacity = 250, ImageUrl = "https://images.unsplash.com/photo-1505236858219-8359eb29e329?w=600" },
                new Venue { VenueId = 7, VenueName = "City Conference Centre", Location = "200 Commissioner Street, Johannesburg", Capacity = 1000, ImageUrl = "https://images.unsplash.com/photo-1497366216548-37526070297c?w=600" }
            );

            modelBuilder.Entity<Event>().HasData(
                new Event { EventId = 1, EventName = "Annual Tech Conference", Description = "A premier technology conference featuring industry leaders.", VenueId = 1 },
                new Event { EventId = 2, EventName = "Wedding Reception - Mokoena", Description = "Private wedding reception for the Mokoena family.", VenueId = 2 },
                new Event { EventId = 3, EventName = "Corporate Year-End Gala", Description = "Formal dinner and awards ceremony for Zephyr Corp employees.", VenueId = 6 },
                new Event { EventId = 4, EventName = "Charity Fun Run Launch", Description = "Kickoff event for the annual Sunshine Charity 10K.", VenueId = 3 },
                new Event { EventId = 5, EventName = "Product Launch - Nova Phone", Description = "Exclusive launch event for the new Nova smartphone line.", VenueId = 7 },
                new Event { EventId = 6, EventName = "Birthday Celebration - Naidoo", Description = "50th birthday celebration for the Naidoo family.", VenueId = 4 },
                new Event { EventId = 7, EventName = "Team Building Retreat", Description = "Two-day team building workshop for Apex Solutions.", VenueId = 5 },
                new Event { EventId = 8, EventName = "Music Festival Day Pass", Description = "Live music performances across multiple stages.", VenueId = 1 }
            );

            modelBuilder.Entity<Booking>().HasData(
                new Booking { BookingId = 1, EventId = 1, VenueId = 1, EventDate = new DateTime(2026, 4, 15), StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0) },
                new Booking { BookingId = 2, EventId = 2, VenueId = 2, EventDate = new DateTime(2026, 5, 10), StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(23, 0, 0) },
                new Booking { BookingId = 3, EventId = 3, VenueId = 6, EventDate = new DateTime(2026, 12, 5), StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(23, 30, 0) },
                new Booking { BookingId = 4, EventId = 4, VenueId = 3, EventDate = new DateTime(2026, 6, 1), StartTime = new TimeSpan(7, 0, 0), EndTime = new TimeSpan(12, 0, 0) },
                new Booking { BookingId = 5, EventId = 5, VenueId = 7, EventDate = new DateTime(2026, 7, 20), StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(15, 0, 0) },
                new Booking { BookingId = 6, EventId = 6, VenueId = 4, EventDate = new DateTime(2026, 8, 14), StartTime = new TimeSpan(17, 0, 0), EndTime = new TimeSpan(22, 0, 0) },
                new Booking { BookingId = 7, EventId = 7, VenueId = 5, EventDate = new DateTime(2026, 9, 3), StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(16, 0, 0) },
                new Booking { BookingId = 8, EventId = 8, VenueId = 1, EventDate = new DateTime(2026, 10, 18), StartTime = new TimeSpan(12, 0, 0), EndTime = new TimeSpan(22, 0, 0) }
            );
        }
    }
}
