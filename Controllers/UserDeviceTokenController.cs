using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO.UserDeviceToken;
using FoodSeekerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

[Route("api/user-device-token")]
[ApiController]
public class UserDeviceTokenController(FoodSeekerContext db) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SaveUserDeviceToken([FromBody] SaveUserDeviceTokenRequestDto dto)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("User not authenticated.");
            }

            if (string.IsNullOrWhiteSpace(dto.DeviceToken))
            {
                return BadRequest("Invalid device token.");
            }

            if (dto.Platform != "Android" && dto.Platform != "iOS")
            {
                return BadRequest("Platform must be either 'Android' or 'iOS'.");
            }

            var existingToken = await _db.UserDeviceTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.DeviceToken == dto.DeviceToken);

            if (existingToken == null)
            {
                var newToken = new UserDeviceToken
                {
                    UserId = userId,
                    DeviceToken = dto.DeviceToken,
                    Platform = dto.Platform,
                    CreatedAt = DateTime.UtcNow,
                };
                await _db.UserDeviceTokens.AddAsync(newToken);
            }

            await _db.SaveChangesAsync();

            return Ok("Device token saved successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving device token: {e.Message}");
            return StatusCode(500, "An error occurred while saving the device token.");
        }
    }
}