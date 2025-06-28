namespace FoodSeekerAPI.DTO.Auth
{
    // DTO for user login request containing credentials.
    // Used to pass email and password from client to backend for authentication.
    public class LoginRequestDto
    {
        // The user's registered email address.
        public required string Email { get; set; }

        // The user's password.
        public required string Password { get; set; }
    }
}
