using FoodSeekerAPI.Data;
using FoodSeekerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using FoodSeekerAPI.DTO.Auth;

namespace FoodSeekerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FoodSeekerContext _db;
    private readonly TokenService _tokenService;
    private readonly PasswordService _passwordService;

    public AuthController(FoodSeekerContext db, TokenService tokenService, PasswordService passwordService)
    {
        _db = db;
        _tokenService = tokenService;
        _passwordService = passwordService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginReqDto loginReqDto)
    {
        try
        {
            Console.WriteLine($"(LOG) Login attempt with email: {loginReqDto.Email} at {DateTime.Now}");
        
            // 1. Find user by email
            var user = _db.Users.SingleOrDefault(u => u.Email == loginReqDto.Email);
            if (user is null)
                return Unauthorized("Invalid email or password");

            // 2. Compare hashed passwords
            if (_passwordService.VerifyPassword(loginReqDto.Password, user.PasswordHash) == false)
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
}