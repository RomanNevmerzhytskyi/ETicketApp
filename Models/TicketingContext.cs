//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;

//namespace ETicketApp.Models
//{
//     public class TicketingContext : IdentityDbContext
//     {
//          public TicketingContext(DbContextOptions<TicketingContext> options) : base(options) { }

//          public DbSet<Ticket> Tickets { get; set; }
//          public DbSet<Event> Events { get; set; }

//          protected override void OnModelCreating(ModelBuilder modelBuilder)
//          {
//               base.OnModelCreating(modelBuilder);

//               // Configure Ticket to User relationship (nullable UserId)
//               modelBuilder.Entity<Ticket>()
//                   .HasOne(t => t.User)
//                   .WithMany()
//                   .HasForeignKey(t => t.UserId)
//                   .IsRequired(false)
//                   .OnDelete(DeleteBehavior.Cascade);  // Cascade delete to remove related tickets when user is deleted

//               // Configure Ticket to Event relationship
//               modelBuilder.Entity<Ticket>()
//                   .HasOne(t => t.Event)
//                   .WithMany(e => e.Tickets)  // Assuming the Event class has a collection of Tickets
//                   .HasForeignKey(t => t.EventId)
//                   .IsRequired();  // EventId is required

//               // Configure TicketPrice to avoid warnings about default precision
//               modelBuilder.Entity<Event>()
//                   .Property(e => e.TicketPrice)
//                   .HasColumnType("decimal(18,2)");  // Set precision and scale for TicketPrice
//          }
//     }
//}


using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ETicketApp.Models
{
     public class TicketingContext : IdentityDbContext
     {
          public TicketingContext(DbContextOptions<TicketingContext> options) : base(options) { }

          public DbSet<Ticket> Tickets { get; set; }
          public DbSet<Event> Events { get; set; }

          protected override void OnModelCreating(ModelBuilder modelBuilder)
          {
               base.OnModelCreating(modelBuilder);

               // Configure Ticket to User relationship (nullable UserId)
               modelBuilder.Entity<Ticket>()
                   .HasOne(t => t.User)
                   .WithMany()
                   .HasForeignKey(t => t.UserId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Cascade);

               // Configure Ticket to Event relationship
               modelBuilder.Entity<Ticket>()
                   .HasOne(t => t.Event)
                   .WithMany(e => e.Tickets)
                   .HasForeignKey(t => t.EventId)
                   .IsRequired();

               // Configure TicketPrice to avoid warnings about default precision
               modelBuilder.Entity<Event>()
                   .Property(e => e.TicketPrice)
                   .HasColumnType("decimal(18,2)");
          }

          protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
          {
               if (!optionsBuilder.IsConfigured)
               {
                    optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=TicketingSystem;Username=postgres;Password=romchik@123R");
               }
          }
     }
}
