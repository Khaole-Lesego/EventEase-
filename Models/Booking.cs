using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        public int VenueID { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Booking Date")]
        public DateTime? BookingDate { get; set; }

        // ==================NEW PROPERTY FOR IMAGE CHOICE==================
        // Stores "event" or "venue" — which image to show on the booking card.
        [StringLength(10)]
        [Display(Name = "Image Source")]
        public string? ImageChoice { get; set; }
        // ===========================================================

        // Navigation properties – may be null if not loaded
        [ForeignKey("EventID")]
        public virtual Event? Event { get; set; }

        [ForeignKey("VenueID")]
        public virtual Venue? Venue { get; set; }
    }
}