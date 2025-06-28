namespace FoodSeekerAPI.DTO.Common
{
    // Data Transfer Object representing a notification sent to a user.
    public class NotificationLogDto
    {
        // Unique identifier for the notification log entry.
        public required long NotificationId { get; set; }

        // Title of the notification shown to the user.
        public required string Title { get; set; }

        // Body message/content of the notification.
        public required string Message { get; set; }

        // The date and time the notification was sent.
        public required DateTime SentAt { get; set; }

        // Indicates whether the user has read this notification.
        public required bool IsRead { get; set; }
    }
}
