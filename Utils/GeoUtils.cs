namespace FoodSeekerAPI.Utils;

// document this file
// This utility class provides methods for geographical calculations, such as calculating the distance between two points on the Earth's surface.
// It uses the Haversine formula to compute the distance based on latitude and longitude coordinates.

/// <summary>
/// Utility class for geographical calculations.
/// </summary>
public static class GeoUtils
{
    // Radius of the Earth in kilometers
    private const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// Checks whether the destination point is within the given distance from the origin point using the Haversine formula.
    /// </summary>
    /// <param name="userLat">Latitude of the user in degrees</param>
    /// <param name="userLng">Longitude of the user in degrees</param>
    /// <param name="destLat">Latitude of the destination in degrees</param>
    /// <param name="destLng">Longitude of the destination in degrees</param>
    /// <param name="distanceKm">Distance radius in kilometers</param>
    /// <returns>True if destination is within distanceKm, false otherwise</returns>
    public static bool IsWithinDistance(double userLat, double userLng, double destLat, double destLng, double distanceKm = 50)
    {
        double dLat = DegreesToRadians(destLat - userLat);
        double dLon = DegreesToRadians(destLng - userLng);

        double lat1Rad = DegreesToRadians(userLat);
        double lat2Rad = DegreesToRadians(destLat);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        double distance = EarthRadiusKm * c;

        return distance <= distanceKm;
    }
    
    /// <summary>
    /// Calculates the distance between two geographical points using the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of the first point.</param>
    /// <param name="lon1">Longitude of the first point.</param>
    /// <param name="lat2">Latitude of the second point.</param>
    /// <param name="lon2">Longitude of the second point.</param>
    /// <returns>Distance in kilometers between the two points.</returns>
    public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Radius of the Earth in kilometers
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c; // Distance in kilometers
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">Value in degrees.</param>
    /// <returns>Value in radians.</returns>
    private static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }
}