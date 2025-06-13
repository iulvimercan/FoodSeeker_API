namespace FoodSeekerAPI.DTO.Common;

public class NotificationLogDto
{
    public required long NotificationId { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public required DateTime SentAt { get; set; }
    public required bool IsRead { get; set; }
}