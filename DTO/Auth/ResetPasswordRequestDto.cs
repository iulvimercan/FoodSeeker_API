namespace FoodSeekerAPI.DTO.Auth
{
    // DTO for resetting user password.
    // Contains the reset token and the new password provided by the user.
    public class ResetPasswordRequestDto
    {
        // The password reset token sent to the user's email for verification.
        public required string Token { get; set; }

        // The new password that the user wants to set.
        public required string NewPassword { get; set; }
    }
}
