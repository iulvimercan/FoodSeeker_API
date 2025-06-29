using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FoodSeekerAPI.Services;

public class TokenService(IConfiguration configuration)
{
    // Constants for the security algorithm used for signing tokens
    public const string DefaultSecurityAlgorithm = SecurityAlgorithms.HmacSha256;
    public const string DefaultSecurityAlgorithmSignature = SecurityAlgorithms.HmacSha256Signature;

    /// <summary>
    /// Provides the default token validation parameters using the app's configuration.
    /// This is used to validate tokens consistently.
    /// </summary>
    public static TokenValidationParameters GetDefaultTokenValidationParameters(IConfiguration configuration)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,         // Validate that the issuer is correct
            ValidateAudience = true,       // Validate that the audience matches
            ValidateLifetime = true,       // Validate token expiry
            ValidateIssuerSigningKey = true,  // Validate token signature

            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
        };
    }

    /// <summary>
    /// Generates a JWT access token for API authentication.
    /// </summary>
    /// <param name="userId">User's unique ID</param>
    /// <param name="isDonator">Whether user is a donator or seeker (for role claim)</param>
    /// <param name="validForMonths">Token expiration duration in months</param>
    /// <returns>Signed JWT as a string</returns>
    public string GenerateApiAccessToken(long userId, bool isDonator, int validForMonths = 6)
    {
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);
        var issuer = configuration["Jwt:Issuer"];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // User ID claim
                new Claim(ClaimTypes.Role, isDonator ? "Donator" : "FoodSeeker") // Role claim
            }),
            Expires = DateTime.UtcNow.AddMonths(validForMonths),
            Issuer = issuer,
            Audience = issuer,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), DefaultSecurityAlgorithmSignature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generates a short-lived email verification token.
    /// </summary>
    public string GenerateEmailVerificationToken(long userId, string email, int validForMinutes = 15)
    {
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);
        var issuer = configuration["Jwt:Issuer"];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim("purpose", "email_verification") // Custom claim to identify token purpose
            }),
            Expires = DateTime.UtcNow.AddMinutes(validForMinutes),
            Issuer = issuer,
            Audience = issuer,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), DefaultSecurityAlgorithmSignature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Validates the email verification token and returns claims principal if valid.
    /// Returns null if token invalid or expired.
    /// </summary>
    public ClaimsPrincipal? ValidateEmailVerificationToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetDefaultTokenValidationParameters(configuration);

            // Throws if invalid token or expired
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // Check the token algorithm is correct (avoid none alg attacks)
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(DefaultSecurityAlgorithm, StringComparison.InvariantCultureIgnoreCase))
                return null;

            // Ensure token purpose claim matches
            var purposeClaim = principal.FindFirst("purpose")?.Value;
            if (purposeClaim != "email_verification")
                return null;

            return principal;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Generates a short-lived password reset token.
    /// </summary>
    public string GeneratePasswordResetToken(long userId, string email, int validForMinutes = 15)
    {
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);
        var issuer = configuration["Jwt:Issuer"];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim("purpose", "forgot_password") // Custom claim for password reset
            }),
            Expires = DateTime.UtcNow.AddMinutes(validForMinutes),
            Issuer = issuer,
            Audience = issuer,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), DefaultSecurityAlgorithmSignature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Validates the password reset token and returns claims principal if valid.
    /// Returns null if token invalid or expired.
    /// </summary>
    public ClaimsPrincipal? ValidatePasswordResetToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetDefaultTokenValidationParameters(configuration);

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(DefaultSecurityAlgorithm, StringComparison.InvariantCultureIgnoreCase))
                return null;

            var purposeClaim = principal.FindFirst("purpose")?.Value;
            if (purposeClaim != "forgot_password")
                return null;

            return principal;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
