using FirebaseAdmin.Messaging;

namespace FoodSeekerAPI.Services;

public class NotificationService
{
    public async Task<string> SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null)
    {
        var message = new Message
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data ?? new Dictionary<string, string>(),
        };

        var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        return response;
    }
}
