using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ETicketApp.Models;
using ETicketApp.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure the TicketingContext with PostgreSQL
builder.Services.AddDbContext<TicketingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TicketingDatabase")));

// Configure Identity services
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
     options.SignIn.RequireConfirmedAccount = false;
     options.Password.RequireDigit = true;
     options.Password.RequiredLength = 6;
     options.Password.RequireNonAlphanumeric = false;
     options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<TicketingContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Register the EmailSender service
builder.Services.AddSingleton<IEmailSender, EmailSender>();

// Register the QrCodeService
builder.Services.AddScoped<QrCodeService>();

// Explicitly configure listening URLs for development
builder.WebHost.UseUrls("http://localhost:5179", "https://localhost:7200");

var app = builder.Build();

// Seed roles and create default admin user
await SeedDatabase(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
     app.UseExceptionHandler("/Home/Error");
     app.UseHsts();
}
else
{
     app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add Identity Middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

// Method to seed roles and a default admin user in the database
async Task SeedDatabase(WebApplication app)
{
     using (var scope = app.Services.CreateScope())
     {
          var services = scope.ServiceProvider;
          var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
          var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

          string[] roleNames = { "Admin", "Organizer", "User" };
          foreach (var roleName in roleNames)
          {
               if (!await roleManager.RoleExistsAsync(roleName))
               {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
               }
          }

          string adminEmail = "admin@eticketapp.com";
          string adminPassword = "Admin@123";
          if (await userManager.FindByEmailAsync(adminEmail) == null)
          {
               var adminUser = new IdentityUser
               {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
               };

               var createResult = await userManager.CreateAsync(adminUser, adminPassword);
               if (createResult.Succeeded)
               {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
               }
          }
     }
}
