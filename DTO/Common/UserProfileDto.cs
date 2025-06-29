using FoodSeekerAPI.DTO.Common;

namespace FoodSeekerAPI.DTO.User
{
    // Data Transfer Object representing the profile of a user in the system.
    // This includes basic user information and optionally donator-specific details.
    public class UserProfileDto
    {
        // Unique identifier of the user.
        public required long UserId { get; set; }

        // Full name of the user.
        public required string FullName { get; set; }

        // Email address of the user.
        public required string Email { get; set; }

        // Indicates whether the user is a donator (true) or a food seeker (false).
        public required bool IsDonator { get; set; }

        // Optional URL to the user's profile photo.
        public string? ProfilePhotoUrl { get; set; }

        // If the user is a donator, this will contain their donator-specific profile data.
        public DonatorProfileDto? DonatorProfile { get; set; }
    }
}
