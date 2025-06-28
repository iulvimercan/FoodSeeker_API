using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class FavoriteDonator
    {
        // Primary key with identity auto-increment.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long FavouriteId { get; set; }

        // Foreign key to the User entity who is the seeker marking the donator as favorite.
        // Required field.
        [Required, ForeignKey("Seeker")]
        public long SeekerId { get; set; }

        // Foreign key to the DonatorProfile entity which is being favorited.
        // Required field.
        [Required, ForeignKey("DonatorProfile")]
        public long DonatorId { get; set; }

        // Timestamp indicating when this favorite was created.
        // Defaults to current UTC time when the entity is instantiated.
        [Required]
        public DateTime FavoritedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to the User entity representing the seeker.
        public User? Seeker { get; set; }

        // Navigation property to the DonatorProfile entity representing the favorited donator.
        public DonatorProfile? DonatorProfile { get; set; }
    }
}
