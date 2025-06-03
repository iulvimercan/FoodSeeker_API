namespace FoodSeekerAPI.DTO.UserDeviceToken;

public class SaveUserDeviceTokenRequestDto
{
    public required string DeviceToken { get; set; }
    public string? Platform { get; set; }  // e.g., 'iOS', 'Android'
}