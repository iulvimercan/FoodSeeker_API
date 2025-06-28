using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class UserDeviceToken
    {
        // Primary key, auto-generated identity column for each device token record
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TokenId { get; set; }

        // Foreign key to the User entity this device token belongs to
        [Required, ForeignKey("User")]
        public long UserId { get; set; }

        // The unique device token string used for push notifications, max length 512 characters
        [Required, MaxLength(512)]
        public required string DeviceToken { get; set; }

        // Optional platform identifier for the device (e.g., 'iOS', 'Android')
        [MaxLength(50)]
        public string? Platform { get; set; }

        // The date and time when this device token was created/registered, defaults to current UTC time
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to the related User entity
        public User? User { get; set; }
    }
}
