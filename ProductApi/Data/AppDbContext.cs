using Microsoft.EntityFrameworkCore;
using ProductApi.Models;

namespace ProductApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // Category 1---* Event: deleting a category is blocked while it still has events.
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Events)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // User 1---* Event (organizer): deleting the organizer is blocked while they still own events.
        // Restrict (not Cascade) also avoids a multiple-cascade-path conflict with User -> Booking below,
        // since Event -> Booking is already a cascade path back to the same User.
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organizer)
            .WithMany(u => u.OrganizedEvents)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        // User 1---* Booking: deleting a user removes their bookings.
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Event 1---* Booking: deleting an event removes its bookings.
        // Together with User <-> Booking, this makes User and Event effectively
        // many-to-many, joined through Booking (which carries its own attributes:
        // BookingDate, NumberOfSeats, Status).
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Conference", Description = "Multi-day professional or academic conferences." },
            new Category { Id = 2, Name = "Workshop", Description = "Hands-on, skill-focused sessions." },
            new Category { Id = 3, Name = "Concert", Description = "Live music performances." },
            new Category { Id = 4, Name = "Sports", Description = "Sporting events and tournaments." },
            new Category { Id = 5, Name = "Networking", Description = "Meetups and social/professional mixers." }
        );
    }
}
