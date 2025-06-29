namespace FoodSeekerAPI.DTO.Auth
{
    // DTO for donator signup request.
    // Captures user info and donator-specific restaurant details for registration.
    public class SignUpDonatorRequestDto
    {
        // Full name of the donator (user).
        public required string FullName { get; set; }

        // Email address used for login and communication.
        public required string Email { get; set; }

        // Password for account authentication.
        public required string Password { get; set; }

        // Optional URL to the donator's profile photo.
        public string? ProfilePhotoUrl { get; set; }

        // Name of the restaurant for the donator profile.
        public required string RestaurantName { get; set; }

        // General address of the restaurant.
        public required string RestaurantAddress { get; set; }

        // Street part of the restaurant's address (optional, defaults to empty string).
        public required string RestaurantAddressStreet { get; set; } = "";

        // Municipality part of the restaurant's address (optional).
        public required string RestaurantAddressMunicipality { get; set; } = "";

        // City where the restaurant is located (optional).
        public required string RestaurantAddressCity { get; set; } = "";

        // Country where the restaurant is located (optional).
        public required string RestaurantAddressCountry { get; set; } = "";

        // Latitude coordinate of the restaurant location.
        public required string RestaurantLatitude { get; set; }

        // Longitude coordinate of the restaurant location.
        public required string RestaurantLongitude { get; set; }

        // Time when donations start (e.g., daily).
        public required TimeOnly DonationStarts { get; set; }

        // Time when donations end (e.g., daily).
        public required TimeOnly DonationEnds { get; set; }
    }
}
