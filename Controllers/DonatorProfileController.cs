using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO.DonatorProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

[Route("api/donator")]
[ApiController]
public class DonatorProfileController(FoodSeekerContext db) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;


    [HttpGet("nearby")]
    public async Task<IActionResult> GetDonatorsByLocation(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double distanceKm = 30)
    {
        try
        {
            double degLat = distanceKm / 111.32;
            double degLon = distanceKm / (111.32 * Math.Cos(latitude * Math.PI / 180));

            var donators = await _db.DonatorProfiles
                .Where(d => d.Latitude >= latitude - degLat &&
                            d.Latitude <= latitude + degLat &&
                            d.Longitude >= longitude - degLon &&
                            d.Longitude <= longitude + degLon)
                .Include(d => d.FoodItems)
                .Include(d => d.User)
                .Select(d => new DTO.Common.DonatorProfileDto
                {
                    DonatorId = d.DonatorId,
                    RestaurantName = d.RestaurantName,
                    ProfilePhotoUrl = d.User!.ProfilePhotoUrl,
                    Address = d.Address,
                    AddressStreet = d.AddressStreet,
                    AddressMunicipality = d.AddressMunicipality,
                    AddressCity = d.AddressCity,
                    AddressCountry = d.AddressCountry,
                    Latitude = d.Latitude,
                    Longitude = d.Longitude,
                    Distance = Utils.GeoUtils.CalculateDistance(latitude, longitude, d.Latitude, d.Longitude),
                    DonationStarts = d.DonationStarts.ToString("HH:mm"),
                    DonationEnds = d.DonationEnds.ToString("HH:mm"),
                    AverageScore = d.AverageScore,
                    FavoritesCount = d.FavoritesCount,
                    FoodItemsCount = Enumerable.Count(d.FoodItems!, fi => fi.IsActive)
                })
                .ToListAsync();

            return Ok(donators);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error fetching nearby donators: " + e.Message);
            return StatusCode(500, $"An error occurred while fetching nearby donators: {e.Message}");
        }
    }

    [HttpGet("profile/{donatorId:long}")]
    public async Task<IActionResult> GetDonatorProfileById(long donatorId)
    {
        try
        {
            var donatorProfile = await _db.DonatorProfiles
                .Where(d => d.DonatorId == donatorId)
                .Include(d => d.User)
                .Include(d => d.FoodItems)
                .Select(d => new DTO.Common.DonatorProfileDto
                {
                    DonatorId = d.DonatorId,
                    RestaurantName = d.RestaurantName,
                    ProfilePhotoUrl = d.User!.ProfilePhotoUrl,
                    Address = d.Address,
                    AddressStreet = d.AddressStreet,
                    AddressMunicipality = d.AddressMunicipality,
                    AddressCity = d.AddressCity,
                    AddressCountry = d.AddressCountry,
                    Latitude = d.Latitude,
                    Longitude = d.Longitude,
                    DonationStarts = d.DonationStarts.ToString("HH:mm"),
                    DonationEnds = d.DonationEnds.ToString("HH:mm"),
                    AverageScore = d.AverageScore,
                    FavoritesCount = d.FavoritesCount,
                    FoodItemsCount = Enumerable.Count(d.FoodItems!, fi => fi.IsActive)
                })
                .FirstOrDefaultAsync();

            if (donatorProfile == null)
            {
                return NotFound($"Donator profile with ID {donatorId} not found.");
            }

            return Ok(new { donatorProfile });
        }
        catch (Exception e)
        {
            Console.WriteLine("Error fetching donator profile: " + e.Message);
            return StatusCode(500, $"An error occurred while fetching the donator profile: {e.Message}");
        }
    }

    [HttpPut("update-restaurant-name")]
    [Authorize(Roles = "Donator")]
    public async Task<IActionResult> UpdateRestaurantName([FromBody] UpdateRestaurantNameRequestDto dto)
    {
        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null || !long.TryParse(userIdString, out long userId))
                return Unauthorized("User ID not found in token.");

            var donatorProfile = await _db.DonatorProfiles.Where(dp => dp.DonatorId == userId).FirstOrDefaultAsync();
            if (donatorProfile == null)
                return NotFound($"Donator profile not found for user ID {userId}.");

            donatorProfile.RestaurantName = dto.RestaurantName;
            _db.DonatorProfiles.Update(donatorProfile);
            await _db.SaveChangesAsync();

            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error updating restaurant name: " + e.Message);
            return StatusCode(500, $"An error occurred while updating the restaurant name: {e.Message}");
        }
    }

    [HttpPut("update-restaurant-address")]
    [Authorize(Roles = "Donator")]
    public async Task<IActionResult> UpdateRestaurantAddress([FromBody] UpdateRestaurantAddressRequestDto dto)
    {
        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null || !long.TryParse(userIdString, out long userId))
                return Unauthorized("User ID not found in token.");

            var donatorProfile = await _db.DonatorProfiles.Where(dp => dp.DonatorId == userId).FirstOrDefaultAsync();
            if (donatorProfile == null)
                return NotFound($"Donator profile not found for user ID {userId}.");

            donatorProfile.Address = dto.Address;
            donatorProfile.AddressStreet = dto.AddressStreet;
            donatorProfile.AddressMunicipality = dto.AddressMunicipality;
            donatorProfile.AddressCity = dto.AddressCity;
            donatorProfile.AddressCountry = dto.AddressCountry;
            donatorProfile.Latitude = dto.Latitude;
            donatorProfile.Longitude = dto.Longitude;

            _db.DonatorProfiles.Update(donatorProfile);
            await _db.SaveChangesAsync();

            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error updating restaurant address: " + e.Message);
            return StatusCode(500, $"An error occurred while updating the restaurant address: {e.Message}");
        }
    }

    [HttpPut("update-donation-times")]
    [Authorize(Roles = "Donator")]
    public async Task<IActionResult> UpdateDonationStartTime([FromBody] UpdateDonationTimeRequestDto dto)
    {
        try
        {
            Console.WriteLine("1");
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null || !long.TryParse(userIdString, out long userId))
                return Unauthorized("User ID not found in token.");
            Console.WriteLine("2");

            var donatorProfile = await _db.DonatorProfiles.Where(dp => dp.DonatorId == userId).FirstOrDefaultAsync();
            if (donatorProfile == null)
                return NotFound($"Donator profile not found for user ID {userId}.");

            Console.WriteLine("3");
            if (dto.DonationStarts.HasValue)
                donatorProfile.DonationStarts = dto.DonationStarts.Value;

            Console.WriteLine("4");
            if (dto.DonationEnds.HasValue)
                donatorProfile.DonationEnds = dto.DonationEnds.Value;
            Console.WriteLine("5");
            _db.DonatorProfiles.Update(donatorProfile);
            Console.WriteLine("6");
            await _db.SaveChangesAsync();
            Console.WriteLine("7");
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error updating donation start time: " + e.Message);
            return StatusCode(500, $"An error occurred while updating the donation start time: {e.Message}");
        }
    }
}