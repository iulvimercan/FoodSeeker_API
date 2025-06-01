namespace FoodSeekerAPI.DTO.FoodItem;

public class UpdateFoodItemRequestDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public bool IsEatIn { get; set; } = false;
    public bool IsTakeAway { get; set; } = false;
    public bool IsBringPack { get; set; } = false;
    public string? PhotoUrl { get; set; }
}