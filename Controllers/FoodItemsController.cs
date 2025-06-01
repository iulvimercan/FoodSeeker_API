using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO.FoodItem;
using FoodSeekerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

[Route("api/food")]
[ApiController]
public class FoodItemsController(FoodSeekerContext db) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;

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


    // put method to update an existing food item
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