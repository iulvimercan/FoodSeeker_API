using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FoodSeekerAPI.Services;
using Microsoft.Extensions.Configuration;

namespace FoodSeekerAPI.Tests;

public class TokenServiceTests
{
    private const string TestKey = "test-signing-key-that-is-long-enough-for-hmac-sha256";
    private const long UserId = 42;
    private const string Email = "jane.doe@example.com";

    private static IConfiguration CreateConfiguration(string key = TestKey) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = "FoodSeekerAPI.Tests"
            })
            .Build();

    private readonly TokenService _service = new(CreateConfiguration());

    private static ClaimsPrincipal ValidateAccessToken(string token) =>
        new JwtSecurityTokenHandler().ValidateToken(
            token, TokenService.GetDefaultTokenValidationParameters(CreateConfiguration()), out _);

    [Theory]
    [InlineData(true, "Donator")]
    [InlineData(false, "FoodSeeker")]
    public void GenerateApiAccessToken_ContainsUserIdAndRole(bool isDonator, string expectedRole)
    {
        var principal = ValidateAccessToken(_service.GenerateApiAccessToken(UserId, isDonator));

        Assert.Equal(UserId.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.True(principal.IsInRole(expectedRole));
    }

    [Fact]
    public void EmailVerificationToken_RoundTrips()
    {
        var token = _service.GenerateEmailVerificationToken(UserId, Email);

        var principal = _service.ValidateEmailVerificationToken(token);

        Assert.NotNull(principal);
        Assert.Equal(Email, principal.FindFirstValue(ClaimTypes.Email));
        Assert.Equal(UserId.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [Fact]
    public void PasswordResetToken_RoundTrips()
    {
        var token = _service.GeneratePasswordResetToken(UserId, Email);

        Assert.NotNull(_service.ValidatePasswordResetToken(token));
    }

    [Fact]
    public void EmailVerificationToken_IsRejectedForPasswordReset()
    {
        var token = _service.GenerateEmailVerificationToken(UserId, Email);

        Assert.Null(_service.ValidatePasswordResetToken(token));
    }

    [Fact]
    public void PasswordResetToken_IsRejectedForEmailVerification()
    {
        var token = _service.GeneratePasswordResetToken(UserId, Email);

        Assert.Null(_service.ValidateEmailVerificationToken(token));
    }

    [Fact]
    public void AccessToken_IsRejectedForEmailVerification()
    {
        var token = _service.GenerateApiAccessToken(UserId, isDonator: false);

        Assert.Null(_service.ValidateEmailVerificationToken(token));
    }

    [Fact]
    public void TokenSignedWithAnotherKey_IsRejected()
    {
        var otherService = new TokenService(CreateConfiguration("a-different-signing-key-that-is-also-long-enough"));
        var token = otherService.GenerateEmailVerificationToken(UserId, Email);

        Assert.Null(_service.ValidateEmailVerificationToken(token));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    public void InvalidInput_ReturnsNull(string? token)
    {
        Assert.Null(_service.ValidateEmailVerificationToken(token));
        Assert.Null(_service.ValidatePasswordResetToken(token));
    }
}
