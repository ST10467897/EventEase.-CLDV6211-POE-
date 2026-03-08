using System.ComponentModel.DataAnnotations;

namespace EventEaseLocal.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Venue name is required.")]
        [StringLength(100)]
        [Display(Name = "Venue Name")]
        public string VenueName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 100000, ErrorMessage = "Capacity must be between 1 and 100,000.")]
        public int Capacity { get; set; }

        [Display(Name = "Image URL")]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
