namespace FoodSeekerAPI.DTO.Common
{
    // Data Transfer Object representing feedback given by a user to a donator.
    public class FeedbackDto
    {
        // Unique identifier for the feedback entry.
        public required long FeedbackId { get; set; }

        // User ID of the person who gave the feedback.
        public required long FromUserId { get; set; }

        // Full name of the user who provided the feedback.
        public required string UserFullName { get; set; }

        // Optional URL to the profile photo of the user giving feedback.
        public string? UserProfilePhotoUrl { get; set; }

        // Rating score given by the user (e.g., 1 to 5).
        public required int Rating { get; set; }

        // Optional textual comment left by the user.
        public string? Comment { get; set; }

        // Donator's user ID who received the feedback.
        public required long DonatorId { get; set; }

        // Name of the restaurant or donator associated with the feedback.
        public required string RestaurantName { get; set; }

        // Optional URL to the donator's profile photo.
        public string? DonatorProfilePhotoUrl { get; set; }

        // Timestamp when the feedback was created.
        public DateTime CreatedAt { get; set; }
    }
}
