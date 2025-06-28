using FirebaseAdmin.Messaging;

namespace FoodSeekerAPI.Services;

public class NotificationService
{
    /// <summary>
    /// Sends a push notification via Firebase Cloud Messaging (FCM) to a specific device token.
    /// </summary>
    /// <param name="deviceToken">The recipient device's FCM token.</param>
    /// <param name="title">The notification title shown on the device.</param>
    /// <param name="body">The notification body text shown on the device.</param>
    /// <param name="data">Optional key-value pairs for additional custom data.</param>
    /// <returns>The message ID string returned by FCM upon successful send.</returns>
    public async Task<string> SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null)
    {
        // Construct the FCM message with target device token, notification content, and optional data payload
        var message = new Message
        {
            Token = deviceToken,  // Target device
            Notification = new Notification
            {
                Title = title,    // Notification title
                Body = body       // Notification message content
            },
            Data = data ?? new Dictionary<string, string>(), // Additional data payload or empty dictionary
        };

        // Send the message asynchronously via FirebaseMessaging default instance
        var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

        // Return the response message ID from Firebase for tracking or logging
        return response;
    }
}
