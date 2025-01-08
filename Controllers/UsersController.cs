using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ETicketApp.ViewModels;
using ETicketApp.Models;

namespace ETicketApp.Controllers
{
     public class UsersController : Controller
     {
          private readonly UserManager<IdentityUser> _userManager;
          private readonly TicketingContext _context;

          // Inject UserManager to manage IdentityUser accounts and TicketingContext to access tickets
          public UsersController(UserManager<IdentityUser> userManager, TicketingContext context)
          {
               _userManager = userManager;
               _context = context;
          }

          // GET: Users
          public async Task<IActionResult> Index()
          {
               var users = await _userManager.Users.ToListAsync();
               return View(users);
          }

          // GET: Users/Create
          public IActionResult Create()
          {
               return View();
          }

          // POST: Users/Create
          [HttpPost]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Create(UserCreationViewModel model)
          {
               if (ModelState.IsValid)
               {
                    var newUser = new IdentityUser
                    {
                         UserName = model.UserName,
                         Email = model.Email
                    };

                    var result = await _userManager.CreateAsync(newUser, model.Password);
                    if (result.Succeeded)
                    {
                         return RedirectToAction(nameof(Index));
                    }
                    foreach (var error in result.Errors)
                    {
                         ModelState.AddModelError(string.Empty, error.Description);
                    }
               }
               return View(model);
          }

          // GET: Users/Details/5
          public async Task<IActionResult> Details(string id)
          {
               if (id == null)
               {
                    return NotFound();
               }

               var user = await _userManager.FindByIdAsync(id);
               if (user == null)
               {
                    return NotFound();
               }

               return View(user);
          }

          // GET: Users/Edit/5
          public async Task<IActionResult> Edit(string id)
          {
               if (id == null)
               {
                    return NotFound();
               }

               var user = await _userManager.FindByIdAsync(id);
               if (user == null)
               {
                    return NotFound();
               }

               var editUserViewModel = new UserEditViewModel
               {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email
               };

               return View(editUserViewModel);
          }

          // POST: Users/Edit/5
          [HttpPost]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Edit(string id, UserEditViewModel model)
          {
               if (id != model.Id)
               {
                    return NotFound();
               }

               if (ModelState.IsValid)
               {
                    var existingUser = await _userManager.FindByIdAsync(id);
                    if (existingUser == null)
                    {
                         return NotFound();
                    }

                    existingUser.UserName = model.UserName;
                    existingUser.Email = model.Email;

                    var result = await _userManager.UpdateAsync(existingUser);
                    if (!result.Succeeded)
                    {
                         foreach (var error in result.Errors)
                         {
                              ModelState.AddModelError(string.Empty, error.Description);
                         }
                         return View(model);
                    }

                    return RedirectToAction(nameof(Index));
               }

               return View(model);
          }

          // GET: Users/Delete/5
          public async Task<IActionResult> Delete(string id)
          {
               if (id == null)
               {
                    return NotFound();
               }

               var user = await _userManager.FindByIdAsync(id);
               if (user == null)
               {
                    return NotFound();
               }

               return View(user);
          }

          // POST: Users/Delete/5
          [HttpPost, ActionName("Delete")]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> DeleteConfirmed(string id)
          {
               var user = await _userManager.FindByIdAsync(id);
               if (user == null)
               {
                    return NotFound();
               }

               // Check if the user has any associated tickets
               var userTickets = await _context.Tickets.Where(t => t.UserId == user.Id).ToListAsync();
               if (userTickets.Any())
               {
                    // Redirect to a custom error page if tickets exist
                    TempData["ErrorMessage"] = "This user has tickets. Please delete their tickets first.";
                    return RedirectToAction(nameof(DeleteError), new { userId = user.Id });
               }

               // Delete the user
               var result = await _userManager.DeleteAsync(user);
               if (result.Succeeded)
               {
                    TempData["SuccessMessage"] = "User successfully deleted.";
                    return RedirectToAction(nameof(Index));
               }

               TempData["ErrorMessage"] = "An error occurred while deleting the user.";
               return RedirectToAction(nameof(Index));
          }

          // GET: Users/DeleteError
          public IActionResult DeleteError(string userId)
          {
               ViewData["UserId"] = userId;
               return View();
          }
     }
}
