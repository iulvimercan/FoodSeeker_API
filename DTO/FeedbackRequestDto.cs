namespace FoodSeekerAPI.DTO;

public class FeedbackRequestDto
{
    public long? FromUserId { get; set; }
    public long DonatorId { get; set; }
    public required int Rating { get; set; }
    public string? Comment { get; set; }
}