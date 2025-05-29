using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Components;

namespace FoodSeekerAPI.Models
{
    public class DonatorProfile
    {
        [Key, ForeignKey("User")]
        public long DonatorId { get; set; }

        [Required, MaxLength(100)]
        public required string RestaurantName { get; set; }

        [MaxLength(500)]
        public required string Address { get; set; }
        
        [MaxLength(64)]
        public required string AddressStreet { get; set; } // cadde
        
        [MaxLength(64)]
        public required string AddressMunicipality { get; set; } // ilçe
        
        [MaxLength(64)]
        public required string AddressCity { get; set; } // il
        
        [MaxLength(64)]
        public required string AddressCountry { get; set; } // ülke

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
        public User? User { get; set; }
    }
}