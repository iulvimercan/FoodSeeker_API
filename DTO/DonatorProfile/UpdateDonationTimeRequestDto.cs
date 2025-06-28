namespace FoodSeekerAPI.DTO.DonatorProfile
{
    // DTO used to update the daily donation time range for a donator profile.
    public class UpdateDonationTimeRequestDto
    {
        // The time when donations start being available. Nullable for partial updates.
        public TimeOnly? DonationStarts { get; set; }

        // The time when donations stop being available. Nullable for partial updates.
        public TimeOnly? DonationEnds { get; set; }
    }
}
