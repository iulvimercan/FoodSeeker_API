using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO;
using FoodSeekerAPI.DTO.Common;
using FoodSeekerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController(FoodSeekerContext db, NotificationService notificationService) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;
    private readonly NotificationService _notificationService = notificationService;

    /// <summary>
    /// Retrieves a list of all notifications for the currently authenticated user.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<NotificationLogDto>>> GetNotifications()
    {
        try
        {
            // Get user ID from JWT claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized("User not authenticated.");

            // Fetch and return notifications sorted by newest first
            var notifications = await _db.NotificationLogs
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt)
                .Select(n => new NotificationLogDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title ?? string.Empty,
                    Message = n.Message ?? string.Empty,
                    SentAt = n.SentAt,
                    IsRead = n.IsRead
                }).ToListAsync();

            return Ok(notifications);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching notifications: {e.Message}");
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
    }

    /// <summary>
    /// Marks all unread notifications of the authenticated user as read.
    /// </summary>
    [HttpPatch("mark-all-read")]
    [Authorize]
    public async Task<ActionResult<List<NotificationLogDto>>> MarkAllAsRead()
    {
        // Get user ID from claims
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
            return Unauthorized("User not authenticated.");

        // Fetch only unread notifications
        var notifications = await _db.NotificationLogs
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        // Mark each as read
        foreach (var n in notifications)
        {
            n.IsRead = true;
        }

        await _db.SaveChangesAsync();

        // Return updated list
        var updatedNotifications = await _db.NotificationLogs
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.SentAt)
            .Select(n => new NotificationLogDto
            {
                NotificationId = n.NotificationId,
                Title = n.Title ?? string.Empty,
                Message = n.Message ?? string.Empty,
                SentAt = n.SentAt,
                IsRead = n.IsRead
            }).ToListAsync();

        return Ok(updatedNotifications);
    }

    /// <summary>
    /// Deletes a single notification that belongs to the authenticated user.
    /// </summary>
    [HttpDelete("{notificationId:long}")]
    [Authorize]
    public async Task<IActionResult> DeleteNotification(long notificationId)
    {
        try
        {
            // Get user ID from claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized("User not authenticated.");

            // Locate the notification
            var notification = await _db.NotificationLogs
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
                return NotFound("Notification not found.");

            // Delete from DB
            _db.NotificationLogs.Remove(notification);
            await _db.SaveChangesAsync();

            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting notification: {e.Message}");
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
    }

    /// <summary>
    /// Deletes all notifications for the authenticated user.
    /// </summary>
    [HttpDelete("delete-all")]
    [Authorize]
    public async Task<IActionResult> DeleteAllNotifications()
    {
        try
        {
            // Get user ID from claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized("User not authenticated.");

            // Get all notifications for user
            var notifications = await _db.NotificationLogs
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (!notifications.Any())
                return NotFound("No notifications found for the user.");

            // Delete all notifications
            _db.NotificationLogs.RemoveRange(notifications);
            await _db.SaveChangesAsync();

            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting notifications: {e.Message}");
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
    }

    /// <summary>
    /// (Test Only) Sends a push notification to the device of the specified user.
    /// This endpoint is for development purposes and should be removed in production.
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequestDto dto)
    {
        try
        {
            // Retrieve device token from database for the target user
            var deviceToken = await _db.UserDeviceTokens
                .Where(t => t.UserId == dto.UserId)
                .Select(t => t.DeviceToken)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(deviceToken))
            {
                return NotFound("Device token not found for the specified user.");
            }

            // Send the notification via Firebase
            var response = await _notificationService.SendNotificationAsync(deviceToken, dto.Title, dto.Body, dto.Data);

            return Ok(new { MessageId = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error sending notification: {ex.Message}");
        }
    }
}
