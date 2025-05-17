using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class PickupIntent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IntentId { get; set; }

        [Required, ForeignKey("FoodItem")]
        public long FoodId { get; set; }

        [Required, ForeignKey("User")]
        public long SeekerId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public required FoodItem FoodItem { get; set; }
        public required User User { get; set; }
    }
}