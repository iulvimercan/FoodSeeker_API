namespace FoodSeekerAPI.Utils
{
    /// <summary>
    /// Provides utility methods for geographical calculations,
    /// primarily for calculating distances and proximity between
    /// two latitude/longitude points on the Earth's surface.
    /// </summary>
    public static class GeoUtils
    {
        // Mean radius of the Earth in kilometers
        private const double EarthRadiusKm = 6371.0;

        /// <summary>
        /// Determines whether a destination point is within a specified distance radius 
        /// from an origin point using the Haversine formula.
        /// </summary>
        /// <param name="userLat">Latitude of the origin point in decimal degrees.</param>
        /// <param name="userLng">Longitude of the origin point in decimal degrees.</param>
        /// <param name="destLat">Latitude of the destination point in decimal degrees.</param>
        /// <param name="destLng">Longitude of the destination point in decimal degrees.</param>
        /// <param name="distanceKm">Maximum allowed distance in kilometers (default is 50 km).</param>
        /// <returns>
        /// True if the destination is within the specified distance radius from the origin;
        /// otherwise, false.
        /// </returns>
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
        /// Calculates the great-circle distance between two geographical points using 
        /// the Haversine formula.
        /// </summary>
        /// <param name="lat1">Latitude of the first point in decimal degrees.</param>
        /// <param name="lon1">Longitude of the first point in decimal degrees.</param>
        /// <param name="lat2">Latitude of the second point in decimal degrees.</param>
        /// <param name="lon2">Longitude of the second point in decimal degrees.</param>
        /// <returns>Distance in kilometers between the two points.</returns>
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        /// <summary>
        /// Converts an angle from degrees to radians.
        /// </summary>
        /// <param name="degrees">Angle in degrees.</param>
        /// <returns>Angle converted to radians.</returns>
        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}
