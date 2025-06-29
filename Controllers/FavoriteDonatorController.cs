using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO.Common;
using FoodSeekerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

/// <summary>
/// Manages operations related to food seekers' favorite donators.
/// </summary>
[Route("api/favorite-donator")]
[ApiController]
[Authorize(Roles = "FoodSeeker")]
public class FavoriteDonatorController(FoodSeekerContext db) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;

    /// <summary>
    /// Adds a donator to the current user's favorites.
    /// </summary>
    /// <param name="donatorId">The ID of the donator to favorite.</param>
    [HttpPost("{donatorId:long}")]
    public async Task<ActionResult<FavoriteDonatorDto>> AddFavoriteDonator(long donatorId)
    {
        try
        {
            // Extract seeker ID from JWT token
            var foodSeekerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (foodSeekerIdStr == null || !long.TryParse(foodSeekerIdStr, out var foodSeekerId))
                return Unauthorized();

            // Validate donator exists
            var donator = await _db.DonatorProfiles
                .Include(d => d.User)
                .SingleOrDefaultAsync(d => donatorId == d.DonatorId);
            if (donator == null)
                return NotFound("Donator not found");

            // Prevent duplicate favorites
            var existingFavorite = await _db.FavoriteDonators
                .FirstOrDefaultAsync(fd => fd.SeekerId == foodSeekerId && fd.DonatorId == donatorId);
            if (existingFavorite != null)
                return Conflict("This donator is already in your favorites");

            // Add to favorites
            var favorite = new FavoriteDonator
            {
                SeekerId = foodSeekerId,
                DonatorId = donatorId,
                FavoritedAt = DateTime.UtcNow
            };
            _db.FavoriteDonators.Add(favorite);

            // Increment the donator's favorites count
            donator.FavoritesCount++;
            await _db.SaveChangesAsync();

            // Return a DTO with donator info
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
                    DonationStarts = donator.DonationStarts.ToString("HH:mm"),
                    DonationEnds = donator.DonationEnds.ToString("HH:mm"),
                    AverageScore = donator.AverageScore,
                    FavoritesCount = donator.FavoritesCount
                }
            };

            return Ok(favoriteDto);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error adding favorite donator: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Removes a donator from the current user's favorites.
    /// </summary>
    /// <param name="donatorId">The ID of the donator to remove.</param>
    [HttpDelete("by-donator/{donatorId:long}")]
    public async Task<IActionResult> RemoveFavoriteDonator(long donatorId)
    {
        try
        {
            // Extract seeker ID from JWT token
            var foodSeekerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (foodSeekerIdStr == null || !long.TryParse(foodSeekerIdStr, out var foodSeekerId))
                return Unauthorized();

            // Find the favorite record
            var favorite = await _db.FavoriteDonators
                .FirstOrDefaultAsync(fd => fd.SeekerId == foodSeekerId && fd.DonatorId == donatorId);
            if (favorite == null)
                return NotFound("Favorite donator not found");

            // Fetch the donator profile for count update
            var donator = await _db.DonatorProfiles.FindAsync(donatorId);
            if (donator == null)
                return NotFound("Donator not found");

            // Remove from favorites and decrement count
            _db.FavoriteDonators.Remove(favorite);
            donator.FavoritesCount--;
            await _db.SaveChangesAsync();

            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error removing favorite donator: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
}
