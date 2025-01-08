using ETicketApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ETicketApp.Controllers
{
     [Authorize(Roles = "Admin")]
     public class EventsController : Controller
     {
          private readonly TicketingContext _context;

          public EventsController(TicketingContext context)
          {
               _context = context;
          }

          // GET: Events
          public async Task<IActionResult> Index()
          {
               return View(await _context.Events.ToListAsync());
          }

          // GET: Events/Details/5
          public async Task<IActionResult> Details(int? id)
          {
               if (id == null)
               {
                    return NotFound();
               }

               var eventObj = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
               if (eventObj == null)
               {
                    return NotFound();
               }

               return View(eventObj);
          }

          // GET: Events/Create
          public IActionResult Create()
          {
               return View();
          }

          // POST: Events/Create
          [HttpPost]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Create([Bind("EventName,Location,EventDate,TicketPrice,EventDuration")] Event eventObj)
          {
               if (ModelState.IsValid)
               {
                    try
                    {
                         if (eventObj.EventDate == default)
                         {
                              eventObj.EventDate = DateTime.UtcNow;  // Use UTC time for consistency
                         }

                         if (eventObj.EventDuration <= 0)
                         {
                              ModelState.AddModelError(nameof(eventObj.EventDuration), "Event duration must be a positive number.");
                              return View(eventObj);
                         }

                         _context.Add(eventObj);
                         await _context.SaveChangesAsync();
                         return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                         ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                    }
               }

               return View(eventObj);
          }

          // GET: Events/Edit/5
          public async Task<IActionResult> Edit(int? id)
          {
               if (id == null)
               {
                    return NotFound();
               }

               var eventObj = await _context.Events.FindAsync(id);
               if (eventObj == null)
               {
                    return NotFound();
               }

               return View(eventObj);
          }

          // POST: Events/Edit/5
          [HttpPost]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,Location,EventDate,TicketPrice,EventDuration")] Event eventObj)
          {
               if (id != eventObj.EventId)
               {
                    return NotFound();
               }

               if (ModelState.IsValid)
               {
                    try
                    {
                         if (eventObj.EventDuration <= 0)
                         {
                              ModelState.AddModelError(nameof(eventObj.EventDuration), "Event duration must be a positive number.");
                              return View(eventObj);
                         }

                         _context.Update(eventObj);
                         await _context.SaveChangesAsync();
                         return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                         if (!EventExists(eventObj.EventId))
                         {
                              return NotFound();
                         }
                         else
                         {
                              throw;
                         }
                    }
               }

               return View(eventObj);
          }

          // GET: Events/Delete/5
          public async Task<IActionResult> Delete(int? id)
          {
               if (id == null)
               {
                    return NotFound();
               }

               var eventObj = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
               if (eventObj == null)
               {
                    return NotFound();
               }

               return View(eventObj);
          }

          // POST: Events/Delete/5
          [HttpPost, ActionName("Delete")]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> DeleteConfirmed(int id)
          {
               var eventObj = await _context.Events.FindAsync(id);
               if (eventObj != null)
               {
                    _context.Events.Remove(eventObj);
                    await _context.SaveChangesAsync();
               }

               return RedirectToAction(nameof(Index));
          }

          private bool EventExists(int id)
          {
               return _context.Events.Any(e => e.EventId == id);
          }
     }
}
