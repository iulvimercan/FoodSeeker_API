using FoodSeekerAPI.DTO.User;

namespace FoodSeekerAPI.DTO.Common;

public class PickupIntentDto
{
    public required long IntentId { get; set; }
    public required FoodItemDto FoodItem { get; set; }
    public required DateTime CreatedAt { get; set; }
    public UserProfileDto? FoodSeeker { get; set; }
}