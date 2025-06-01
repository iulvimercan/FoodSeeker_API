using FoodSeekerAPI.DTO.Common;

namespace FoodSeekerAPI.DTO.User;

public class UserProfileDto
{
    public required long UserId { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required bool IsDonator { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public DonatorProfileDto? DonatorProfile { get; set; }
}