namespace FoodSeekerAPI.DTO.DonatorProfile
{
    // DTO for updating the name of the restaurant in a donator profile.
    public class UpdateRestaurantNameRequestDto
    {
        // New name of the restaurant (required).
        public required string RestaurantName { get; set; }
    }
}
