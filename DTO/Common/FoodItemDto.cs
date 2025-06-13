namespace FoodSeekerAPI.DTO.Common;

public class FoodItemDto
{
    public required long FoodId { get; set; }
    public required string FoodName { get; set; }
    public string? Description { get; set; }
    public required bool IsEatIn { get; set; }
    public required bool IsTakeAway { get; set; }
    public required bool IsBringPack { get; set; }
    public string? PhotoUrl { get; set; }
    public required bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DonatorProfileDto? DonatorProfile { get; set; }
}
