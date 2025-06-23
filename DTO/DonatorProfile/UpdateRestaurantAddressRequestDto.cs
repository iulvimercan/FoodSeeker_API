namespace FoodSeekerAPI.DTO.DonatorProfile;

public class UpdateRestaurantAddressRequestDto
{
    public required string Address { get; set; }
    public required string AddressStreet { get; set; }
    public required string AddressMunicipality { get; set; }
    public required string AddressCity { get; set; }
    public required string AddressCountry { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
}