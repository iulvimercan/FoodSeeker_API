using FoodSeekerAPI.Data;
using FoodSeekerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Services;

public class PickupIntentService(FoodSeekerContext db, NotificationService notificationService)
{
    private readonly FoodSeekerContext _db = db;
    private readonly NotificationService _notificationService = notificationService;

    /// <summary>
    /// Deletes pickup intents older than 6 hours to clean up stale entries.
    /// </summary>
    /// <returns>True if cleanup succeeded, false if an error occurred.</returns>
    public async Task<bool> CleanOldPickupIntentsAsync()
    {
        try
        {
            // Define cutoff time for old intents (6 hours ago)
            var threshold = DateTime.UtcNow.AddHours(-6);

            // Retrieve pickup intents created before the threshold
            var oldIntents = _db.PickupIntents
                .Where(intent => intent.CreatedAt < threshold)
                .ToList();

            if (oldIntents.Count > 0)
            {
                Console.WriteLine(
                    $"Removing {oldIntents.Count} old pickup intents older than 6 hours or inactive food items.");
                
                // Remove old pickup intents from database
                _db.PickupIntents.RemoveRange(oldIntents);
                await _db.SaveChangesAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            // Log error and return failure
            Console.WriteLine($"Error cleaning old pickup intents: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Removes all pickup intents related to a given food item and notifies the respective users.
    /// </summary>
    /// <param name="foodId">The ID of the food item whose pickup intents should be removed.</param>
    /// <returns>True if operation succeeded, false if an error occurred.</returns>
    public async Task<bool> RemovePickupIntentsByFoodIdAndNotifyAsync(long foodId)
    {
        try
        {
            // Clean old intents first to keep database tidy
            await CleanOldPickupIntentsAsync();

            // Load all pickup intents for the specified foodId including related user and device tokens
            var pickupIntents = await _db.PickupIntents
                .Where(pi => pi.FoodId == foodId)
                .Include(pi => pi.User)
                .ThenInclude(u => u.DeviceTokens)
                .Include(pi => pi.FoodItem)
                .ToListAsync();

            if (pickupIntents.Count > 0)
            {
                Console.WriteLine($"Removing {pickupIntents.Count} pickup intents for food ID {foodId}.");

                // Notify each user that the food item has been removed and their intent cancelled
                foreach (var intent in pickupIntents)
                {
                    var title = "Food Item Removed";
                    var message =
                        $"The food item '{intent.FoodItem!.FoodName}' has been removed. Your pickup intent has been cancelled.";

                    // Add a notification log entry for audit/history
                    var notificationLog = new NotificationLog
                    {
                        UserId = intent.SeekerId,
                        Title = title,
                        Message = message,
                        SentAt = DateTime.UtcNow
                    };
                    _db.NotificationLogs.Add(notificationLog);
                    await _db.SaveChangesAsync();

                    // Prepare data payload for push notification
                    var data = new Dictionary<string, string>
                    {
                        { "notificationId", notificationLog.NotificationId.ToString() },
                        { "sentAt", DateTime.UtcNow.ToString("o") },
                        { "screen", "pickup_intents" }
                    };

                    // Send push notification to the user's last registered device token
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

                // Remove all the pickup intents for the food item from database
                _db.PickupIntents.RemoveRange(pickupIntents);
                await _db.SaveChangesAsync();

                return true;
            }

            return true; // No intents found to remove is not an error
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error removing pickup intents for food ID {foodId}: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Notifies all users who expressed pickup intent that a food item has been updated.
    /// </summary>
    /// <param name="foodItem">The updated food item entity.</param>
    /// <returns>True if notifications succeeded, false otherwise.</returns>
    public async Task<bool> NotifyFoodItemIsUpdatedAsync(FoodItem foodItem)
    {
        try
        {
            // Clean old pickup intents first
            await CleanOldPickupIntentsAsync();

            // Retrieve pickup intents for this food item with user and device tokens loaded
            var pickupIntents = await _db.PickupIntents
                .Where(pi => pi.FoodId == foodItem.FoodId)
                .Include(pi => pi.User)
                .ThenInclude(u => u.DeviceTokens)
                .ToListAsync();

            if (pickupIntents.Count > 0)
            {
                // Send notification to each user about the update
                foreach (var intent in pickupIntents)
                {
                    var title = "Food Item Updated";
                    var message = $"The food item '{foodItem.FoodName}' has been updated.";

                    // Log notification in database
                    var notificationLog = new NotificationLog
                    {
                        UserId = intent.SeekerId,
                        Title = title,
                        Message = message,
                        SentAt = DateTime.UtcNow
                    };
                    _db.NotificationLogs.Add(notificationLog);
                    await _db.SaveChangesAsync();

                    // Prepare additional data payload for the notification
                    var data = new Dictionary<string, string>
                    {
                        { "notificationId", notificationLog.NotificationId.ToString() },
                        { "sentAt", DateTime.UtcNow.ToString("o") },
                        { "screen", "pickup_intents" }
                    };

                    // Send push notification to user's last device token if exists
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
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error notifying food item update: {e.Message}");
            return false;
        }
    }
}
