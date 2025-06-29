using FoodSeekerAPI.DTO.User;

namespace FoodSeekerAPI.DTO.Common
{
    // Data Transfer Object representing a pickup intent made by a food seeker for a specific food item.
    public class PickupIntentDto
    {
        // Unique identifier for the pickup intent.
        public required long IntentId { get; set; }

        // The food item associated with the pickup intent.
        public required FoodItemDto FoodItem { get; set; }

        // Timestamp indicating when the pickup intent was created.
        public required DateTime CreatedAt { get; set; }

        // (Optional) Profile information of the food seeker who made the pickup intent.
        // This field is populated only when the request is made by a donator.
        public UserProfileDto? FoodSeeker { get; set; }
    }
}
