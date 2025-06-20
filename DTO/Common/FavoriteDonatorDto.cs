namespace FoodSeekerAPI.DTO.Common;

public class FavoriteDonatorDto
{
    public required long FavoriteId { get; set; }
    public required DateTime FavoritedAt { get; set; }
    public required DonatorProfileDto DonatorProfile { get; set; }
    
}