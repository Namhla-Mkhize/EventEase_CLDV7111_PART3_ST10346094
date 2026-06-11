using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        public string? EventName { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        public DateTime? EventDate { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }

        public int VenueId { get; set; }

        public Venue? Venue { get; set; }

        public int EventTypeId { get; set; }

        public EventType? EventType { get; set; }
    }
}