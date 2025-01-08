using System.ComponentModel.DataAnnotations;

namespace ETicketApp.Models
{
     public class Event
     {
          [Key]
          public int EventId { get; set; }

          [Required]
          public string EventName { get; set; } = string.Empty;

          [Required]
          public string Location { get; set; } = string.Empty;

          [Required]
          public decimal TicketPrice { get; set; }

          public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

          [Required]
          public DateTime EventDate { get; set; }

          // Duration of the event in minutes
          public int EventDuration { get; set; } // Duration in minutes
     }
}
