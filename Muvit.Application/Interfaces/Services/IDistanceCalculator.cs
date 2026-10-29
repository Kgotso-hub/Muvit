using Muvit.Domain.ValueObjects;

namespace Muvit.Application.Interfaces.Services;

public interface IDistanceCalculator
{
    Task<double> GetDistanceKmAsync(GeoCoordinate origin, GeoCoordinate destination);
}