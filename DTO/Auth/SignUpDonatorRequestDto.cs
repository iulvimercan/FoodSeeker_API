namespace FoodSeekerAPI.DTO.Auth;

public class SignUpDonatorRequestDto
{
    public required String FullName { get; set; }
    public required String Email { get; set; }
    public required String Password { get; set; }
    // public String? ProfilePhotoUrl { get; set; } // todo - Deal with CDN and image upload later
    public required String RestaurantName { get; set; }
    public required String RestaurantLatitude { get; set; }
    public required String RestaurantLongitude { get; set; }
    public required String DonationStarts { get; set; }
    public required String DonationEnds { get; set; }
}