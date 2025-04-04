using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Display(Name = "Event Name")]
        public string? EventName { get; set; }

        [Display(Name = "Date of the Event")]
        public DateTime EventDate { get; set; }

        public string? Description { get; set; }

    }
}
