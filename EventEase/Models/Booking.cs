using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; }

        [Display(Name = "Venue")]
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }

        [Display(Name = "Event")]
        public int EventId { get; set; }
        public Event? Event { get; set; }

    }
}