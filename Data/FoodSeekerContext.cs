using Microsoft.EntityFrameworkCore;
using FoodSeekerAPI.Models;

namespace FoodSeekerAPI.Data
{
    // DbContext class representing the database session for the FoodSeeker API
    // It manages the entity sets corresponding to database tables
    public class FoodSeekerContext(DbContextOptions<FoodSeekerContext> options) : DbContext(options)
    {
        // Represents the Users table in the database
        public DbSet<User> Users { get; set; }

        // Represents the DonatorProfiles table, holding profiles for donators (restaurants)
        public DbSet<DonatorProfile> DonatorProfiles { get; set; }

        // Represents the FoodItems table, holding the food donation listings
        public DbSet<FoodItem> FoodItems { get; set; }

        // Represents the PickupIntents table, tracking users' intent to pick up food items
        public DbSet<PickupIntent> PickupIntents { get; set; }

        // Represents the Feedbacks table, storing user feedback on donations or donators
        public DbSet<Feedback> Feedbacks { get; set; }

        // Represents the FavoriteDonators table, where users save their favorite donators
        public DbSet<FavoriteDonator> FavoriteDonators { get; set; }

        // Represents the UserDeviceTokens table, storing device tokens for push notifications
        public DbSet<UserDeviceToken> UserDeviceTokens { get; set; }

        // Represents the NotificationLogs table, logging notifications sent to users
        public DbSet<NotificationLog> NotificationLogs { get; set; }
    }
}
