using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class FavoriteDonator
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FavouriteId { get; set; }

        [Required, ForeignKey("Seeker")]
        public long SeekerId { get; set; }

        [Required, ForeignKey("DonatorProfile")]
        public long DonatorId { get; set; }

        [Required]
        public DateTime FavoritedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public required User Seeker { get; set; }
        public required DonatorProfile DonatorProfile { get; set; }
    }
}