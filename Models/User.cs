using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class User
    {
        // Primary key, auto-generated identity column
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserId { get; set; }

        // Full name of the user, required, max length 100 characters
        [Required, MaxLength(100)]
        public required string FullName { get; set; }

        // Email address of the user, required and validated as email format
        [Required, EmailAddress]
        public required string Email { get; set; }

        // Hashed password string, required, max length 60 (e.g. bcrypt hash length)
        [Required, MaxLength(60)]
        public required string PasswordHash { get; set; }

        // Optional URL for the user's profile photo, max length 256 characters
        [MaxLength(256)]
        public string? ProfilePhotoUrl { get; set; }

        // Indicates whether the user has verified their account (e.g. email verification)
        public bool IsVerified { get; set; }

        // Indicates whether the user is a donator (restaurant owner) or seeker (food seeker)
        public bool IsDonator { get; set; }

        // Date and time when the user account was created, defaults to UTC now
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to the DonatorProfile, if the user is a donator
        public DonatorProfile? DonatorProfile { get; set; }

        // Collection of pickup intents made by the user (as a seeker)
        public ICollection<PickupIntent> PickupIntents { get; set; } = new List<PickupIntent>();

        // Collection of feedback entries written by the user
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

        // Collection of donators this user has marked as favorite
        public ICollection<FavoriteDonator> FavoriteDonators { get; set; } = new List<FavoriteDonator>();

        // Collection of device tokens associated with the user for push notifications
        public ICollection<UserDeviceToken> DeviceTokens { get; set; } = new List<UserDeviceToken>();

        // Collection of notifications received by the user
        public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
    }
}
