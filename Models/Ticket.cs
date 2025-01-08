using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ETicketApp.Models
{
     public class Ticket
     {
          [Key] // Primary key
          public int TicketId { get; set; }

          public string? UserId { get; set; }

          public IdentityUser? User { get; set; }

          [Required] // EventId is required to link the ticket to an event
          public int EventId { get; set; }

          public Event? Event { get; set; }

          [Required]
          public DateTime PurchaseDate { get; set; }

          [Required]
          public string Status { get; set; } = "Active"; // Default status is "Active"

          public string? QrText { get; set; } // Text content for QR code

          public string? QrCodePath { get; set; } // Path to QR code image (if saving on the server)

          public string? QRCodeImage { get; set; } // Base64 encoded image of the QR Code

          public DateTime ExpiryDate { get; set; } // Expiry date for the ticket
     }
}
