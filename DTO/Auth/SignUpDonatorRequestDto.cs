namespace FoodSeekerAPI.DTO.Auth;

public class SignUpDonatorRequestDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public required string RestaurantName { get; set; }
    public required string RestaurantAddress { get; set; }
    public required string RestaurantAddressStreet { get; set; } = "";
    public required string RestaurantAddressMunicipality { get; set; } = "";
    public required string RestaurantAddressCity { get; set; } = "";
    public required string RestaurantAddressCountry { get; set; } = "";
    public required string RestaurantLatitude { get; set; }
    public required string RestaurantLongitude { get; set; }
    public required TimeOnly DonationStarts { get; set; }
    public required TimeOnly DonationEnds { get; set; }
}