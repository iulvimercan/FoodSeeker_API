namespace FoodSeekerAPI.DTO.DonatorProfile
{
    // DTO used to update the restaurant (donator's) address and location information.
    public class UpdateRestaurantAddressRequestDto
    {
        // Full formatted address string (can include building, floor, etc.)
        public required string Address { get; set; }

        // Street name of the restaurant's location.
        public required string AddressStreet { get; set; }

        // Municipality or local district of the address.
        public required string AddressMunicipality { get; set; }

        // City where the restaurant is located.
        public required string AddressCity { get; set; }

        // Country where the restaurant is located.
        public required string AddressCountry { get; set; }

        // Latitude coordinate for the restaurant's location on the map.
        public required double Latitude { get; set; }

        // Longitude coordinate for the restaurant's location on the map.
        public required double Longitude { get; set; }
    }
}
