namespace FoodSeekerAPI.DTO.User
{
    // DTO for updating the user's password.
    public class UpdateUserPasswordRequestDto
    {
        // New password (required).
        public required string Password { get; set; }
    }
}
