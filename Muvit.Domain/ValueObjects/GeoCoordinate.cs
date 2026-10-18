namespace Muvit.Domain.ValueObjects;

public sealed record GeoCoordinate
{
    public decimal Latitude { get; }
    public decimal Longitude { get; }

    public GeoCoordinate(decimal latitude, decimal longitude)
    {
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");

        Latitude = latitude;
        Longitude = longitude;
    }

    /// Distance in kilometers, using the Haversine formula (accounts for Earth's curvature).
    public double DistanceTo(GeoCoordinate other)
    {
        const double earthRadiusKm = 6371.0;

        var lat1 = (double)Latitude * Math.PI / 180.0;
        var lat2 = (double)other.Latitude * Math.PI / 180.0;
        var deltaLat = ((double)other.Latitude - (double)Latitude) * Math.PI / 180.0;
        var deltaLon = ((double)other.Longitude - (double)Longitude) * Math.PI / 180.0;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }
}