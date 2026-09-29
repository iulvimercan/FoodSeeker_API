using FoodSeekerAPI.Services;

namespace FoodSeekerAPI.Tests;

public class PasswordServiceTests
{
    private readonly PasswordService _service = new();

    [Fact]
    public void HashPassword_DoesNotReturnPlainText()
    {
        var hash = _service.HashPassword("s3cret-Pass");

        Assert.NotEqual("s3cret-Pass", hash);
        Assert.StartsWith("$2", hash); // BCrypt hash prefix
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ProducesDifferentHashes()
    {
        Assert.NotEqual(_service.HashPassword("s3cret-Pass"), _service.HashPassword("s3cret-Pass"));
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var hash = _service.HashPassword("s3cret-Pass");

        Assert.True(_service.VerifyPassword("s3cret-Pass", hash));
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _service.HashPassword("s3cret-Pass");

        Assert.False(_service.VerifyPassword("s3cret-pass", hash));
    }
}
