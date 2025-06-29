using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class NotificationLog
    {
        // Primary key, auto-generated identity column.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long NotificationId { get; set; }

        // Foreign key referencing the User who received the notification.
        [Required, ForeignKey("User")]
        public long UserId { get; set; }

        // Optional title of the notification, max length 255 characters.
        [MaxLength(255)]
        public string? Title { get; set; }

        // Optional detailed message of the notification, max length 1000 characters.
        [MaxLength(1000)]
        public string? Message { get; set; }

        // Timestamp when the notification was sent, defaults to current UTC time.
        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Flag indicating whether the notification has been read by the user.
        [Required]
        public bool IsRead { get; set; } = false;

        // Navigation property to the associated User entity.
        public User? User { get; set; }
    }
}
