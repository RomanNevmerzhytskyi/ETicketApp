using ETicketApp.Models;
using ETicketApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace ETicketApp.Controllers
{
     [Route("Tickets")]
     public class TicketsController : Controller
     {
          private readonly TicketingContext _context;
          private readonly QrCodeService _qrCodeService;

          public TicketsController(TicketingContext context, QrCodeService qrCodeService)
          {
               _context = context;
               _qrCodeService = qrCodeService;
          }

          // GET: Tickets
          [HttpGet]
          public async Task<IActionResult> Index()
          {
               var tickets = await _context.Tickets.Include(t => t.Event).Include(t => t.User).ToListAsync();
               return View(tickets);
          }

          // Create Ticket (restricted to Admin)
          [HttpGet("Create")]
          [Authorize(Roles = "Admin")]
          public IActionResult Create()
          {
               PopulateUserDropdown();
               PopulateEventDropdown();
               return View();
          }

          [HttpPost("Create")]
          [Authorize(Roles = "Admin")]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Create([Bind("EventId,UserId,PurchaseDate,Status")] Ticket ticket)
          {
               if (ModelState.IsValid)
               {
                    try
                    {
                         // Ensure EventId is valid
                         if (ticket.EventId == 0)
                         {
                              TempData["ErrorMessage"] = "Event is required.";
                              return RedirectToAction(nameof(Index));
                         }

                         // Retrieve event details to calculate expiry
                         var eventDetails = await _context.Events.FirstOrDefaultAsync(e => e.EventId == ticket.EventId);
                         if (eventDetails != null)
                         {
                              ticket.ExpiryDate = eventDetails.EventDate.AddMinutes(eventDetails.EventDuration); // Assuming EventDuration is in minutes
                         }
                         else
                         {
                              TempData["ErrorMessage"] = "Event not found!";
                              return RedirectToAction(nameof(Index));
                         }

                         // Save the ticket to the database to generate TicketId
                         _context.Add(ticket);
                         await _context.SaveChangesAsync();

                         // Generate QR code text automatically based on ticket details (now we have TicketId)
                         string qrText = $"TicketId:{ticket.TicketId},EventId:{ticket.EventId},ExpiryDate:{ticket.ExpiryDate:yyyy-MM-dd},Status:{ticket.Status}";
                         ticket.QrText = qrText;

                         // Generate the QR code image as Base64 and set it in the ticket
                         ticket.QRCodeImage = _qrCodeService.GenerateQrCodeBase64(qrText); // Base64-encoded image

                         // Update the ticket with QR code image
                         _context.Update(ticket);
                         await _context.SaveChangesAsync();

                         // Fetch the ticket again to ensure QRCodeImage is included before returning to the view
                         var savedTicket = await _context.Tickets
                             .Include(t => t.Event)
                             .Include(t => t.User)
                             .FirstOrDefaultAsync(t => t.TicketId == ticket.TicketId);

                         TempData["SuccessMessage"] = "Ticket successfully created!";
                         return View(savedTicket);  // Pass the saved ticket (including QRCodeImage) back to the view
                    }
                    catch (Exception ex)
                    {
                         TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                         return RedirectToAction(nameof(Index));
                    }
               }

               PopulateUserDropdown();
               PopulateEventDropdown();
               return View(ticket);
          }

          // Edit Ticket (restricted to Admin)
          [HttpGet("Edit/{id}")]
          [Authorize(Roles = "Admin")]
          public async Task<IActionResult> Edit(int id)
          {
               var ticket = await _context.Tickets
                   .Include(t => t.Event)
                   .Include(t => t.User)
                   .FirstOrDefaultAsync(t => t.TicketId == id);

               if (ticket == null)
               {
                    return NotFound();
               }

               PopulateUserDropdown(ticket.UserId);
               PopulateEventDropdown(ticket.EventId);
               return View(ticket);
          }

          [HttpPost("Edit/{id}")]
          [Authorize(Roles = "Admin")]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Edit(int id, [Bind("TicketId,EventId,UserId,PurchaseDate,Status,QrText")] Ticket ticket)
          {
               if (id != ticket.TicketId)
               {
                    return NotFound();
               }

               if (ModelState.IsValid)
               {
                    try
                    {
                         if (string.IsNullOrEmpty(ticket.QRCodeImage))
                         {
                              ticket.QRCodeImage = _qrCodeService.GenerateQrCodeBase64(ticket.QrText ?? "Default text");
                         }
                         _context.Update(ticket);
                         await _context.SaveChangesAsync();
                         TempData["SuccessMessage"] = "Ticket successfully updated!";
                         return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                         if (!TicketExists(ticket.TicketId))
                         {
                              return NotFound();
                         }
                         else
                         {
                              throw;
                         }
                    }
               }

               PopulateUserDropdown(ticket.UserId);
               PopulateEventDropdown(ticket.EventId);
               return View(ticket);
          }

          // Delete Ticket (restricted to Admin)
          [HttpGet("Delete/{id}")]
          [Authorize(Roles = "Admin")]
          public async Task<IActionResult> Delete(int id)
          {
               var ticket = await _context.Tickets
                   .Include(t => t.Event)
                   .Include(t => t.User)
                   .FirstOrDefaultAsync(t => t.TicketId == id);

               if (ticket == null)
               {
                    return NotFound();
               }

               return View(ticket);
          }

          [HttpPost("Delete/{id}"), ActionName("Delete")]
          [Authorize(Roles = "Admin")]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> DeleteConfirmed(int id)
          {
               var ticket = await _context.Tickets.FindAsync(id);
               if (ticket != null)
               {
                    _context.Tickets.Remove(ticket);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Ticket successfully deleted!";
               }
               return RedirectToAction(nameof(Index));
          }

          // Details Ticket
          [HttpGet("Details/{id}")]
          public async Task<IActionResult> Details(int id)
          {
               var ticket = await _context.Tickets
                   .Include(t => t.Event)
                   .Include(t => t.User)
                   .FirstOrDefaultAsync(t => t.TicketId == id);

               if (ticket == null)
               {
                    return NotFound();
               }

               return View(ticket);
          }

          // QR Code Scanner Page
          [HttpGet("QRScanner")]
          public IActionResult QRScanner()
          {
               return View();
          }

          // Ticket Validation Endpoint for QR Code
          [HttpPost("ValidateTicket")]
          public async Task<IActionResult> ValidateTicket([FromBody] TicketValidationRequest request)
          {
               var ticket = await _context.Tickets
                   .Include(t => t.Event)
                   .FirstOrDefaultAsync(t => t.TicketId == request.TicketId);

               if (ticket == null)
               {
                    return BadRequest(new { success = false, message = "Ticket not found." });
               }

               if (ticket.ExpiryDate < DateTime.Now)
               {
                    return BadRequest(new { success = false, message = "Ticket has expired." });
               }

               if (ticket.Status == "Used")
               {
                    return BadRequest(new { success = false, message = "Ticket has already been used." });
               }

               ticket.Status = "Used";
               _context.Update(ticket);
               await _context.SaveChangesAsync();

               return Ok(new { success = true, message = "Ticket validated successfully." });
          }

          private void PopulateUserDropdown(object selectedUser = null)
          {
               var usersQuery = from u in _context.Users
                                orderby u.UserName
                                select u;

               ViewBag.UserIdList = new SelectList(usersQuery.AsNoTracking(), "Id", "UserName", selectedUser);
          }

          private void PopulateEventDropdown(object selectedEvent = null)
          {
               var eventsQuery = from e in _context.Events
                                 orderby e.EventName
                                 select e;

               ViewBag.EventIdList = new SelectList(eventsQuery.AsNoTracking(), "EventId", "EventName", selectedEvent);
          }

          private bool TicketExists(int id)
          {
               return _context.Tickets.Any(e => e.TicketId == id);
          }
     }
}
