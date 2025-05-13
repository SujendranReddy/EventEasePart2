﻿using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        //Added simple validaton
        [Key]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event Name is required.")]
        [StringLength(100, ErrorMessage = "Event Name cannot exceed 100 characters.")]
        [Display(Name = "Event Name")]
        public string? EventName { get; set; }

        [Required(ErrorMessage = "Event Date is required.")]
        [Display(Name = "Date of the Event")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime EventDate { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
    }
}
