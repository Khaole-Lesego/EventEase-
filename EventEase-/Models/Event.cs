using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // ==================NEW PROPERTIES FOR IMAGE==================
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        // ==========================================================

        // Navigation property – an Event can have many Bookings
        public ICollection<Booking>? Bookings { get; set; } // nullable
    }
}