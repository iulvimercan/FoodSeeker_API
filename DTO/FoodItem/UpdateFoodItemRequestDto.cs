namespace FoodSeekerAPI.DTO.FoodItem
{
    // DTO used to update existing food item details.
    public class UpdateFoodItemRequestDto
    {
        // Updated name of the food item (required).
        public required string Name { get; set; }

        // Updated description of the food item (required).
        public required string Description { get; set; }

        // Indicates whether the food is for eating inside the restaurant (default: false).
        public bool IsEatIn { get; set; } = false;

        // Indicates whether the food is available for take away (default: false).
        public bool IsTakeAway { get; set; } = false;

        // Indicates whether the food seeker should bring their own container (default: false).
        public bool IsBringPack { get; set; } = false;

        // Optional updated URL of the food item photo.
        public string? PhotoUrl { get; set; }
    }
}
