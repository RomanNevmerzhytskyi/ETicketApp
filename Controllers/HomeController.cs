using ETicketApp.Models;
using ETicketApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ETicketApp.Controllers
{
     public class HomeController : Controller
     {
          private readonly TicketingContext _context;
          private readonly ILogger<HomeController> _logger;
          private readonly QrCodeService _qrCodeService;

          public HomeController(TicketingContext context, ILogger<HomeController> logger, QrCodeService qrCodeService)
          {
               _context = context;
               _logger = logger;
               _qrCodeService = qrCodeService;
          }

          // Index method to display events and tickets
          public async Task<IActionResult> Index()
          {
               var model = new HomeViewModel
               {
                    Events = await _context.Events.ToListAsync(),
                    Tickets = await _context.Tickets
                       .Include(t => t.User)  // Ensure User is loaded
                       .Where(t => t.User.Email == User.Identity.Name) // Get tickets for the current logged-in user
                       .ToListAsync()
               };

               return View(model);
          }

          // Details for Event
          public async Task<IActionResult> EventDetails(int id)
          {
               var eventDetail = await _context.Events
                   .FirstOrDefaultAsync(e => e.EventId == id);

               if (eventDetail == null)
               {
                    return NotFound();
               }

               return View(eventDetail);
          }

          // Details for Ticket
          public async Task<IActionResult> TicketDetails(int id)
          {
               var ticketDetail = await _context.Tickets
                   .Include(t => t.Event)  // Ensure event details are included
                   .FirstOrDefaultAsync(t => t.TicketId == id);

               if (ticketDetail == null)
               {
                    return NotFound();
               }

               return View(ticketDetail);
          }

          // Simulate ticket purchase for non-admin users
          public async Task<IActionResult> BuyTicket(int eventId)
          {
               if (!User.Identity.IsAuthenticated) // Check if user is logged in
               {
                    // Redirect to LoginRequired page if not logged in
                    return RedirectToAction("LoginRequired");
               }

               var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
               var eventDetails = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);

               if (user == null || eventDetails == null)
               {
                    return NotFound();
               }

               var ticket = new Ticket
               {
                    UserId = user.Id,
                    EventId = eventDetails.EventId,
                    Status = "Purchased",
                    PurchaseDate = DateTime.Now,
                    ExpiryDate = eventDetails.EventDate.AddHours(2) // Set expiry 2 hours after the event
               };

               // Add the ticket to the database
               _context.Tickets.Add(ticket);
               await _context.SaveChangesAsync();

               // Generate the QR Code for the ticket
               string qrCodeContent = $"TicketId:{ticket.TicketId},Event:{eventDetails.EventName},Status:{ticket.Status},User:{user.UserName},Expiry:{ticket.ExpiryDate}";
               var qrCodeImage = _qrCodeService.GenerateQrCodeBase64(qrCodeContent); // Generate the QR code image

               // Save the QR code image and text in the ticket
               ticket.QRCodeImage = qrCodeImage;   // Store the Base64 string for QR Code Image
               ticket.QrText = qrCodeContent;      // Store the QR code text

               // Update the ticket and save it to the database
               _context.Tickets.Update(ticket);
               await _context.SaveChangesAsync();

               TempData["SuccessMessage"] = $"You have successfully purchased a ticket for {eventDetails.EventName}!";
               return RedirectToAction("TicketPurchased");
          }

          // Confirmation page for ticket purchase
          public IActionResult TicketPurchased()
          {
               return View();
          }

          // Login Required page
          public IActionResult LoginRequired()
          {
               return View();
          }

          // Delete Ticket method for non-admin users
          public async Task<IActionResult> Delete(int id)
          {
               var ticket = await _context.Tickets
                   .Include(t => t.User)  // Ensure User is loaded
                   .FirstOrDefaultAsync(t => t.TicketId == id && t.User.Email == User.Identity.Name); // Check if the ticket belongs to the logged-in user

               if (ticket == null)
               {
                    return NotFound();
               }

               _context.Tickets.Remove(ticket);
               await _context.SaveChangesAsync();

               TempData["SuccessMessage"] = "Your ticket has been successfully deleted.";
               return RedirectToAction(nameof(Index));
          }

          [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
          public IActionResult Error()
          {
               return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
          }
     }
}
