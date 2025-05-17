using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodSeekerAPI.Models
{
    public class DonatorProfile
    {
        [Key, ForeignKey("User")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long DonatorId { get; set; }

        [Required, MaxLength(100)]
        public required string RestaurantName { get; set; }

        [MaxLength(200)]
        public required string ShortAddress { get; set; }

        [MaxLength(500)]
        public required string Address { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public TimeOnly DonationStarts { get; set; }

        [Required]
        public TimeOnly DonationEnds { get; set; }

        public float AverageScore { get; set; }

        public int FavoritesCount { get; set; }

        // Navigation Property
        public required User User { get; set; }
    }
}