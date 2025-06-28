using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO.Common;
using FoodSeekerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

[Route("api/favorite-donator")]
[ApiController]
[Authorize(Roles = "FoodSeeker")]
public class FavoriteDonatorController(FoodSeekerContext db) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;

    [HttpPost("{donatorId:long}")]
    public async Task<ActionResult<FavoriteDonatorDto>> AddFavoriteDonator(long donatorId)
    {
        try
        {
            // Get user ID from JWT claims
            var foodSeekerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (foodSeekerIdStr == null || !long.TryParse(foodSeekerIdStr, out var foodSeekerId))
                return Unauthorized();

            // Check if the donator exists
            var donator = await _db.DonatorProfiles
                .Include(d => d.User)
                .SingleOrDefaultAsync(d => donatorId == d.DonatorId);
            if (donator == null)
                return NotFound("Donator not found");

            // Check if the user already favorited this donator
            var existingFavorite = await _db.FavoriteDonators
                .FirstOrDefaultAsync(fd => fd.SeekerId == foodSeekerId && fd.DonatorId == donatorId);
            if (existingFavorite != null)
                return Conflict("This donator is already in your favorites");

            // Create new favorite entry
            var favorite = new FavoriteDonator
            {
                SeekerId = foodSeekerId,
                DonatorId = donatorId,
                FavoritedAt = DateTime.UtcNow
            };
            // Add the favorite to the database
            _db.FavoriteDonators.Add(favorite);
            // Increment the favorites count for the donator
            donator.FavoritesCount++;
            // Save changes to the database
            await _db.SaveChangesAsync();

            // Return the favorite donator DTO
            var favoriteDto = new FavoriteDonatorDto
            {
                FavoriteId = favorite.FavouriteId,
                FavoritedAt = favorite.FavoritedAt,
                DonatorProfile = new DonatorProfileDto
                {
                    DonatorId = donator.DonatorId,
                    RestaurantName = donator.RestaurantName,
                    ProfilePhotoUrl = donator.User!.ProfilePhotoUrl,
                    Address = donator.Address,
                    AddressStreet = donator.AddressStreet,
                    AddressMunicipality = donator.AddressMunicipality,
                    AddressCity = donator.AddressCity,
                    AddressCountry = donator.AddressCountry,
                    Latitude = donator.Latitude,
                    Longitude = donator.Longitude,
                    DonationStarts = donator.DonationStarts.ToString(@"HH:mm"),
                    DonationEnds = donator.DonationStarts.ToString(@"HH:mm"),
                    AverageScore = donator.AverageScore,
                    FavoritesCount = donator.FavoritesCount
                }
            };

            return Ok(favoriteDto);
        }
        catch (Exception e)
        {
            // Log the exception (you can use a logging framework here)
            Console.WriteLine($"Error adding favorite donator: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpDelete("by-donator/{donatorId:long}")]
    public async Task<IActionResult> RemoveFavoriteDonator(long donatorId)
    {
        try
        {
            // Get user ID from JWT claims
            var foodSeekerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (foodSeekerIdStr == null || !long.TryParse(foodSeekerIdStr, out var foodSeekerId))
                return Unauthorized();

            // Find the favorite donator entry
            var favorite = await _db.FavoriteDonators
                .FirstOrDefaultAsync(fd => fd.SeekerId == foodSeekerId && fd.DonatorId == donatorId);
            if (favorite == null)
                return NotFound("Favorite donator not found");

            // Find the donator profile to update favorites count
            var donator = await _db.DonatorProfiles.FindAsync(donatorId);
            if (donator == null)
                return NotFound("Donator not found");

            // Remove the favorite entry and update the donator's favorites count
            _db.FavoriteDonators.Remove(favorite);
            donator.FavoritesCount--;
            await _db.SaveChangesAsync();

            return Ok();
        }
        catch (Exception e)
        {
            // Log the exception (you can use a logging framework here)
            Console.WriteLine($"Error removing favorite donator: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
}