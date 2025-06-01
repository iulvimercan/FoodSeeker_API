using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using FoodSeekerAPI.DTO.Auth;
using FoodSeekerAPI.DTO.Common;
using FoodSeekerAPI.DTO.User;
using FoodSeekerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodSeekerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    FoodSeekerContext db,
    TokenService tokenService,
    PasswordService passwordService,
    EmailService emailService)
    : ControllerBase
{
    // Dependencies injected via constructor
    private readonly FoodSeekerContext _db = db;
    private readonly TokenService _tokenService = tokenService;
    private readonly PasswordService _passwordService = passwordService;
    private readonly EmailService _emailService = emailService;

    /// <summary>
    /// Handles user login requests.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        try
        {
            Console.WriteLine($"(LOG) Login attempt with email: {dto.Email} at {DateTime.UtcNow}");

            // 1. Find user by email
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user is null)
                return Unauthorized("Invalid email or password");

            // 2. Validate the password hash
            if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password");

            // 3. Check if user email is verified; if not, send verification email
            if (!user.IsVerified)
            {
                var emailVerificationToken = _tokenService.GenerateEmailVerificationToken(user.UserId, user.Email);
                var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={emailVerificationToken}";

                // Email content for verification
                const string subject = "Email Verification Required";
                string body = $"<h1>Email Verification Required</h1><p>Please verify your email by clicking <a href='{verificationLink}'>here</a>. If this wasn't you, please ignore this email.</p>";
                await _emailService.SendEmailAsync(user.Email, subject, body);

                return BadRequest("Please verify your email before logging in.");
            }

            // 4. Fetch user profile including DonatorProfile if exists
            var userProfileDto = new UserProfileDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                IsDonator = user.IsDonator,
                ProfilePhotoUrl = user.ProfilePhotoUrl
            };
            if (user.IsDonator)
            {
                var donatorProfile = (await _db.DonatorProfiles.SingleOrDefaultAsync(d => d.DonatorId == user.UserId))!;
                var donatorProfileDto = new DonatorProfileDto
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
                
                userProfileDto.DonatorProfile = donatorProfileDto;
            }
            
            // 5. Generate JWT token for authenticated user
            var token = _tokenService.GenerateApiAccessToken(user.UserId, user.IsDonator);

            // 6. Return user info and token
            return Ok(new { token, user = userProfileDto });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) An error occurred during login: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Registers a new Food Seeker user.
    /// </summary>
    [HttpPost("sign-up/food-seeker")]
    public async Task<IActionResult> SignUpFoodSeeker([FromBody] SignUpFoodSeekerRequestDto dto)
    {
        try
        {
            Console.WriteLine($"(LOG) Sign-up attempt with email: {dto.Email} at {DateTime.UtcNow}");

            // 1. Check if email already exists
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                if (existingUser.IsVerified)
                    return Conflict("User with this email already exists.");

                // Remove unverified user and related donator profile if applicable
                if (existingUser.IsDonator)
                {
                    var donatorProfile = await _db.DonatorProfiles.FirstOrDefaultAsync(dp => dp.DonatorId == existingUser.UserId);
                    if (donatorProfile != null) _db.DonatorProfiles.Remove(donatorProfile);
                }

                _db.Users.Remove(existingUser);
                await _db.SaveChangesAsync();
                Console.WriteLine($"(LOG) Deleted unverified user with email: {dto.Email} at {DateTime.UtcNow}");
            }

            // 2. Hash the user's password
            var passwordHash = _passwordService.HashPassword(dto.Password);

            // 3. Create and save new user
            var newUser = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                IsDonator = false,
                CreatedAt = DateTime.UtcNow,
            };
            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            // 4. Generate email verification token and link
            var token = _tokenService.GenerateEmailVerificationToken(newUser.UserId, newUser.Email);
            var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={token}";

            // 5. Send welcome and verification email
            const string subject = "Welcome to FoodSeeker!";
            string body = $"<h1>Welcome to FoodSeeker!</h1><p>Thank you for signing up. Please verify your email <a href='{verificationLink}'>here</a>. Ignore if you didn't register.</p>";
            await _emailService.SendEmailAsync(dto.Email, subject, body);

            // 6. Respond success
            return Ok("Sign-up successful! Please check your email to verify your account.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) An error occurred during food seeker sign-up: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Registers a new Donator user along with their profile.
    /// </summary>
    [HttpPost("sign-up/donator")]
    public async Task<IActionResult> SignUpDonator([FromBody] SignUpDonatorRequestDto dto)
    {
        try
        {
            Console.WriteLine($"(LOG) Donator sign-up attempt with email: {dto.Email} at {DateTime.UtcNow}");

            // 1. Check if email already exists
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                if (existingUser.IsVerified)
                    return Conflict("User with this email already exists.");

                // Remove unverified user and related donator profile
                if (existingUser.IsDonator)
                {
                    var donatorProfile = await _db.DonatorProfiles.FirstOrDefaultAsync(dp => dp.DonatorId == existingUser.UserId);
                    if (donatorProfile != null) _db.DonatorProfiles.Remove(donatorProfile);
                }

                _db.Users.Remove(existingUser);
                await _db.SaveChangesAsync();
                Console.WriteLine($"(LOG) Deleted unverified user with email: {dto.Email} at {DateTime.UtcNow}");
            }

            // 2. Hash the password
            var passwordHash = _passwordService.HashPassword(dto.Password);

            // 3. Create and save new donator user
            var newUser = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                IsDonator = true,
                CreatedAt = DateTime.UtcNow,
            };
            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            // 4. Create donator profile linked to user
            var newDonatorProfile = new DonatorProfile
            {
                DonatorId = newUser.UserId,
                RestaurantName = dto.RestaurantName,
                Address = dto.RestaurantAddress,
                AddressStreet = dto.RestaurantAddressStreet,
                AddressMunicipality = dto.RestaurantAddressMunicipality,
                AddressCity = dto.RestaurantAddressCity,
                AddressCountry = dto.RestaurantAddressCountry,
                Latitude = double.Parse(dto.RestaurantLatitude),
                Longitude = double.Parse(dto.RestaurantLongitude),
                DonationStarts = dto.DonationStarts,
                DonationEnds = dto.DonationEnds,
            };
            _db.DonatorProfiles.Add(newDonatorProfile);
            await _db.SaveChangesAsync();

            // 5. Generate email verification token and send email
            var token = _tokenService.GenerateEmailVerificationToken(newUser.UserId, newUser.Email);
            var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={token}";

            const string subject = "Welcome to FoodSeeker!";
            string body = $"<h1>Welcome to FoodSeeker!</h1><p>Thank you for signing up as a donator. Please verify your email <a href='{verificationLink}'>here</a>. Ignore if you didn't register.</p>";
            await _emailService.SendEmailAsync(dto.Email, subject, body);

            // 6. Respond success
            return Ok("Sign-up successful! Please check your email to verify your account.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) An error occurred during donator sign-up: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Verifies user's email address using a token.
    /// </summary>
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        try
        {
            // Validate the token and extract claims principal
            var principal = _tokenService.ValidateEmailVerificationToken(token);
            if (principal == null)
            {
                Console.WriteLine($"(ERROR) Invalid email verification token at {DateTime.UtcNow}");
                return BadRequest("Invalid or expired token.");
            }

            // Extract user ID claim from token
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                Console.WriteLine($"(ERROR) Token missing user ID claim at {DateTime.UtcNow}");
                return BadRequest("Invalid token structure.");
            }

            if (!long.TryParse(userIdClaim.Value, out long userId))
            {
                Console.WriteLine($"(ERROR) Invalid user ID format in token at {DateTime.UtcNow}");
                return BadRequest("Invalid token structure.");
            }

            // Find user by ID
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"(ERROR) User not found for ID {userId} at {DateTime.UtcNow}");
                return NotFound("User not found.");
            }

            // If already verified, inform client
            if (user.IsVerified)
            {
                Console.WriteLine($"(LOG) User {userId} already verified at {DateTime.UtcNow}");
                return Ok("Email is already verified.");
            }

            // Mark user as verified
            user.IsVerified = true;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            Console.WriteLine($"(LOG) User {userId} verified at {DateTime.UtcNow}");
            return Ok("Email verified successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) An error occurred during email verification: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
}
