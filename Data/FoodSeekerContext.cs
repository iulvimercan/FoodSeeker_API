using Microsoft.EntityFrameworkCore;
using FoodSeekerAPI.Models;

namespace FoodSeekerAPI.Data
{
    public class FoodSeekerContext(DbContextOptions<FoodSeekerContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<DonatorProfile> DonatorProfiles { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<PickupIntent> PickupIntents { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<FavoriteDonator> FavoriteDonators { get; set; }
        public DbSet<UserDeviceToken> UserDeviceTokens { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }

    }
}