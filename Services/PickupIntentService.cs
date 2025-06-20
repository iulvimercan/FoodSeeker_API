using FoodSeekerAPI.Data;
using FoodSeekerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Services;

public class PickupIntentService(FoodSeekerContext db, NotificationService notificationService)
{
    private readonly FoodSeekerContext _db = db;
    private readonly NotificationService _notificationService = notificationService;

    public async Task<bool> CleanOldPickupIntentsAsync()
    {
        try
        {
            // Define the threshold for old pickup intents (e.g., 30 days)
            var threshold = DateTime.UtcNow.AddHours(-6);

            // Find all pickup intents older than the threshold
            var oldIntents = _db.PickupIntents
                .Where(intent => intent.CreatedAt < threshold)
                .ToList();

            if (oldIntents.Count > 0)
            {
                // Log the removal of old pickup intents
                Console.WriteLine(
                    $"Removing {oldIntents.Count} old pickup intents older than 6 hours or inactive food items.");
                _db.PickupIntents.RemoveRange(oldIntents);
                await _db.SaveChangesAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            // Log the exception (logging mechanism not shown here)
            Console.WriteLine($"Error cleaning old pickup intents: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemovePickupIntentsByFoodIdAndNotifyAsync(long foodId)
    {
        try
        {
            // Clean old pickup intents before removing specific ones
            await CleanOldPickupIntentsAsync();

            var pickupIntents = await _db.PickupIntents
                .Where(pi => pi.FoodId == foodId)
                .Include(pi => pi.User)
                .ThenInclude(u => u.DeviceTokens)
                .Include(pi => pi.FoodItem)
                .ToListAsync();

            if (pickupIntents.Count > 0)
            {
                // Log the removal of pickup intents for the specified food item
                Console.WriteLine($"Removing {pickupIntents.Count} pickup intents for food ID {foodId}.");

                // Notify users about the removal (notification mechanism not shown here)
                foreach (var intent in pickupIntents)
                {
                    var title = "Food Item Removed";
                    var message =
                        $"The food item '{intent.FoodItem!.FoodName}' has been removed. Your pickup intent has been cancelled.";

                    // Log the notification
                    var notificationLog = new NotificationLog
                    {
                        UserId = intent.SeekerId,
                        Title = title,
                        Message = message,
                        SentAt = DateTime.UtcNow
                    };
                    _db.NotificationLogs.Add(notificationLog);
                    await _db.SaveChangesAsync();

                    var data = new Dictionary<string, string>
                    {
                        { "notificationId", notificationLog.NotificationId.ToString() },
                        { "sentAt", DateTime.UtcNow.ToString("o") },
                    };
                    var deviceToken = intent.User!.DeviceTokens.LastOrDefault();
                    if (deviceToken != null)
                    {
                        await _notificationService.SendNotificationAsync(
                            deviceToken.DeviceToken,
                            title,
                            message,
                            data
                        );
                    }
                }

                _db.PickupIntents.RemoveRange(pickupIntents);
                await _db.SaveChangesAsync();

                return true;
            }

            return true;
        }
        catch (Exception e)
        {
            // Log the exception (logging mechanism not shown here)
            Console.WriteLine($"Error removing pickup intents for food ID {foodId}: {e.Message}");
            return false;
        }
    }
}