using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.DTO.Common;
using FoodSeekerAPI.DTO.User;
using FoodSeekerAPI.Models;
using FoodSeekerAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace FoodSeekerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // All endpoints in this controller require authenticated users
public class UserController(FoodSeekerContext db, TokenService tokenService, EmailService emailService, PasswordService passwordService) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;
    private readonly TokenService _tokenService = tokenService;
    private readonly EmailService _emailService = emailService;
    private readonly PasswordService _passwordService = passwordService;

    /// <summary>
    /// Returns the full profile information of the authenticated user.
    /// Includes DonatorProfile (if user is a donator), notifications, and favorite donators.
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        try
        {
            // Extract user ID from token claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            // Load user with related DonatorProfile, NotificationLogs, and FavoriteDonators
            var user = await _db.Users
                .Include(u => u.DonatorProfile)
                .Include(u => u.NotificationLogs)
                .Include(u => u.FavoriteDonators)!
                    .ThenInclude(fd => fd.DonatorProfile)!
                    .ThenInclude(dp => dp!.User)
                .SingleOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            // Map basic profile info
            var profile = new UserProfileDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                IsDonator = user.IsDonator,
                ProfilePhotoUrl = user.ProfilePhotoUrl
            };

            // Add DonatorProfile if the user is a donator
            if (user.IsDonator)
            {
                var dp = user.DonatorProfile!;
                profile.DonatorProfile = new DonatorProfileDto
                {
                    DonatorId = dp.DonatorId,
                    RestaurantName = dp.RestaurantName,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    Address = dp.Address,
                    AddressStreet = dp.AddressStreet,
                    AddressMunicipality = dp.AddressMunicipality,
                    AddressCity = dp.AddressCity,
                    AddressCountry = dp.AddressCountry,
                    Latitude = dp.Latitude,
                    Longitude = dp.Longitude,
                    DonationStarts = dp.DonationStarts.ToString("HH:mm"),
                    DonationEnds = dp.DonationEnds.ToString("HH:mm"),
                    AverageScore = dp.AverageScore,
                    FavoritesCount = dp.FavoritesCount
                };
            }

            // Convert notifications to DTO
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

            // Convert favorites to DTO
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
                        DonationStarts = fd.DonatorProfile.DonationStarts.ToString("HH:mm"),
                        DonationEnds = fd.DonatorProfile.DonationEnds.ToString("HH:mm"),
                        AverageScore = fd.DonatorProfile.AverageScore,
                        FavoritesCount = fd.DonatorProfile.FavoritesCount
                    }
                }).ToList();

            return Ok(new { profile, notifications, favoriteDonators });
        }
        catch (Exception e)
        {
            Console.WriteLine($"(ERROR) Exception in GetProfile: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Returns all users in the system.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        Console.WriteLine($"(LOG) GET Request to {HttpContext.Request.Path} at {DateTime.UtcNow}");
        try
        {
            var users = await _db.Users.ToListAsync();
            return Ok(users);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Returns a specific user by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(long id)
    {
        try
        {
            var user = await _db.Users.FindAsync(id);
            return user == null ? NotFound() : Ok(user);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Adds a new user to the system.
    /// </summary>
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

    /// <summary>
    /// Updates the authenticated user's full name.
    /// </summary>
    [HttpPut("update-name")]
    public async Task<IActionResult> UpdateUserName([FromBody] UpdateUserNameRequestDto dto)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.FullName = dto.FullName;
            await _db.SaveChangesAsync();
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine($"(ERROR) Exception in UpdateUserName: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Initiates email change verification by sending a confirmation link to the new address.
    /// </summary>
    [HttpPut("update-email")]
    public async Task<IActionResult> UpdateUserEmail([FromBody] UpdateUserEmailRequestDto dto)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            // Generate email verification token and link
            var token = _tokenService.GenerateEmailVerificationToken(user.UserId, dto.Email, 5);
            var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={token}";

            const string subject = "FoodSeeker - Changing Account Email";
            string body = $"<h1>Hi there!</h1><p>You requested to change your email to '{dto.Email}'. " +
                          $"Please verify it <a href='{verificationLink}'>here</a>.</p>";

            await _emailService.SendEmailAsync(user.Email, subject, body);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine($"(ERROR) Exception in UpdateUserEmail: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates the authenticated user's password.
    /// </summary>
    [HttpPut("update-password")]
    public async Task<IActionResult> UpdateUserPassword([FromBody] UpdateUserPasswordRequestDto dto)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.PasswordHash = _passwordService.HashPassword(dto.Password);
            await _db.SaveChangesAsync();
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine($"(ERROR) Exception in UpdateUserPassword: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Updates the authenticated user's profile photo.
    /// </summary>
    [HttpPut("update-profile-photo")]
    public async Task<IActionResult> UpdateProfilePhoto([FromBody] UpdateProfilePhotoRequestDto dto)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.ProfilePhotoUrl = string.IsNullOrWhiteSpace(dto.ProfilePhotoUrl) ? null : dto.ProfilePhotoUrl;
            await _db.SaveChangesAsync();
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine($"(ERROR) Exception in UpdateProfilePhoto: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
}
