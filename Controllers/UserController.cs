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
[Authorize]
public class UserController(FoodSeekerContext db, TokenService tokenService, EmailService emailService, PasswordService passwordService) : ControllerBase
{
    private readonly FoodSeekerContext _db = db;
    private readonly TokenService _tokenService = tokenService;
    private readonly EmailService _emailService = emailService;
    private readonly PasswordService _passwordService = passwordService;
    
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
    
    [HttpPut("update-name")]
    [Authorize]
    public async Task<IActionResult> UpdateUserName([FromBody] UpdateUserNameRequestDto dto)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            // Update user's full name
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
    
    [HttpPut("update-email")]
    [Authorize]
    public async Task<IActionResult> UpdateUserEmail([FromBody] UpdateUserEmailRequestDto dto)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound();
            
            var token = _tokenService.GenerateEmailVerificationToken(user.UserId, dto.Email, 5);
            var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={token}";

            const string subject = "FoodSeeker - Changing Account Email";
            string body = $"<h1>Hi there!</h1><p>You requested to change your current email address to '{dto.Email}'. To complete the process, please verify your new email <a href='{verificationLink}'>here</a>. Ignore if you don't want to.</p>";
            await _emailService.SendEmailAsync(user.Email, subject, body);
            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine($"(ERROR) Exception in UpdateUserEmail: {e.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPut("update-password")]
    [Authorize]
    public async Task<IActionResult> UpdateUserPassword([FromBody] UpdateUserPasswordRequestDto dto)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            // Update user's password
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
    
    [HttpPut("update-profile-photo")]
    [Authorize]
    public async Task<IActionResult> UpdateProfilePhoto([FromBody] UpdateProfilePhotoRequestDto dto)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null || !long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            // Update user's profile photo URL
            var newPhotoUrl = dto.ProfilePhotoUrl.Length > 0 ? dto.ProfilePhotoUrl : null;
            user.ProfilePhotoUrl = newPhotoUrl;
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