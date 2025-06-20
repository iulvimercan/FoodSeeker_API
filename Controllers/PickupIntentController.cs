using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO.Common;
using FoodSeekerAPI.DTO.User;
using FoodSeekerAPI.Models;
using FoodSeekerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

[Route("api/pickup-intent")]
[ApiController]
public class PickupIntentController(FoodSeekerContext db, NotificationService notificationService, PickupIntentService pickupIntentService) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;
    private readonly NotificationService _notificationService = notificationService;
    private readonly PickupIntentService _pickupIntentService = pickupIntentService;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetPickupIntents()
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();
            var isDonator = User.FindFirstValue(ClaimTypes.Role) == "Donator";

            // Clean old pickup intents
            await _pickupIntentService.CleanOldPickupIntentsAsync();
            
            // fetch the pickup intents for the user
            // can be both donator or seeker
            var pickupIntents = await _db.PickupIntents
                .Include(pi => pi.FoodItem)
                .ThenInclude(fi => fi!.DonatorProfile)
                .ThenInclude(d => d!.User)
                .Include(pi => pi.User)
                .Where(pi => pi.FoodItem!.DonatorId == userId || pi.SeekerId == userId)
                .ToListAsync();

            var desiredFoodItemIds = pickupIntents
                .Select(pi => pi.FoodItem!.FoodId)
                .Distinct()
                .ToList();
            List<PickupIntentDto> pickupIntentDtos;
            if (isDonator)
            {
                pickupIntentDtos = pickupIntents
                    .OrderByDescending(pi => pi.FoodId)
                    .ThenByDescending(pi => pi.CreatedAt)
                    .Select(pi => new PickupIntentDto
                    {
                        IntentId = pi.IntentId,
                        FoodItem = new FoodItemDto
                        {
                            FoodId = pi.FoodItem!.FoodId,
                            FoodName = pi.FoodItem.FoodName,
                            Description = pi.FoodItem.Description,
                            IsEatIn = pi.FoodItem.IsEatIn,
                            IsTakeAway = pi.FoodItem.IsTakeAway,
                            IsBringPack = pi.FoodItem.IsBringPack,
                            PhotoUrl = pi.FoodItem.PhotoUrl,
                            CreatedAt = pi.FoodItem.CreatedAt,
                            IsActive = pi.FoodItem.IsActive,
                            DonatorProfile = new DonatorProfileDto
                            {
                                DonatorId = pi.FoodItem.DonatorProfile!.DonatorId,
                                RestaurantName = pi.FoodItem.DonatorProfile.RestaurantName,
                                ProfilePhotoUrl = pi.FoodItem.DonatorProfile.User!.ProfilePhotoUrl,
                                Address = pi.FoodItem.DonatorProfile.Address,
                                AddressStreet = pi.FoodItem.DonatorProfile.AddressStreet,
                                AddressMunicipality = pi.FoodItem.DonatorProfile.AddressMunicipality,
                                AddressCity = pi.FoodItem.DonatorProfile.AddressCity,
                                AddressCountry = pi.FoodItem.DonatorProfile.AddressCountry,
                                Latitude = pi.FoodItem.DonatorProfile.Latitude,
                                Longitude = pi.FoodItem.DonatorProfile.Longitude,
                                DonationStarts = pi.FoodItem.DonatorProfile.DonationStarts.ToString("HH:mm"),
                                DonationEnds = pi.FoodItem.DonatorProfile.DonationEnds.ToString("HH:mm"),
                                AverageScore = pi.FoodItem.DonatorProfile.AverageScore,
                                FavoritesCount = pi.FoodItem.DonatorProfile.FavoritesCount,
                            }
                        },
                        CreatedAt = pi.CreatedAt,
                        FoodSeeker = new UserProfileDto
                        {
                            UserId = pi.User!.UserId,
                            FullName = pi.User.FullName,
                            Email = pi.User.Email,
                            IsDonator = false, // Seeker is not a donator
                            ProfilePhotoUrl = pi.User.ProfilePhotoUrl
                        }
                    }).ToList();
            }
            else
            {
                pickupIntentDtos = pickupIntents
                    .OrderByDescending(pi => pi.CreatedAt)
                    .Select(pi => new PickupIntentDto
                    {
                        IntentId = pi.IntentId,
                        FoodItem = new FoodItemDto
                        {
                            FoodId = pi.FoodItem!.FoodId,
                            FoodName = pi.FoodItem.FoodName,
                            Description = pi.FoodItem.Description,
                            IsEatIn = pi.FoodItem.IsEatIn,
                            IsTakeAway = pi.FoodItem.IsTakeAway,
                            IsBringPack = pi.FoodItem.IsBringPack,
                            PhotoUrl = pi.FoodItem.PhotoUrl,
                            CreatedAt = pi.FoodItem.CreatedAt,
                            IsActive = pi.FoodItem.IsActive,
                            DonatorProfile = new DonatorProfileDto
                            {
                                DonatorId = pi.FoodItem.DonatorProfile!.DonatorId,
                                RestaurantName = pi.FoodItem.DonatorProfile.RestaurantName,
                                ProfilePhotoUrl = pi.FoodItem.DonatorProfile.User!.ProfilePhotoUrl,
                                Address = pi.FoodItem.DonatorProfile.Address,
                                AddressStreet = pi.FoodItem.DonatorProfile.AddressStreet,
                                AddressMunicipality = pi.FoodItem.DonatorProfile.AddressMunicipality,
                                AddressCity = pi.FoodItem.DonatorProfile.AddressCity,
                                AddressCountry = pi.FoodItem.DonatorProfile.AddressCountry,
                                Latitude = pi.FoodItem.DonatorProfile.Latitude,
                                Longitude = pi.FoodItem.DonatorProfile.Longitude,
                                DonationStarts = pi.FoodItem.DonatorProfile.DonationStarts.ToString("HH:mm"),
                                DonationEnds = pi.FoodItem.DonatorProfile.DonationEnds.ToString("HH:mm"),
                                AverageScore = pi.FoodItem.DonatorProfile.AverageScore,
                                FavoritesCount = pi.FoodItem.DonatorProfile.FavoritesCount,
                            }
                        },
                        CreatedAt = pi.CreatedAt,
                    }).ToList();
            }

            return Ok(new { foodIds = desiredFoodItemIds, pickupIntents = pickupIntentDtos });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching pickup intents: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

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

            if (isAlreadyRequested == true)
                return BadRequest("You have already requested pickup for this food item.");

            var donatorDeviceToken = await _db.UserDeviceTokens
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
                { "sentAt", DateTime.UtcNow.ToString("o") },
                { "screen", "pickup_intents" }
            };
            await _notificationService.SendNotificationAsync(donatorDeviceToken, titleDonator, bodyDonator,
                dataDonator);


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
                { "sentAt", DateTime.UtcNow.ToString("o") },
                { "screen", "pickup_intents" }
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

    [HttpDelete("{intentId:long}")]
    [Authorize]
    public async Task<IActionResult> DeletePickupIntent([FromRoute] long intentId)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var pickupIntent = await _db.PickupIntents
                .Include(pi => pi.FoodItem)
                .SingleOrDefaultAsync(pi =>
                    pi.IntentId == intentId && (pi.SeekerId == userId || pi.FoodItem!.DonatorId == userId));

            if (pickupIntent == null)
                return NotFound("Pickup intent not found.");

            _db.PickupIntents.Remove(pickupIntent);
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Pickup intent deleted successfully." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting pickup intent: {ex.Message}");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}