namespace FoodSeekerAPI.DTO;

public class SendNotificationRequestDto
{
    public required long UserId { get; set; } 
    public required string Title { get; set; }
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, string>? Data { get; set; }
}
