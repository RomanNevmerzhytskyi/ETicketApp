using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ETicketApp.Models
{
     public class Event
     {
          [Key]
          public int EventId { get; set; }

          [Required(ErrorMessage = "Event name is required.")]
          [StringLength(100, ErrorMessage = "Event name must be less than 100 characters.")]
          public string EventName { get; set; } = string.Empty;

          [Required(ErrorMessage = "Location is required.")]
          [StringLength(200, ErrorMessage = "Location must be less than 200 characters.")]
          public string Location { get; set; } = string.Empty;

          [Required(ErrorMessage = "Ticket price is required.")]
          [Range(0, 100000, ErrorMessage = "Ticket price must be a positive number.")]
          public decimal TicketPrice { get; set; }

          public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

          [Required(ErrorMessage = "Event date is required.")]
          [DataType(DataType.DateTime)]
          public DateTime EventDate { get; set; }

          [Required(ErrorMessage = "Event duration is required.")]
          [Range(1, 1440, ErrorMessage = "Event duration must be between 1 and 1440 minutes.")]
          public int EventDuration { get; set; }  // Duration in minutes
     }
}
