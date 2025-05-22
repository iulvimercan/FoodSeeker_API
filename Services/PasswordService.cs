using BCrypt.Net;

namespace FoodSeekerAPI.Services;

public class PasswordService
{
    // Hash the password securely
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // Verify a raw password against its hashed version
    public bool VerifyPassword(string inputPassword, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
    }
}