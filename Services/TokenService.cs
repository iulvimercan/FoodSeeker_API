using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FoodSeekerAPI.Services;

public class TokenService(IConfiguration configuration)
{
    public string GenerateToken(long userId, bool isDonator)
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
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),  // Put the username as a claim
                new Claim(ClaimTypes.Role, isDonator ? "Donator" : "FoodSeeker")  // Role claim based on user type
            }),

            // b) Expiration date/time for the token validity
            Expires = DateTime.UtcNow.AddMonths(1),

            // c) Issuer of the token (who issues it)
            Issuer = issuer,

            // d) Audience - who the token is intended for (usually same as issuer)
            Audience = issuer,

            // e) Signing credentials (how the token is signed and verified)
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),    // Use the secret key wrapped in a SymmetricSecurityKey
                SecurityAlgorithms.HmacSha256Signature  // Use HMAC SHA-256 as the signing algorithm
            )
        };

        // 4. Create a token handler to generate the JWT
        var tokenHandler = new JwtSecurityTokenHandler();

        // 5. Create the token based on the descriptor above
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // 6. Serialize the token to a string to send it to the client
        return tokenHandler.WriteToken(token);
    }
}