namespace FoodSeekerAPI.DTO.Common
{
    // DTO representing a user's favorite donator entry,
    // including when it was favorited and the donator's profile details.
    public class FavoriteDonatorDto
    {
        // Unique identifier for this favorite entry.
        public required long FavoriteId { get; set; }

        // Timestamp indicating when the donator was favorited.
        public required DateTime FavoritedAt { get; set; }

        // The profile details of the favorited donator.
        public required DonatorProfileDto DonatorProfile { get; set; }
    }
}
