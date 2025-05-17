using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class UserDeviceToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TokenId { get; set; }

        [Required, ForeignKey("User")]
        public long UserId { get; set; }

        [Required, MaxLength(512)]
        public required string DeviceToken { get; set; }

        [MaxLength(50)]
        public string? Platform { get; set; }  // e.g., 'iOS', 'Android'

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public required User User { get; set; }
    }
}