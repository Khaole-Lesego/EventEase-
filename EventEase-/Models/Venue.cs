using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//
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

        // ==================NEW PROPERTY FOR PART2 ==================
        // This is IFormFile. It is used to capture the uploaded file from the Create/Edit form.
        // [NotMapped] tells Entity Framework to ignore this property when saving to the database.
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        // =============================================================

        // Navigation property
        public ICollection<Booking>? Bookings { get; set; }
    }
}