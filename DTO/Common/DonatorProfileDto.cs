namespace FoodSeekerAPI.DTO.Common;

public class DonatorProfileDto
{
    public required long DonatorId { get; set; }
    public required string RestaurantName { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
    public required string Address { get; set; } = string.Empty;
    public required string AddressStreet { get; set; } = string.Empty;
    public required string AddressMunicipality { get; set; } = string.Empty;
    public required string AddressCity { get; set; } = string.Empty;
    public required string AddressCountry { get; set; } = string.Empty;
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }

    // Use string to match Dart structure ("HH:mm")
    public required  string DonationStarts { get; set; } = string.Empty;
    public required  string DonationEnds { get; set; } = string.Empty;
 
    public required  double AverageScore { get; set; }
    public required  int FavoritesCount { get; set; }
}