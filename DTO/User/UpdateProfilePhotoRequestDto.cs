namespace FoodSeekerAPI.DTO.User
{
    // DTO for updating the user's profile photo URL.
    public class UpdateProfilePhotoRequestDto
    {
        // New profile photo URL (required).
        public required string ProfilePhotoUrl { get; set; }
    }
}
