using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO;
using FoodSeekerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController(FoodSeekerContext db, NotificationService notificationService) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;
    private readonly NotificationService _notificationService = notificationService;

    // todo - for testing purposes only, remove in production
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequestDto dto)
    {
        try
        {
            var deviceToken = await _db.UserDeviceTokens
                .Where(t => t.UserId == dto.UserId)
                .Select(t => t.DeviceToken)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(deviceToken))
            {
                return NotFound("Device token not found for the specified user.");
            }

            var response = await _notificationService.SendNotificationAsync(deviceToken, dto.Title, dto.Body, dto.Data);
            return Ok(new { MessageId = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error sending notification: {ex.Message}");
        }
    }
}