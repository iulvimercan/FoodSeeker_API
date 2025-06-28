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

/// <summary>
/// Handles authentication and account-related endpoints (login, sign-up, email verification, password reset).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(
    FoodSeekerContext db,
    TokenService tokenService,
    PasswordService passwordService,
    EmailService emailService)
    : ControllerBase
{
    // Injected dependencies via primary constructor
    private readonly FoodSeekerContext _db = db;
    private readonly TokenService _tokenService = tokenService;
    private readonly PasswordService _passwordService = passwordService;
    private readonly EmailService _emailService = emailService;

    /// <summary>
    /// Authenticates the user and returns a JWT token if valid.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        try
        {
            Console.WriteLine($"(LOG) Login attempt with email: {dto.Email} at {DateTime.UtcNow}");

            // 1. Check if the user exists
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user is null)
                return Unauthorized("Invalid email or password");

            // 2. Validate password hash
            if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password");

            // 3. If user is not verified, send a verification email and deny login
            if (!user.IsVerified)
            {
                var emailVerificationToken = _tokenService.GenerateEmailVerificationToken(user.UserId, user.Email);
                var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={emailVerificationToken}";

                const string subject = "Email Verification Required";
                string body = $"<h1>Email Verification Required</h1><p>Please verify your email by clicking <a href='{verificationLink}'>here</a>. If this wasn't you, please ignore this email.</p>";

                await _emailService.SendEmailAsync(user.Email, subject, body);
                return BadRequest("Please verify your email before logging in.");
            }

            // 4. Generate JWT access token
            var token = _tokenService.GenerateApiAccessToken(user.UserId, user.IsDonator);

            // 5. Return token
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) Login error: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Registers a new user as a Food Seeker.
    /// </summary>
    [HttpPost("sign-up/food-seeker")]
    public async Task<IActionResult> SignUpFoodSeeker([FromBody] SignUpFoodSeekerRequestDto dto)
    {
        try
        {
            Console.WriteLine($"(LOG) Sign-up attempt with email: {dto.Email} at {DateTime.UtcNow}");

            // 1. Check for existing user
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                // If already verified, reject
                if (existingUser.IsVerified)
                    return Conflict("User with this email already exists.");

                // Remove unverified user (and donator profile if exists)
                if (existingUser.IsDonator)
                {
                    var donatorProfile = await _db.DonatorProfiles.FirstOrDefaultAsync(dp => dp.DonatorId == existingUser.UserId);
                    if (donatorProfile != null) _db.DonatorProfiles.Remove(donatorProfile);
                }

                _db.Users.Remove(existingUser);
                await _db.SaveChangesAsync();
                Console.WriteLine($"(LOG) Removed unverified duplicate: {dto.Email}");
            }

            // 2. Hash password
            var passwordHash = _passwordService.HashPassword(dto.Password);

            // 3. Create new user record
            var newUser = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                ProfilePhotoUrl = dto.ProfilePhotoUrl,
                IsDonator = false,
                CreatedAt = DateTime.UtcNow,
            };
            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            // 4. Send email verification link
            var token = _tokenService.GenerateEmailVerificationToken(newUser.UserId, newUser.Email);
            var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={token}";

            const string subject = "Welcome to FoodSeeker!";
            string body = $"<h1>Welcome to FoodSeeker!</h1><p>Please verify your email <a href='{verificationLink}'>here</a>. Ignore if you didn't register.</p>";
            await _emailService.SendEmailAsync(dto.Email, subject, body);

            return Ok("Sign-up successful! Please check your email to verify your account.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) Food Seeker Sign-up error: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Registers a new user as a Donator and creates a Donator profile.
    /// </summary>
    [HttpPost("sign-up/donator")]
    public async Task<IActionResult> SignUpDonator([FromBody] SignUpDonatorRequestDto dto)
    {
        try
        {
            Console.WriteLine($"(LOG) Donator sign-up attempt: {dto.Email}");

            // 1. Handle duplicate unverified user
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                if (existingUser.IsVerified)
                    return Conflict("User with this email already exists.");

                if (existingUser.IsDonator)
                {
                    var donatorProfile = await _db.DonatorProfiles.FirstOrDefaultAsync(dp => dp.DonatorId == existingUser.UserId);
                    if (donatorProfile != null) _db.DonatorProfiles.Remove(donatorProfile);
                }

                _db.Users.Remove(existingUser);
                await _db.SaveChangesAsync();
                Console.WriteLine($"(LOG) Removed unverified donator: {dto.Email}");
            }

            // 2. Hash password and create new donator user
            var passwordHash = _passwordService.HashPassword(dto.Password);

            var newUser = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                ProfilePhotoUrl = dto.ProfilePhotoUrl,
                IsDonator = true,
                CreatedAt = DateTime.UtcNow,
            };
            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            // 3. Create DonatorProfile
            var newProfile = new DonatorProfile
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
            _db.DonatorProfiles.Add(newProfile);
            await _db.SaveChangesAsync();

            // 4. Send email verification
            var token = _tokenService.GenerateEmailVerificationToken(newUser.UserId, newUser.Email);
            var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={token}";

            const string subject = "Welcome to FoodSeeker!";
            string body = $"<h1>Welcome to FoodSeeker!</h1><p>Thank you for signing up as a donator. Please verify your email <a href='{verificationLink}'>here</a>.</p>";
            await _emailService.SendEmailAsync(dto.Email, subject, body);

            return Ok("Sign-up successful! Please check your email to verify your account.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) Donator Sign-up error: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Verifies user's email address from the token.
    /// </summary>
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        try
        {
            // 1. Validate token
            var principal = _tokenService.ValidateEmailVerificationToken(token);
            if (principal == null)
                return BadRequest("Invalid or expired token.");

            // 2. Extract user ID from claims
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
                return BadRequest("Invalid token structure.");

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            // 3. If already verified, optionally update email
            if (user.IsVerified)
            {
                var email = principal.FindFirstValue(ClaimTypes.Email);
                if (user.Email != email)
                {
                    user.Email = email!;
                    _db.Users.Update(user);
                    await _db.SaveChangesAsync();
                    return Ok("Email updated successfully.");
                }

                return Ok("Email is already verified.");
            }

            // 4. Mark as verified
            user.IsVerified = true;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return Ok("Email verified successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) Email verification error: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Sends a password reset email with a token.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        try
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return NotFound("User with this email does not exist.");

            var token = _tokenService.GeneratePasswordResetToken(user.UserId, user.Email);
            var resetLink = $"{Request.Scheme}://{Request.Host}/api/auth/reset-password?token={token}";

            const string subject = "Password Reset Request";
            string body = $"<h1>Password Reset Request</h1><p>To reset your password, copy and paste the token: <strong>{token}</strong> into the reset screen.</p>";
            await _emailService.SendEmailAsync(dto.Email, subject, body);

            return Ok("Password reset token has been sent to your email.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) Forgot password error: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Resets the user's password using the provided token.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        try
        {
            var principal = _tokenService.ValidatePasswordResetToken(dto.Token);
            if (principal == null)
                return BadRequest("Invalid or expired token.");

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
                return BadRequest("Invalid token structure.");

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return Ok("Password has been reset successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) Password reset error: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
}
