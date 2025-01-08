using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ETicketApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ETicketApp.Areas.Identity.Pages.Account.Manage
{
     public class DeletePersonalDataModel : PageModel
     {
          private readonly UserManager<IdentityUser> _userManager;
          private readonly SignInManager<IdentityUser> _signInManager;
          private readonly TicketingContext _context;
          private readonly ILogger<DeletePersonalDataModel> _logger;

          public DeletePersonalDataModel(
              UserManager<IdentityUser> userManager,
              SignInManager<IdentityUser> signInManager,
              TicketingContext context,
              ILogger<DeletePersonalDataModel> logger)
          {
               _userManager = userManager;
               _signInManager = signInManager;
               _context = context;
               _logger = logger;
          }

          [BindProperty]
          public InputModel Input { get; set; }

          public bool RequirePassword { get; set; }

          public class InputModel
          {
               [Required]
               [DataType(DataType.Password)]
               public string Password { get; set; }
          }

          public async Task<IActionResult> OnGet()
          {
               var user = await _userManager.GetUserAsync(User);
               if (user == null)
               {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
               }

               RequirePassword = await _userManager.HasPasswordAsync(user);
               return Page();
          }

          public async Task<IActionResult> OnPostAsync()
          {
               var user = await _userManager.GetUserAsync(User);
               if (user == null)
               {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
               }

               RequirePassword = await _userManager.HasPasswordAsync(user);
               if (RequirePassword)
               {
                    if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                    {
                         ModelState.AddModelError(string.Empty, "Incorrect password.");
                         return Page();
                    }
               }

               // Delete related tickets before deleting the user
               var userTickets = await _context.Tickets.Where(t => t.UserId == user.Id).ToListAsync();
               _context.Tickets.RemoveRange(userTickets);
               await _context.SaveChangesAsync();

               // Delete the user
               var result = await _userManager.DeleteAsync(user);
               var userId = await _userManager.GetUserIdAsync(user);
               if (!result.Succeeded)
               {
                    throw new InvalidOperationException($"Unexpected error occurred deleting user.");
               }

               await _signInManager.SignOutAsync();

               _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

               return Redirect("~/");
          }
     }
}
