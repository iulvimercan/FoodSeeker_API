using FoodSeekerAPI.Data;
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
                    FoodItemsCount = d.FoodItems!.Count
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
}