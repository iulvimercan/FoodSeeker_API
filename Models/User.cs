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

        [Required, MaxLength(100)] public required string FullName { get; set; }

        [Required, EmailAddress] public required string Email { get; set; }

        [Required, MaxLength(60)] public required string PasswordHash { get; set; }

        [MaxLength(256)] public string? ProfilePhotoUrl { get; set; }

        public bool IsVerified { get; set; }

        public bool IsDonator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public DonatorProfile? DonatorProfile { get; set; }
        public ICollection<PickupIntent>? PickupIntents { get; set; }
        public ICollection<Feedback>? Feedbacks { get; set; }
        public ICollection<FavoriteDonator>? FavoriteDonators { get; set; }
        public ICollection<UserDeviceToken>? DeviceTokens { get; set; }
        public ICollection<NotificationLog>? NotificationLogs { get; set; }
    }
}