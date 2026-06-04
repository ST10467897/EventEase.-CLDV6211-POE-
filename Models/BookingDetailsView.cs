namespace EventEaseLocal.Models
{
    public class BookingDetailsView
    {
        public int BookingId { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string BookedBy { get; set; } = string.Empty;

        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? EventDescription { get; set; }

        public int VenueId { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public string VenueLocation { get; set; } = string.Empty;
        public int VenueCapacity { get; set; }
        public string? VenueImageUrl { get; set; }
    }
}
