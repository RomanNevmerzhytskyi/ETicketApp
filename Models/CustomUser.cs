using Microsoft.AspNetCore.Identity;

namespace ETicketApp.Models
{
    public class CustomUser : IdentityUser
    {
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
