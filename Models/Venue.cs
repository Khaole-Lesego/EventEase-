using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueID { get; set; }

        [Required]
        [StringLength(255)]
        public string VenueName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        // Navigation property
        public ICollection<Booking>? Bookings { get; set; }
    }
}