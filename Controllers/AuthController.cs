using FoodSeekerAPI.Data;
using FoodSeekerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using FoodSeekerAPI.DTO.Auth;

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

    [HttpGet("email")]
    public async Task<IActionResult> Email([FromQuery] string email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email query parameter is required.");

            const string subject = "Test";
            const string body = "Test body";

            await _emailService.SendEmailAsync(email, subject, body);
            return Ok("The email is sent successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) An error occurred while sending the email: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }


    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto dto)
    {
        try
        {
            Console.WriteLine($"(LOG) Login attempt with email: {dto.Email} at {DateTime.Now}");

            // 1. Find user by email
            var user = _db.Users.SingleOrDefault(u => u.Email == dto.Email);
            if (user is null)
                return Unauthorized("Invalid email or password");

            // 2. Compare hashed passwords
            if (_passwordService.VerifyPassword(dto.Password, user.PasswordHash) == false)
                return Unauthorized("Invalid email or password");

            // 3. Generate token
            var token = _tokenService.GenerateToken(user.UserId, user.IsDonator);

            // 4. Return token
            return Ok(new { user.UserId, user.IsDonator, token });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) An error occurred during login: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred. Please try again later.");
        }
    }

    [HttpPost("sign-up/food-seeker")]
    public async Task<IActionResult> SignUpFoodSeeker(SignUpFoodSeekerRequestDto dto)
    {
        // Registration logic for food seekers
        return Ok("Unimplemented");
    }

    [HttpPost("sign-up/donator")]
    public async Task<IActionResult> SignUpDonator(SignUpDonatorRequestDto dto)
    {
        // Registration logic for donators
        return Ok("Unimplemented");
    }
}