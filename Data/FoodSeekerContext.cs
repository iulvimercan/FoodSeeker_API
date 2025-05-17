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

        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder);
        //
        //     modelBuilder.Entity<FavoriteDonator>()
        //         .HasOne(f => f.Seeker)
        //         .WithMany()
        //         .HasForeignKey(f => f.SeekerId)
        //         .OnDelete(DeleteBehavior.Restrict); // or .NoAction()
        //
        //     modelBuilder.Entity<Feedback>()
        //         .HasOne(f => f.FromUser)
        //         .WithMany()
        //         .HasForeignKey(f => f.FromUserId)
        //         .OnDelete(DeleteBehavior.Restrict);
        //
        //     modelBuilder.Entity<Feedback>()
        //         .HasOne<object>(f => f.Donator)
        //         .WithMany()
        //         .HasForeignKey(f => f.DonatorId)
        //         .OnDelete(DeleteBehavior.Restrict);
        //
        //     modelBuilder.Entity<PickupIntent>()
        //         .HasOne(p => p.Seeker)
        //         .WithMany()
        //         .HasForeignKey(p => p.SeekerId)
        //         .OnDelete(DeleteBehavior.Restrict);
        //
        //     modelBuilder.Entity<UserDeviceToken>()
        //         .HasOne(t => t.User)
        //         .WithMany()
        //         .HasForeignKey(t => t.UserId)
        //         .OnDelete(DeleteBehavior.Restrict);
        //
        //     modelBuilder.Entity<NotificationLog>()
        //         .HasOne(n => n.User)
        //         .WithMany()
        //         .HasForeignKey(n => n.UserId)
        //         .OnDelete(DeleteBehavior.Restrict);
        //
        //     // Add similar lines for any other conflicting FK to Users
        // }

    }
}