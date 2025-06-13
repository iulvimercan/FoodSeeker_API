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
                // Return User + DonatorProfile data
                var donatorDto = new DonatorProfileDto
                {
                    DonatorId = user.DonatorProfile!.DonatorId,
                    RestaurantName = user.DonatorProfile.RestaurantName,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    Address = user.DonatorProfile.Address,
                    AddressStreet = user.DonatorProfile.AddressStreet,
                    AddressMunicipality = user.DonatorProfile.AddressMunicipality,
                    AddressCity = user.DonatorProfile.AddressCity,
                    AddressCountry = user.DonatorProfile.AddressCountry,
                    Latitude = user.DonatorProfile.Latitude,
                    Longitude = user.DonatorProfile.Longitude,
                    DonationStarts = user.DonatorProfile.DonationStarts.ToString(@"HH:mm"),
                    DonationEnds = user.DonatorProfile.DonationEnds.ToString(@"HH:mm"),
                    AverageScore = user.DonatorProfile.AverageScore,
                    FavoritesCount = user.DonatorProfile.FavoritesCount
                };

                profile.DonatorProfile = donatorDto;
            }

            var notifications = await _db.NotificationLogs
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt)
                .Select(n => new NotificationLogDto {
                    NotificationId = n.NotificationId,
                    Title = n.Title ?? string.Empty,
                    Message = n.Message ?? string.Empty,
                    SentAt = n.SentAt,
                    IsRead = n.IsRead
                })
                .ToListAsync();

            return Ok(new { profile, notifications });
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