using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FoodSeekerAPI.Services;

public class TokenService(IConfiguration configuration)
{
    public const string DefaultSecurityAlgorithm = SecurityAlgorithms.HmacSha256;
    public const string DefaultSecurityAlgorithmSignature = SecurityAlgorithms.HmacSha256Signature;

    public static TokenValidationParameters GetDefaultTokenValidationParameters(IConfiguration configuration)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
        };
    }

    public string GenerateApiAccessToken(long userId, bool isDonator, int validForMonths = 6)
    {
        // 1. Get the secret key from configuration and convert to bytes
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);

        // 2. Get the issuer from configuration (usually your API or domain name)
        var issuer = configuration["Jwt:Issuer"];

        // 3. Create token descriptor - defines what's inside the token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // a) ClaimsIdentity: defines the user identity claims inside the token
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // Put the username as a claim
                new Claim(ClaimTypes.Role, isDonator ? "Donator" : "FoodSeeker") // Role claim based on user type
            }),

            // b) Expiration date/time for the token validity
            Expires = DateTime.UtcNow.AddMonths(validForMonths),

            // c) Issuer of the token (who issues it)
            Issuer = issuer,

            // d) Audience - who the token is intended for (usually same as issuer)
            Audience = issuer,

            // e) Signing credentials (how the token is signed and verified)
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), DefaultSecurityAlgorithmSignature)
        };

        // 4. Create a token handler to generate the JWT
        var tokenHandler = new JwtSecurityTokenHandler();

        // 5. Create the token based on the descriptor above
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // 6. Serialize the token to a string to send it to the client
        return tokenHandler.WriteToken(token);
    }

    public string GenerateEmailVerificationToken(long userId, string email, int validForMinutes = 15)
    {
        // 1. Get the secret key from configuration and convert to bytes
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);

        // 2. Get the issuer from configuration (usually your API or domain name)
        var issuer = configuration["Jwt:Issuer"];

        // 3. Create token descriptor - defines what's inside the token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // a) ClaimsIdentity: defines the user identity claims inside the token
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // Put the username as a claim
                new Claim(ClaimTypes.Email, email), // Include the user's email as a claim
                new Claim("purpose", "email_verification"), // Custom claim to indicate this is for email verification
            }),

            // b) Expiration date/time for the token validity
            Expires = DateTime.UtcNow.AddMinutes(validForMinutes),

            // c) Issuer of the token (who issues it)
            Issuer = issuer,

            // d) Audience - who the token is intended for (usually same as issuer)
            Audience = issuer,

            // e) Signing credentials (how the token is signed and verified)
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), DefaultSecurityAlgorithmSignature)
        };

        // 4. Create a token handler to generate the JWT
        var tokenHandler = new JwtSecurityTokenHandler();

        // 5. Create the token based on the descriptor above
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // 6. Serialize the token to a string to send it to the client
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateEmailVerificationToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return null;
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetDefaultTokenValidationParameters(configuration);

            // if the token is not valid, this will throw an exception
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // Additional check: ensure token algorithm is expected
            // Otherwise, malicious people can set algorithm to none and remove the signature
            // part from the token, which might allow them to bypass validation
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(DefaultSecurityAlgorithm, StringComparison.InvariantCultureIgnoreCase))
                return null;

            // Check custom "purpose" claim equals "email_verification"
            var purposeClaim = principal.FindFirst("purpose")?.Value;
            if (purposeClaim != "email_verification")
                return null;

            return principal;
        }
        catch (Exception)
        {
            // Token invalid or expired
            return null;
        }
    }
    
    public string GeneratePasswordResetToken(long userId, string email, int validForMinutes = 15)
    {
        // 1. Get the secret key from configuration and convert to bytes
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);

        // 2. Get the issuer from configuration (usually your API or domain name)
        var issuer = configuration["Jwt:Issuer"];

        // 3. Create token descriptor - defines what's inside the token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // a) ClaimsIdentity: defines the user identity claims inside the token
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()), // Put the username as a claim
                new Claim(ClaimTypes.Email, email), // Include the user's email as a claim
                new Claim("purpose", "forgot_password"), // Custom claim to indicate this is for password reset
            }),

            // b) Expiration date/time for the token validity
            Expires = DateTime.UtcNow.AddMinutes(validForMinutes),

            // c) Issuer of the token (who issues it)
            Issuer = issuer,

            // d) Audience - who the token is intended for (usually same as issuer)
            Audience = issuer,

            // e) Signing credentials (how the token is signed and verified)
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), DefaultSecurityAlgorithmSignature)
        };

        // 4. Create a token handler to generate the JWT
        var tokenHandler = new JwtSecurityTokenHandler();

        // 5. Create the token based on the descriptor above
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // 6. Serialize the token to a string to send it to the client
        return tokenHandler.WriteToken(token);
    }
    
    public ClaimsPrincipal? ValidatePasswordResetToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return null;
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetDefaultTokenValidationParameters(configuration);

            // if the token is not valid, this will throw an exception
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // Additional check: ensure token algorithm is expected
            // Otherwise, malicious people can set algorithm to none and remove the signature
            // part from the token, which might allow them to bypass validation
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(DefaultSecurityAlgorithm, StringComparison.InvariantCultureIgnoreCase))
                return null;

            // Check custom "purpose" claim equals "forgot_password"
            var purposeClaim = principal.FindFirst("purpose")?.Value;
            if (purposeClaim != "forgot_password")
                return null;

            return principal;
        }
        catch (Exception)
        {
            // Token invalid or expired
            return null;
        }
    }
}