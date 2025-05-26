namespace FoodSeekerAPI.DTO.Auth;

public class SignUpFoodSeekerRequestDto
{
    public required String FullName { get; set; }
    public required String Email { get; set; }
    public required String Password { get; set; }
    // public String? ProfilePhotoUrl { get; set; } // todo - Deal with CDN and image upload later
}