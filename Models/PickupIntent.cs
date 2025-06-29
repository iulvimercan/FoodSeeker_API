using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class PickupIntent
    {
        // Primary key, auto-generated identity column.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IntentId { get; set; }

        // Foreign key referencing the FoodItem that the seeker intends to pick up.
        [Required, ForeignKey("FoodItem")]
        public long FoodId { get; set; }

        // Foreign key referencing the User who is the seeker expressing interest.
        [Required, ForeignKey("User")]
        public long SeekerId { get; set; }

        // Timestamp when the pickup intent was created, defaults to UTC now.
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to the associated FoodItem entity.
        public FoodItem? FoodItem { get; set; }

        // Navigation property to the User (seeker) entity.
        public User? User { get; set; }
    }
}
