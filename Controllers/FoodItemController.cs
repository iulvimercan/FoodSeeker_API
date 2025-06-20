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

    [HttpGet("nearby")]
    public async Task<ActionResult<IEnumerable<FoodItemDto>>> GetFoodItemsByLocation(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double distanceKm = 30,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 3)
    {
        try
        {
            double degLat = distanceKm / 111.32;
            double degLon = distanceKm / (111.32 * Math.Cos(latitude * Math.PI / 180));

            var foodItemsWithProfiles = await _db.FoodItems
                .Where(fi => fi.IsActive &&
                             fi.DonatorProfile != null &&
                             fi.DonatorProfile.Latitude >= latitude - degLat &&
                             fi.DonatorProfile.Latitude <= latitude + degLat &&
                             fi.DonatorProfile.Longitude >= longitude - degLon &&
                             fi.DonatorProfile.Longitude <= longitude + degLon)
                .Include(fi => fi.DonatorProfile)
                .Include(fi => fi.DonatorProfile!.User)
                .ToListAsync();

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
                .OrderBy(fi => fi.DonatorProfile!.Distance)
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

            // Add food item to the database
            await _db.FoodItems.AddAsync(foodItem);
            await _db.SaveChangesAsync();

            var message = "Food item created successfully!";
            return Ok(new { message, foodItem });
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while creating food item: " + e.Message);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("by-donator/{donatorId}")]
    public async Task<ActionResult<IEnumerable<FoodItem>>> GetFoodItemsByDonatorId([FromRoute] long donatorId)
    {
        try
        {
            // Validate donatorId
            if (donatorId <= 0)
                return BadRequest("Invalid donator ID.");

            // Query food items for the donator
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

    [HttpPut("{foodItemId}")]
    [Authorize(Roles = "Donator")]
    public async Task<ActionResult<FoodItem>> UpdateFoodItem([FromRoute] long foodItemId,
        [FromBody] UpdateFoodItemRequestDto dto)
    {
        try
        {
            // Validate foodItemId
            if (foodItemId <= 0)
                return BadRequest("Invalid food item ID.");

            // Get the donator ID from the claims
            var donatorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (donatorIdStr == null || !long.TryParse(donatorIdStr, out var donatorId))
                return Unauthorized();

            // Check if the food item belongs to the donator
            var foodItem = await _db.FoodItems
                .FirstOrDefaultAsync(fi => fi.FoodId == foodItemId && fi.DonatorId == donatorId);

            if (foodItem == null)
                return NotFound("Food item not found.");

            // Update food item properties
            foodItem.FoodName = dto.Name;
            foodItem.Description = dto.Description;
            foodItem.IsEatIn = dto.IsEatIn;
            foodItem.IsTakeAway = dto.IsTakeAway;
            foodItem.IsBringPack = dto.IsBringPack;
            foodItem.PhotoUrl = dto.PhotoUrl;

            _db.FoodItems.Update(foodItem);
            await _db.SaveChangesAsync();

            var message = "Food item is updated successfully!";
            return Ok(new { message, foodItem });
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while updating food item: " + e.Message);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPatch("{foodItemId}/toggle-activeness")]
    [Authorize(Roles = "Donator")]
    public async Task<ActionResult<FoodItem>> ToggleFoodItemActiveness([FromRoute] long foodItemId)
    {
        try
        {
            // Validate foodItemId
            if (foodItemId <= 0)
                return BadRequest("Invalid food item ID.");

            // Get the donator ID from the claims
            var donatorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (donatorIdStr == null || !long.TryParse(donatorIdStr, out var donatorId))
                return Unauthorized();

            // Check if the food item belongs to the donator
            var foodItem = await _db.FoodItems
                .FirstOrDefaultAsync(fi => fi.FoodId == foodItemId && fi.DonatorId == donatorId);

            if (foodItem == null)
                return NotFound("Food item not found.");

            // Toggle activeness
            foodItem.IsActive = !foodItem.IsActive;
            _db.FoodItems.Update(foodItem);
            await _db.SaveChangesAsync();

            // notify the food seekers with pickup intent for this food item
            await _pickupIntentService.RemovePickupIntentsByFoodIdAndNotifyAsync(foodItemId);

            return Ok(foodItem);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred while switching food item availability: " + e.Message);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpDelete("{foodItemId}")]
    [Authorize(Roles = "Donator")]
    public async Task<IActionResult> DeleteFoodItem([FromRoute] long foodItemId)
    {
        try
        {
            // Validate foodItemId
            if (foodItemId <= 0)
                return BadRequest("Invalid food item ID.");

            // Get the donator ID from the claims
            var donatorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (donatorIdStr == null || !long.TryParse(donatorIdStr, out var donatorId))
                return Unauthorized();

            // Check if the food item belongs to the donator
            var foodItem = await _db.FoodItems
                .FirstOrDefaultAsync(fi => fi.FoodId == foodItemId && fi.DonatorId == donatorId);

            if (foodItem == null)
                return NotFound("Food item not found.");
            
            // Remove pickup intents associated with this food item
            await _pickupIntentService.RemovePickupIntentsByFoodIdAndNotifyAsync(foodItemId);

            // Remove food item from the database
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