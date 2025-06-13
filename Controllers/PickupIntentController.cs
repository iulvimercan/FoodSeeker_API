using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.Models;
using FoodSeekerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

[Route("api/pickup-intent")]
[ApiController]
public class PickupIntentController(FoodSeekerContext db, NotificationService notificationService) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;
    private readonly NotificationService _notificationService = notificationService;

    // todo - for testing purposes only, remove in production
    [HttpPost("{foodItemId}/send-notification")]
    [Authorize(Roles = "FoodSeeker")]
    public async Task<IActionResult> CreatePickupIntent([FromRoute] long foodItemId)
    {
        try
        {
            var foodSeekerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (foodSeekerIdStr == null || !long.TryParse(foodSeekerIdStr, out var foodSeekerId))
                return Unauthorized();

            if (foodItemId <= 0)
                return BadRequest("Invalid food item ID.");

            var foodItem = await _db.FoodItems
                .Include(fi => fi.PickupIntents)
                .SingleOrDefaultAsync(fi => fi.FoodId == foodItemId);

            if (foodItem == null)
                return NotFound("Food item not found.");

            if (!foodItem.IsActive)
                return BadRequest("Food item is not active.");

            var isAlreadyRequested = foodItem.PickupIntents?
                .Any(pi => pi.SeekerId == foodSeekerId);

            // if (isAlreadyRequested == true)
                // return BadRequest("You have already requested pickup for this food item.");

            var donatorDeviceToken= await _db.UserDeviceTokens
                .Where(t => t.UserId == foodItem.DonatorId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => t.DeviceToken)
                .FirstOrDefaultAsync();

            if (donatorDeviceToken == null)
                return NotFound("Device token not found for the specified donator.");

            // Create the pickup intent
            var pickupIntent = new PickupIntent
            {
                FoodId = foodItemId,
                SeekerId = foodSeekerId,
                CreatedAt = DateTime.UtcNow
            };

            _db.PickupIntents.Add(pickupIntent);
            await _db.SaveChangesAsync();

            // Send notification to the donator
            var titleDonator = "Pickup Intent Created";
            var bodyDonator = $"A pickup intent has been created for the food item '{foodItem.FoodName}'.";
            var donatorNotificationLog = new NotificationLog
            {
                UserId = foodItem.DonatorId,
                Title = titleDonator,
                Message = bodyDonator,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            _db.NotificationLogs.Add(donatorNotificationLog);
            await _db.SaveChangesAsync();
            
            var dataDonator = new Dictionary<string, string>
            {
                { "notificationId", donatorNotificationLog.NotificationId.ToString() },
                { "sentAt", DateTime.UtcNow.ToString("o") }
            };
            await _notificationService.SendNotificationAsync(donatorDeviceToken, titleDonator, bodyDonator, dataDonator);
            

            
            var titleSeeker = "Pickup Intent Created";
            var bodySeeker = $"You have successfully created a pickup intent for '{foodItem.FoodName}'.";
            // Log notification for the food seeker
            var userNotificationLog = new NotificationLog
            {
                UserId = foodSeekerId,
                Title = titleSeeker,
                Message = bodySeeker,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            _db.NotificationLogs.Add(userNotificationLog);
            await _db.SaveChangesAsync();
            var dataSeeker = new Dictionary<string, string>
            {
                { "notificationId", userNotificationLog.NotificationId.ToString() },
                { "sentAt", DateTime.UtcNow.ToString("o") }
            };
            // Send notification to the food seeker
            var userDeviceToken = await _db.UserDeviceTokens
                .Where(t => t.UserId == foodSeekerId)
                .Select(t => t.DeviceToken)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(userDeviceToken))
                await _notificationService.SendNotificationAsync(userDeviceToken, titleSeeker, bodySeeker, dataSeeker);
            
            return Ok(new { Message = "Pickup intent created and notification sent." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating pickup intent: {ex.Message}");
            return StatusCode(500, $"Error sending notification: {ex.Message}");
        }
    }
}