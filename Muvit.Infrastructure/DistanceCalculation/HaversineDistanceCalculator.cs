using Muvit.Application.Interfaces.Services;
using Muvit.Domain.ValueObjects;

namespace Muvit.Infrastructure.DistanceCalculation;

public sealed class HaversineDistanceCalculator : IDistanceCalculator
{
    public Task<double> GetDistanceKmAsync(GeoCoordinate origin, GeoCoordinate destination)
        => Task.FromResult(origin.DistanceTo(destination));
}