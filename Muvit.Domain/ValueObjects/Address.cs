namespace Muvit.Domain.ValueObjects;

public sealed record Address
{
    public string Line1 { get; }
    public string? Line2 { get; }
    public string City { get; }
    public string? StateProvince { get; }
    public string? PostalCode { get; }
    public string Country { get; }
    public GeoCoordinate? Coordinate { get; }

    public Address(
        string line1,
        string city,
        string country,
        string? line2 = null,
        string? stateProvince = null,
        string? postalCode = null,
        GeoCoordinate? coordinate = null)
    {
        if (string.IsNullOrWhiteSpace(line1))
            throw new ArgumentException("Line1 is required.", nameof(line1));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required.", nameof(country));

        Line1 = line1;
        Line2 = line2;
        City = city;
        StateProvince = stateProvince;
        PostalCode = postalCode;
        Country = country;
        Coordinate = coordinate;
    }
}