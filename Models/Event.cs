using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        [StringLength(255)]
        public string EventName { get; set; } = string.Empty; // never null

        [StringLength(1000)]
        public string? Description { get; set; } // nullable

        // Navigation property – an Event can have many Bookings
        public ICollection<Booking>? Bookings { get; set; } // nullable
    }
}