using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO.Common;
using FoodSeekerAPI.DTO.User;
using FoodSeekerAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace FoodSeekerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController(FoodSeekerContext db) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;

    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        try
        {
            // Get user ID from JWT claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            // Query User including DonatorProfile if exists
            var user = await _db.Users
                .Include(u => u.DonatorProfile) // Include DonatorProfile navigation property
                .Include(u => u.NotificationLogs) // Include NotificationLogs for the user
                .Include(u => u.FavoriteDonators) // Include FavoriteDonators for the user
                .ThenInclude(fd => fd.DonatorProfile)
                .ThenInclude(d => d!.User) // Include DonatorProfile in FavoriteDonators
                .SingleOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            // Return only User data if not a donator
            var profile = new UserProfileDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                IsDonator = user.IsDonator,
                ProfilePhotoUrl = user.ProfilePhotoUrl
            };

            if (user.IsDonator)
            {
                // if the user is a donator, include DonatorProfile data as well
                var donatorProfile = user.DonatorProfile!;
                var donatorDto = new DonatorProfileDto
                {
                    DonatorId = donatorProfile.DonatorId,
                    RestaurantName = donatorProfile.RestaurantName,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    Address = donatorProfile.Address,
                    AddressStreet = donatorProfile.AddressStreet,
                    AddressMunicipality = donatorProfile.AddressMunicipality,
                    AddressCity = donatorProfile.AddressCity,
                    AddressCountry = donatorProfile.AddressCountry,
                    Latitude = donatorProfile.Latitude,
                    Longitude = donatorProfile.Longitude,
                    DonationStarts = donatorProfile.DonationStarts.ToString(@"HH:mm"),
                    DonationEnds = donatorProfile.DonationEnds.ToString(@"HH:mm"),
                    AverageScore = donatorProfile.AverageScore,
                    FavoritesCount = donatorProfile.FavoritesCount
                };
                profile.DonatorProfile = donatorDto;
            }

            // Prepare notifications to return
            var notifications = user.NotificationLogs
                .OrderByDescending(n => n.SentAt)
                .Select(n => new NotificationLogDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title ?? string.Empty,
                    Message = n.Message ?? string.Empty,
                    SentAt = n.SentAt,
                    IsRead = n.IsRead
                }).ToList();
            
            // Prepare favorite donators to return
            var favoriteDonators = user.FavoriteDonators!
                .OrderByDescending(fd => fd.FavoritedAt)
                .Select(fd => new FavoriteDonatorDto
                {
                    FavoriteId = fd.FavouriteId,
                    FavoritedAt = fd.FavoritedAt,
                    DonatorProfile = new DonatorProfileDto
                    {
                        DonatorId = fd.DonatorProfile!.DonatorId,
                        RestaurantName = fd.DonatorProfile.RestaurantName,
                        ProfilePhotoUrl = fd.DonatorProfile.User?.ProfilePhotoUrl,
                        Address = fd.DonatorProfile.Address,
                        AddressStreet = fd.DonatorProfile.AddressStreet,
                        AddressMunicipality = fd.DonatorProfile.AddressMunicipality,
                        AddressCity = fd.DonatorProfile.AddressCity,
                        AddressCountry = fd.DonatorProfile.AddressCountry,
                        Latitude = fd.DonatorProfile.Latitude,
                        Longitude = fd.DonatorProfile.Longitude,
                        DonationStarts = fd.DonatorProfile.DonationStarts.ToString(@"HH:mm"),
                        DonationEnds = fd.DonatorProfile.DonationEnds.ToString(@"HH:mm"),
                        AverageScore = fd.DonatorProfile.AverageScore,
                        FavoritesCount = fd.DonatorProfile.FavoritesCount
                    }
                }).ToList();
            
            return Ok(new { profile, notifications, favoriteDonators });
        }
        catch (Exception e)
        {
            // Log the exception (you can use a logging framework here)
            Console.WriteLine($"(ERROR) Exception in GetProfile: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }


    // GET: api/User
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        // log the endpoint and the request time
        Console.WriteLine($"(LOG) GET Request to {HttpContext.Request.Path} at {DateTime.UtcNow}");
        try
        {
            var users = await _db.Users.ToListAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/User/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(long id)
    {
        try
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/User
    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        try
        {
            user.CreatedAt = DateTime.UtcNow;
            _db.Users.Add(user);

            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }
        catch (DbUpdateException)
        {
            if (_db.Users.Any(u => u.UserId == user.UserId))
                return Conflict();
            throw;
        }
    }

    // PUT: api/User/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(long id, User user)
    {
        if (id != user.UserId)
            return BadRequest();

        var existingUser = await _db.Users.FindAsync(id);
        if (existingUser == null)
            return NotFound();

        // Update fields
        existingUser.FullName = user.FullName;
        existingUser.Email = user.Email;
        existingUser.IsDonator = user.IsDonator;
        existingUser.ProfilePhotoUrl = user.ProfilePhotoUrl;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/User/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}