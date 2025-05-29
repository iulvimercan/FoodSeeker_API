namespace FoodSeekerAPI.DTO.Auth;

public class SignUpFoodSeekerRequestDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    // public String? ProfilePhotoUrl { get; set; } // todo - Deal with CDN and image upload later
}