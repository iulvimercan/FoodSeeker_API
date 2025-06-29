namespace FoodSeekerAPI.DTO.User
{
    // DTO for updating a user's email address.
    public class UpdateUserEmailRequestDto
    {
        // New email address (required).
        public required string Email { get; set; }
    }
}
