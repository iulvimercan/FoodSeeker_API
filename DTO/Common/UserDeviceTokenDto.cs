namespace FoodSeekerAPI.DTO.Common
{
    // Data Transfer Object for saving or deleting a user's device token.
    // This token is used to send push notifications to the user's device.
    public class UserDeviceTokenDto
    {
        // The device token used for push notifications (FCM/APNs token).
        public required string DeviceToken { get; set; }

        // The platform of the device (e.g., "iOS", "Android").
        public string? Platform { get; set; }
    }
}
