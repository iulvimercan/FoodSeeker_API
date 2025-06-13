namespace FoodSeekerAPI.DTO.Common;

public class UserDeviceTokenDto
{
    public required string DeviceToken { get; set; }
    public string? Platform { get; set; }  // e.g., 'iOS', 'Android'
}