using FoodSeekerAPI.Utils;

namespace FoodSeekerAPI.Tests;

public class GeoUtilsTests
{
    private const double IstanbulLat = 41.0082, IstanbulLng = 28.9784;
    private const double AnkaraLat = 39.9334, AnkaraLng = 32.8597;

    [Fact]
    public void CalculateDistance_SamePoint_IsZero()
    {
        var distance = GeoUtils.CalculateDistance(IstanbulLat, IstanbulLng, IstanbulLat, IstanbulLng);

        Assert.Equal(0, distance, precision: 6);
    }

    [Fact]
    public void CalculateDistance_IstanbulToAnkara_IsAbout350Km()
    {
        var distance = GeoUtils.CalculateDistance(IstanbulLat, IstanbulLng, AnkaraLat, AnkaraLng);

        Assert.InRange(distance, 345, 355);
    }

    [Fact]
    public void CalculateDistance_IsSymmetric()
    {
        var there = GeoUtils.CalculateDistance(IstanbulLat, IstanbulLng, AnkaraLat, AnkaraLng);
        var back = GeoUtils.CalculateDistance(AnkaraLat, AnkaraLng, IstanbulLat, IstanbulLng);

        Assert.Equal(there, back, precision: 9);
    }

    [Fact]
    public void CalculateDistance_OneDegreeOfLatitude_IsAbout111Km()
    {
        var distance = GeoUtils.CalculateDistance(41, 29, 42, 29);

        Assert.InRange(distance, 111.1, 111.3);
    }

    [Theory]
    [InlineData(112, true)]  // one degree of latitude is ~111.19 km
    [InlineData(111, false)]
    public void IsWithinDistance_RespectsRadius(double radiusKm, bool expected)
    {
        Assert.Equal(expected, GeoUtils.IsWithinDistance(41, 29, 42, 29, radiusKm));
    }

    [Fact]
    public void IsWithinDistance_DefaultRadiusIs50Km()
    {
        Assert.True(GeoUtils.IsWithinDistance(41, 29, 41.4, 29));   // ~44 km
        Assert.False(GeoUtils.IsWithinDistance(41, 29, 41.5, 29));  // ~56 km
    }
}
