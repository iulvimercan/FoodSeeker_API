namespace FoodSeekerAPI.DTO.User
{
    // DTO for updating the user's full name.
    public class UpdateUserNameRequestDto
    {
        // New full name (required).
        public required string FullName { get; set; }
    }
}
