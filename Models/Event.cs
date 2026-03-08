using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseLocal.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(150)]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a venue.")]
        [Display(Name = "Venue")]
        public int VenueId { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
