using System.ComponentModel.DataAnnotations;

namespace EventEaseLocal.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(60)]
        [Display(Name = "Event Type")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
