namespace FoodSeekerAPI.DTO.FoodItem
{
    // DTO for creating a new food item (donator provides details).
    public class CreateFoodItemRequestDto
    {
        // Name of the food item (required).
        public required string Name { get; set; }
        
        // Description of the food item (required).
        public required string Description { get; set; }
        
        // Indicates if the food is meant to be eaten in the restaurant (default: false).
        public bool IsEatIn { get; set; } = false;
        
        // Indicates if the food is available for take away (default: false).
        public bool IsTakeAway { get; set; } = false;
        
        // Indicates if the seeker should bring their own pack/container (default: false).
        public bool IsBringPack { get; set; } = false;
        
        // Optional URL for the food item photo.
        public string? PhotoUrl { get; set; } 
    }
}
