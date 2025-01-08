using System.Collections.Generic;
using ETicketApp.Models;

namespace ETicketApp.Models
{
     public class HomeViewModel
     {
          public List<Event> Events { get; set; }
          public List<Ticket> Tickets { get; set; }
     }
}
