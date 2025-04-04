using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Display(Name = "Venue Name")]
        public string? VenueName { get; set; }

        public string? Location { get; set; }

        public int Capacity { get; set; }

        [Display(Name = "Image URL")]
        public string? ImageURL { get; set; }
    }
}

