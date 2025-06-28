namespace FoodSeekerAPI.DTO
{
    // DTO for sending a push notification to a user.
    public class SendNotificationRequestDto
    {
        // The ID of the user to whom the notification will be sent.
        public required long UserId { get; set; }

        // The title of the notification.
        public required string Title { get; set; }

        // The body content of the notification. Optional, defaults to empty string.
        public string Body { get; set; } = string.Empty;

        // Optional dictionary for any additional data to send with the notification.
        public Dictionary<string, string>? Data { get; set; }
    }
}
