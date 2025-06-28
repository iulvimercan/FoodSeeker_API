namespace FoodSeekerAPI.DTO.Auth
{
    // DTO for food seeker signup request.
    // Contains basic user registration information for food seekers.
    public class SignUpFoodSeekerRequestDto
    {
        // Full name of the food seeker.
        public required string FullName { get; set; }

        // Email address used for login and communication.
        public required string Email { get; set; }

        // Password for account authentication.
        public required string Password { get; set; }

        // Optional URL for the user's profile photo.
        public string? ProfilePhotoUrl { get; set; }
    }
}
