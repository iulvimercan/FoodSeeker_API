using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO.Common;
using FoodSeekerAPI.DTO.FoodItem;
using FoodSeekerAPI.Models;
using FoodSeekerAPI.Services;
using FoodSeekerAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

[Route("api/food")]
[ApiController]
public class FoodItemController(FoodSeekerContext db, PickupIntentService pickupIntentService) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;
    private readonly PickupIntentService _pickupIntentService = pickupIntentService;

    /// <summary>
    /// Retrieves a paginated list of nearby active food items filtered by optional parameters.
    /// </summary>
    [HttpGet("nearby")]
    public async Task<ActionResult<IEnumerable<FoodItemDto>>> GetFoodItemsByLocation(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] string? keyword,
        [FromQuery] double distanceKm = 30,
        [FromQuery] bool eatIn = true,
        [FromQuery] bool takeAway = true,
        [FromQuery] bool bringPack = true,
        [FromQuery] int pageSize = 30,
        [FromQuery] int page = 1
    )
    {
        try
        {
            // Convert distance in km to degrees for bounding box search
            double degLat = distanceKm / 111.32;
            double degLon = distanceKm / (111.32 * Math.Cos(latitude * Math.PI / 180));

            // Fetch filtered food items within the bounding box and matching keyword/type filters
            var foodItemsWithProfiles = await _db.FoodItems
                .Where(fi => fi.IsActive &&
                             (
                                 (!eatIn && !takeAway && !bringPack) || // No filter case
                                 (eatIn && fi.IsEatIn) ||
                                 (takeAway && fi.IsTakeAway) ||
                                 (bringPack && fi.IsBringPack)
                             ) &&
                             (string.IsNullOrEmpty(keyword) || fi.FoodName.Contains(keyword) ||
                              (!string.IsNullOrEmpty(fi.Description) && fi.Description.Contains(keyword))) &&
                             fi.DonatorProfile != null &&
                             fi.DonatorProfile.Latitude >= latitude - degLat &&
                             fi.DonatorProfile.Latitude <= latitude + degLat &&
                             fi.DonatorProfile.Longitude >= longitude - degLon &&
                             fi.DonatorProfile.Longitude <= longitude + degLon)
                .Include(fi => fi.DonatorProfile)
                .Include(fi => fi.DonatorProfile!.User)
                .ToListAsync();

            // Project into DTOs with calculated distances and pagination
            var sorted = foodItemsWithProfiles
                .Select(fi =>
                    new FoodItemDto
                    {
                        FoodId = fi.FoodId,
                        FoodName = fi.FoodName,
                        Description = fi.Description,
                        IsEatIn = fi.IsEatIn,
                        IsTakeAway = fi.IsTakeAway,
                        IsBringPack = fi.IsBringPack,
                        PhotoUrl = fi.PhotoUrl,
                        IsActive = fi.IsActive,
                        CreatedAt = fi.CreatedAt,
                        DonatorProfile = new DonatorProfileDto
                        {
                            DonatorId = fi.DonatorProfile!.DonatorId,
                            RestaurantName = fi.DonatorProfile.RestaurantName,
                            ProfilePhotoUrl = fi.DonatorProfile.User?.ProfilePhotoUrl,
                            Address = fi.DonatorProfile.Address,
                            AddressStreet = fi.DonatorProfile.AddressStreet,
                            AddressMunicipality = fi.DonatorProfile.AddressMunicipality,
                            AddressCity = fi.DonatorProfile.AddressCity,
                            AddressCountry = fi.DonatorProfile.AddressCountry,
                            Latitude = fi.DonatorProfile.Latitude,
                            Longitude = fi.DonatorProfile.Longitude,
                            Distance = GeoUtils.CalculateDistance(
                                latitude,
                                longitude,
                                fi.DonatorProfile.Latitude,
                                fi.DonatorProfile.Longitude),
                            DonationStarts = fi.DonatorProfile.DonationStarts.ToString("HH:mm"),
                            DonationEnds = fi.DonatorProfile.DonationEnds.ToString("HH:mm"),
                            AverageScore = (double)fi.DonatorProfile.AverageScore,
                            FavoritesCount = fi.DonatorProfile.FavoritesCount
                        }
                    })
                .OrderBy(fi => fi.DonatorProfile!.Distance) // Sort by closest distance
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(sorted);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while retrieving food items: " + e.Message);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Allows a donator to create a new food item.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Donator")]
    public async Task<IActionResult> CreateFoodItem([FromBody] CreateFoodItemRequestDto dto)
    {
        try
        {
            var donatorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (donatorIdStr == null || !long.TryParse(donatorIdStr, out var donatorId))
                return Unauthorized();

            var foodItem = new FoodItem
            {
                DonatorId = donatorId,
                FoodName = dto.Name,
                Description = dto.Description,
                IsEatIn = dto.IsEatIn,
                IsTakeAway = dto.IsTakeAway,
                IsBringPack = dto.IsBringPack,
                PhotoUrl = dto.PhotoUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _db.FoodItems.AddAsync(foodItem);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Food item created successfully!", foodItem });
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while creating food item: " + e.Message);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Retrieves all food items posted by a specific donator.
    /// </summary>
    [HttpGet("by-donator/{donatorId}")]
    public async Task<ActionResult<IEnumerable<FoodItem>>> GetFoodItemsByDonatorId([FromRoute] long donatorId)
    {
        try
        {
            if (donatorId <= 0)
                return BadRequest("Invalid donator ID.");

            var foodItems = await _db.FoodItems
                .Where(fi => fi.DonatorId == donatorId)
                .ToListAsync();

            if (foodItems.Count == 0)
                return NotFound("No food items found for this donator.");

            return Ok(foodItems);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while retrieving food items: " + e.Message);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Allows a donator to update one of their existing food items.
    /// </summary>
    [HttpPut("{foodItemId}")]
    [Authorize(Roles = "Donator")]
    public async Task<ActionResult<FoodItem>> UpdateFoodItem([FromRoute] long foodItemId,
        [FromBody] UpdateFoodItemRequestDto dto)
    {
        try
        {
            if (foodItemId <= 0)
                return BadRequest("Invalid food item ID.");

            var donatorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (donatorIdStr == null || !long.TryParse(donatorIdStr, out var donatorId))
                return Unauthorized();

            var foodItem = await _db.FoodItems
                .Include(fi => fi.DonatorProfile)
                    .ThenInclude(d => d!.User)
                .FirstOrDefaultAsync(fi => fi.FoodId == foodItemId && fi.DonatorId == donatorId);

            if (foodItem == null)
                return NotFound("Food item not found.");

            // Update properties
            foodItem.FoodName = dto.Name;
            foodItem.Description = dto.Description;
            foodItem.IsEatIn = dto.IsEatIn;
            foodItem.IsTakeAway = dto.IsTakeAway;
            foodItem.IsBringPack = dto.IsBringPack;
            foodItem.PhotoUrl = dto.PhotoUrl;

            _db.FoodItems.Update(foodItem);
            await _db.SaveChangesAsync();

            // Notify seekers who showed intent for this food
            await _pickupIntentService.NotifyFoodItemIsUpdatedAsync(foodItem);

            // Return updated item in DTO format
            var foodItemDto = new FoodItemDto
            {
                FoodId = foodItem.FoodId,
                FoodName = foodItem.FoodName,
                Description = foodItem.Description,
                IsEatIn = foodItem.IsEatIn,
                IsTakeAway = foodItem.IsTakeAway,
                IsBringPack = foodItem.IsBringPack,
                PhotoUrl = foodItem.PhotoUrl,
                IsActive = foodItem.IsActive,
                CreatedAt = foodItem.CreatedAt,
                DonatorProfile = new DonatorProfileDto
                {
                    DonatorId = foodItem.DonatorProfile!.DonatorId,
                    RestaurantName = foodItem.DonatorProfile.RestaurantName,
                    ProfilePhotoUrl = foodItem.DonatorProfile.User?.ProfilePhotoUrl,
                    Address = foodItem.DonatorProfile.Address,
                    AddressStreet = foodItem.DonatorProfile.AddressStreet,
                    AddressMunicipality = foodItem.DonatorProfile.AddressMunicipality,
                    AddressCity = foodItem.DonatorProfile.AddressCity,
                    AddressCountry = foodItem.DonatorProfile.AddressCountry,
                    Latitude = foodItem.DonatorProfile.Latitude,
                    Longitude = foodItem.DonatorProfile.Longitude,
                    DonationStarts = foodItem.DonatorProfile.DonationStarts.ToString("HH:mm"),
                    DonationEnds = foodItem.DonatorProfile.DonationEnds.ToString("HH:mm"),
                    AverageScore = (double)foodItem.DonatorProfile.AverageScore,
                    FavoritesCount = foodItem.DonatorProfile.FavoritesCount
                }
            };

            return Ok(new { message = "Food item is updated successfully!", foodItem = foodItemDto });
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while updating food item: " + e.Message);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Toggles the activeness status (available/unavailable) of a food item.
    /// </summary>
    [HttpPatch("{foodItemId}/toggle-activeness")]
    [Authorize(Roles = "Donator")]
    public async Task<ActionResult<FoodItem>> ToggleFoodItemActiveness([FromRoute] long foodItemId)
    {
        try
        {
            if (foodItemId <= 0)
                return BadRequest("Invalid food item ID.");

            var donatorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (donatorIdStr == null || !long.TryParse(donatorIdStr, out var donatorId))
                return Unauthorized();

            var foodItem = await _db.FoodItems
                .FirstOrDefaultAsync(fi => fi.FoodId == foodItemId && fi.DonatorId == donatorId);

            if (foodItem == null)
                return NotFound("Food item not found.");

            foodItem.IsActive = !foodItem.IsActive;
            _db.FoodItems.Update(foodItem);
            await _db.SaveChangesAsync();

            // Notify seekers that food is no longer available
            await _pickupIntentService.RemovePickupIntentsByFoodIdAndNotifyAsync(foodItemId);

            return Ok(foodItem);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while switching food item availability: " + e.Message);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Deletes a food item created by the donator.
    /// </summary>
    [HttpDelete("{foodItemId}")]
    [Authorize(Roles = "Donator")]
    public async Task<IActionResult> DeleteFoodItem([FromRoute] long foodItemId)
    {
        try
        {
            if (foodItemId <= 0)
                return BadRequest("Invalid food item ID.");

            var donatorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (donatorIdStr == null || !long.TryParse(donatorIdStr, out var donatorId))
                return Unauthorized();

            var foodItem = await _db.FoodItems
                .FirstOrDefaultAsync(fi => fi.FoodId == foodItemId && fi.DonatorId == donatorId);

            if (foodItem == null)
                return NotFound("Food item not found.");

            // Clean up associated pickup intents
            await _pickupIntentService.RemovePickupIntentsByFoodIdAndNotifyAsync(foodItemId);

            _db.FoodItems.Remove(foodItem);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Food item deleted successfully." });
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while deleting food item: " + e.Message);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
