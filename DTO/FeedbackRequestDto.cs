namespace FoodSeekerAPI.DTO
{
    // DTO for submitting feedback about a donator.
    public class FeedbackRequestDto
    {
        // Optional: ID of the user giving the feedback.
        public long? FromUserId { get; set; }
        
        // Required: ID of the donator receiving the feedback.
        public long DonatorId { get; set; }
        
        // Required: Rating score provided by the user.
        public required int Rating { get; set; }
        
        // Optional: Additional comment or review.
        public string? Comment { get; set; }
    }
}
