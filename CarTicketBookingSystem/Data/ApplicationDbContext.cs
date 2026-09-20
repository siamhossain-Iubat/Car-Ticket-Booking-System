
using CarTicketBookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarTicketBookingSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TravelRoute> TravelRoutes => Set<TravelRoute>();

        public DbSet<Booking> Bookings => Set<Booking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.TravelRoute)
                .WithMany()
                .HasForeignKey(b => b.TravelRouteId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TravelRoute>().HasData(
           new TravelRoute
           {
               Id = 1,
               From = "Dhaka",
               To = "Chattogram",
               Fare = 800,
               TravelTime = "8h"
           },
    new TravelRoute
    {
        Id = 2,
        From = "Dhaka",
        To = "Sylhet",
        Fare = 700,
        TravelTime = "6h"
    },
    new TravelRoute
    {
        Id = 3,
        From = "Dhaka",
        To = "Rajshahi",
        Fare = 600,
        TravelTime = "7h"
    },
    new TravelRoute
    {
        Id = 4,
        From = "Dhaka",
        To = "Khulna",
        Fare = 650,
        TravelTime = "7h"
    },
    new TravelRoute
    {
        Id = 5,
        From = "Dhaka",
        To = "Barishal",
        Fare = 550,
        TravelTime = "6h"
    },
    new TravelRoute
    {
        Id = 6,
        From = "Chattogram",
        To = "Cox's Bazar",
        Fare = 400,
        TravelTime = "4h"
    }
            );
        }
    }
}