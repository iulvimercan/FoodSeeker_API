using System.Security.Claims;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using FoodSeekerAPI.DTO.Auth;
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
    private readonly FoodSeekerContext _db = db;
    private readonly TokenService _tokenService = tokenService;
    private readonly PasswordService _passwordService = passwordService;
    private readonly EmailService _emailService = emailService;


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        try
        {
            Console.WriteLine($"(LOG) Login attempt with email: {dto.Email} at {DateTime.UtcNow}");

            // 1. Find user by email
            var user = _db.Users.SingleOrDefault(u => u.Email == dto.Email);
            if (user is null)
                return Unauthorized("Invalid email or password");

            // 2. Compare hashed passwords
            if (_passwordService.VerifyPassword(dto.Password, user.PasswordHash) == false)
                return Unauthorized("Invalid email or password");

            // 3. Check if user is verified. If not, send a verification email
            if (user.IsVerified == false)
            {
                var emailVerificationToken = _tokenService.GenerateEmailVerificationToken(user.UserId, user.Email);
                var verificationLink =
                    $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={emailVerificationToken}";

                // Send verification email
                const string subject = "Email Verification Required";
                string body =
                    $"<h1>Email Verification Required</h1><p>Please use the following link to verify your email: <a href=\' {verificationLink} \'>Verify Email</a>.<b>Please ignore this email if you are not trying to login.</b></p>";
                await _emailService.SendEmailAsync(user.Email, subject, body);

                return BadRequest("Please verify your email before logging in.");
            }

            // 4. Generate token
            var token = _tokenService.GenerateApiAccessToken(user.UserId, user.IsDonator);

            // 5. Return token
            return Ok(new { user.UserId, user.IsDonator, token });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) An error occurred during login: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    [HttpPost("sign-up/food-seeker")]
    public async Task<IActionResult> SignUpFoodSeeker([FromBody] SignUpFoodSeekerRequestDto dto)
    {
        try
        {
            Console.WriteLine($"(LOG) Sign-up attempt with email: {dto.Email} at {DateTime.UtcNow}");

            // 1. Check if user already exists
            User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user != null)
            {
                if (user.IsVerified)
                {
                    return Conflict("User with this email already exists.");
                }

                // delete the unverified user from users table and also from DonatorProfiles table if the type is donator
                if (user.IsDonator)
                {
                    DonatorProfile? donatorProfile = await _db.DonatorProfiles
                        .FirstOrDefaultAsync(dp => dp.DonatorId == user.UserId);

                    if (donatorProfile != null)
                    {
                        _db.DonatorProfiles.Remove(donatorProfile);
                    }
                }

                _db.Users.Remove(user);
                await _db.SaveChangesAsync();

                Console.WriteLine($"(LOG) Deleted unverified user with email: {dto.Email} at {DateTime.UtcNow}");
            }

            // 2. Hash the password
            var passwordHash = _passwordService.HashPassword(dto.Password);

            // 3. Create a new user
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

            // 4. Generate token
            var token = _tokenService.GenerateEmailVerificationToken(newUser.UserId, newUser.Email);
            var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={token}";

            // 5. Send welcome email for greeting and the email verification
            const string subject = "Welcome to FoodSeeker!";
            string body =
                $"<h1>Welcome to FoodSeeker!</h1><p>Thank you for signing up.<br>Please use the following link to verify your email and complete the registration: <a href=\' {verificationLink} \'>Verify Email</a>. <b>If you are not the one who is trying to register, ignore this email.</b></p>";
            await _emailService.SendEmailAsync(dto.Email, subject, body);

            // 6. Return the success response
            return Ok("Sign-up successful! Please check your email inbox to verify your account.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) An error occurred during food seeker sign-up: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    [HttpPost("sign-up/donator")]
    public async Task<IActionResult> SignUpDonator([FromBody] SignUpDonatorRequestDto dto)
    {
        try
        {
            Console.WriteLine($"(LOG) Donator sign-up attempt with email: {dto.Email} at {DateTime.UtcNow}");

            // 1. Check if user already exists
            User? user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user != null)
            {
                if (user.IsVerified)
                {
                    return Conflict("User with this email already exists.");
                }

                // delete the unverified user from users table and also from DonatorProfiles table if the type is donator
                if (user.IsDonator)
                {
                    DonatorProfile? donatorProfile = await _db.DonatorProfiles
                        .FirstOrDefaultAsync(dp => dp.DonatorId == user.UserId);

                    if (donatorProfile != null)
                    {
                        _db.DonatorProfiles.Remove(donatorProfile);
                    }
                }

                _db.Users.Remove(user);
                await _db.SaveChangesAsync();

                Console.WriteLine($"(LOG) Deleted unverified user with email: {dto.Email} at {DateTime.UtcNow}");
            }

            // 2. Hash the password
            var passwordHash = _passwordService.HashPassword(dto.Password);

            // 3. Create a new user
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

            // 4. Create a new donator profile
            var newDonator = new DonatorProfile
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

            _db.DonatorProfiles.Add(newDonator);
            await _db.SaveChangesAsync();

            // 5. Generate token
            var token = _tokenService.GenerateEmailVerificationToken(newUser.UserId, newUser.Email);
            var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={token}";
            // 6. Send welcome email for greeting and the email verification
            const string subject = "Welcome to FoodSeeker!";

            string body =
                $"<h1>Welcome to FoodSeeker!</h1><p>Thank you for signing up as a donator.<br>Please use the following link to verify your email and complete the registration: <a href=\' {verificationLink} \'>Verify Email</a>. <b>If you are not the one who is trying to register, ignore this email.</b></p>";
            await _emailService.SendEmailAsync(dto.Email, subject, body);
            return Ok("Sign-up successful! Please check your email inbox to verify your account.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) An error occurred during donator sign-up: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        try
        {
            var principal = _tokenService.ValidateEmailVerificationToken(token);
            if (principal == null)
            {
                Console.WriteLine($"(ERROR) Invalid email verification token at {DateTime.UtcNow}");
                return BadRequest("Invalid or expired token.");
            }

            // Extract user ID from token claims
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                Console.WriteLine($"(ERROR) Token does not contain user ID claim at {DateTime.UtcNow}");
                return BadRequest("Invalid token structure.");
            }

            if (!long.TryParse(userIdClaim.Value, out long userId))
            {
                Console.WriteLine($"(ERROR) Invalid user ID in token at {DateTime.UtcNow}");
                return BadRequest("Invalid token structure.");
            }

            // 1. Find user by ID
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                Console.WriteLine($"(ERROR) User not found for ID: {userId} at {DateTime.UtcNow}");
                return NotFound("User not found.");
            }

            // 2. Check if user is already verified
            if (user.IsVerified)
            {
                Console.WriteLine($"(LOG) User with ID: {userId} is already verified at {DateTime.UtcNow}");
                return Ok("Email is already verified.");
            }

            // 3. Mark user as verified
            user.IsVerified = true;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            Console.WriteLine($"(LOG) User with ID: {userId} has been verified successfully at {DateTime.UtcNow}");

            return Ok("Email is verified successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) An error occurred during email verification: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }
}