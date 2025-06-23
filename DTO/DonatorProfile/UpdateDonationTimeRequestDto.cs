namespace FoodSeekerAPI.DTO.DonatorProfile;

public class UpdateDonationTimeRequestDto
{
    public TimeOnly? DonationStarts { get; set; }
    public TimeOnly? DonationEnds { get; set; }
}