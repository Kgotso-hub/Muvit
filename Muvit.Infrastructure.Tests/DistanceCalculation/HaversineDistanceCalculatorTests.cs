using Muvit.Domain.ValueObjects;
using Muvit.Infrastructure.DistanceCalculation;
using Xunit;

namespace Muvit.Infrastructure.Tests.DistanceCalculation;

public class HaversineDistanceCalculatorTests
{
    [Fact]
    public async Task GetDistanceKmAsync_ReturnsSameValueAsGeoCoordinateDistanceTo()
    {
        var origin = new GeoCoordinate(-26.2041m, 28.0473m); // Johannesburg
        var destination = new GeoCoordinate(-33.9249m, 18.4241m); // Cape Town
        var calculator = new HaversineDistanceCalculator();

        var result = await calculator.GetDistanceKmAsync(origin, destination);

        Assert.Equal(origin.DistanceTo(destination), result);
    }

    [Fact]
    public async Task GetDistanceKmAsync_SameCoordinate_ReturnsZero()
    {
        var point = new GeoCoordinate(-26.2041m, 28.0473m);
        var calculator = new HaversineDistanceCalculator();

        var result = await calculator.GetDistanceKmAsync(point, point);

        Assert.Equal(0, result, precision: 10);
    }
}