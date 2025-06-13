namespace FoodSeekerAPI.DTO.Common;

public class FeedbackDto
{
    public required long FeedbackId { get; set; }
    public required long FromUserId { get; set; }
    public required string UserFullName { get; set; }
    public string? UserProfilePhotoUrl { get; set; }
    public required int Rating { get; set; } 
    public string? Comment { get; set; }
    public required long DonatorId { get; set; }
    public required string RestaurantName { get; set; }
    public string? DonatorProfilePhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}