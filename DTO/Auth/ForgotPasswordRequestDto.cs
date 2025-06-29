namespace FoodSeekerAPI.DTO.Auth
{
    // DTO for Forgot Password request containing the user's email address.
    // Used when a user requests to reset their password by providing their registered email.
    public class ForgotPasswordRequestDto
    {
        // The email address of the user requesting the password reset.
        public required string Email { get; set; }
    }
}
