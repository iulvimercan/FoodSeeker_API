using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserId { get; set; }

        [Required, MaxLength(100)]
        public required string FullName { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, MaxLength(60)]
        public required string PasswordHash { get; set; }

        [MaxLength(256)]
        public string? ProfilePhotoUrl { get; set; }

        public bool IsDonator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public required DonatorProfile DonatorProfile { get; set; }
        public required ICollection<PickupIntent> PickupIntents { get; set; }
        public required ICollection<Feedback> Feedbacks { get; set; }
        public required ICollection<FavoriteDonator> Favorites { get; set; }
        public required ICollection<UserDeviceToken> DeviceTokens { get; set; }
        public required ICollection<NotificationLog> NotificationLogs { get; set; }
    }
}