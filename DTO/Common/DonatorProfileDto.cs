namespace FoodSeekerAPI.DTO.Common
{
    // DTO representing a donator's profile details,
    // including restaurant info, location, donation times, and ratings.
    public class DonatorProfileDto
    {
        // Unique identifier for the donator profile.
        public required long DonatorId { get; set; }

        // Name of the restaurant owned by the donator.
        public required string RestaurantName { get; set; } = string.Empty;

        // Optional URL to the donator's profile photo.
        public string? ProfilePhotoUrl { get; set; }

        // General address of the restaurant.
        public required string Address { get; set; } = string.Empty;

        // Street component of the restaurant address.
        public required string AddressStreet { get; set; } = string.Empty;

        // Municipality component of the restaurant address.
        public required string AddressMunicipality { get; set; } = string.Empty;

        // City component of the restaurant address.
        public required string AddressCity { get; set; } = string.Empty;

        // Country component of the restaurant address.
        public required string AddressCountry { get; set; } = string.Empty;

        // Latitude coordinate of the restaurant location.
        public required double Latitude { get; set; }

        // Longitude coordinate of the restaurant location.
        public required double Longitude { get; set; }

        // Optional: distance from a reference point (e.g., user's location), if applicable.
        public double? Distance { get; set; } 

        // Donation start time in string format ("HH:mm") to align with Dart frontend.
        public required string DonationStarts { get; set; } = string.Empty;

        // Donation end time in string format ("HH:mm") to align with Dart frontend.
        public required string DonationEnds { get; set; } = string.Empty;

        // Average user rating score of the donator.
        public required double AverageScore { get; set; }

        // Number of users who have favorited this donator.
        public required int FavoritesCount { get; set; }

        // Optional: number of food items currently available from the donator.
        public int? FoodItemsCount { get; set; }
    }
}
