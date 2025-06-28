namespace FoodSeekerAPI.DTO.Common
{
    // Data Transfer Object representing a food item available for donation.
    public class FoodItemDto
    {
        // Unique identifier for the food item.
        public required long FoodId { get; set; }

        // Name or title of the food item.
        public required string FoodName { get; set; }

        // Optional description providing more details about the food.
        public string? Description { get; set; }

        // Indicates if the food is meant to be eaten on-site.
        public required bool IsEatIn { get; set; }

        // Indicates if the food can be taken away by the seeker.
        public required bool IsTakeAway { get; set; }

        // Indicates if the seeker should bring their own packaging.
        public required bool IsBringPack { get; set; }

        // Optional URL to a photo representing the food item.
        public string? PhotoUrl { get; set; }

        // Indicates whether the food item is currently active and available.
        public required bool IsActive { get; set; }

        // Timestamp marking when the food item was created/listed.
        public DateTime CreatedAt { get; set; }

        // Optional nested donator profile information related to this food item.
        public DonatorProfileDto? DonatorProfile { get; set; }
    }
}
