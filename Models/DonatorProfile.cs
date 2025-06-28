using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Components;

namespace FoodSeekerAPI.Models
{
    public class DonatorProfile
    {
        // Primary key and foreign key to the User entity.
        // Each DonatorProfile corresponds to exactly one User.
        [Key, ForeignKey("User")]
        public long DonatorId { get; set; }

        // Name of the restaurant donating food.
        // Required and max length limited to 100 characters.
        [Required, MaxLength(100)]
        public required string RestaurantName { get; set; }

        // Full address string, e.g. complete textual address.
        // Required and max length limited to 500 characters.
        [MaxLength(500)]
        public required string Address { get; set; }

        // Street name part of the address (cadde).
        // Required, max length 64 characters.
        [MaxLength(64)]
        public required string AddressStreet { get; set; }
        
        // Municipality / district part of the address (ilçe).
        // Required, max length 64 characters.
        [MaxLength(64)]
        public required string AddressMunicipality { get; set; }
        
        // City part of the address (il).
        // Required, max length 64 characters.
        [MaxLength(64)]
        public required string AddressCity { get; set; }
        
        // Country part of the address (ülke).
        // Required, max length 64 characters.
        [MaxLength(64)]
        public required string AddressCountry { get; set; }

        // Latitude coordinate for geolocation.
        // Required to facilitate map display and distance calculations.
        [Required]
        public double Latitude { get; set; }

        // Longitude coordinate for geolocation.
        // Required to facilitate map display and distance calculations.
        [Required]
        public double Longitude { get; set; }

        // Time when donations start daily.
        // Required to specify donation availability hours.
        [Required]
        public TimeOnly DonationStarts { get; set; }

        // Time when donations end daily.
        // Required to specify donation availability hours.
        [Required]
        public TimeOnly DonationEnds { get; set; }

        // Average rating score given by food seekers.
        // Can be used to indicate donator reliability or quality.
        public float AverageScore { get; set; }

        // Number of times this donator has been favorited by seekers.
        public int FavoritesCount { get; set; }

        // Navigation property linking to the User entity.
        // Represents the user account owning this donator profile.
        public User? User { get; set; }

        // Collection of FoodItems that this donator currently offers.
        // Initialized as an empty list to avoid null reference issues.
        public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
    }
}
