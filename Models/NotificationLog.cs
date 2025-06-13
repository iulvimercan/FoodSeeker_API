using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class NotificationLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long NotificationId { get; set; }

        [Required, ForeignKey("User")]
        public long UserId { get; set; }

        [MaxLength(255)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsRead { get; set; } = false;

        // Navigation property
        public User? User { get; set; }
    }
}