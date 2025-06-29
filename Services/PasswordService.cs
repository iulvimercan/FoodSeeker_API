using BCrypt.Net;

namespace FoodSeekerAPI.Services;

public class PasswordService
{
    /// <summary>
    /// Securely hashes a plain text password using the BCrypt algorithm.
    /// </summary>
    /// <param name="password">The raw plain text password to hash.</param>
    /// <returns>The hashed password string including salt.</returns>
    public string HashPassword(string password)
    {
        // Use BCrypt to hash the password with an automatically generated salt
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Verifies a plain text password against a stored BCrypt hashed password.
    /// </summary>
    /// <param name="inputPassword">The plain text password to verify.</param>
    /// <param name="storedHash">The stored BCrypt hashed password.</param>
    /// <returns>True if the input password matches the stored hash; otherwise, false.</returns>
    public bool VerifyPassword(string inputPassword, string storedHash)
    {
        // Use BCrypt to check if the input password corresponds to the stored hash
        return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
    }
}
