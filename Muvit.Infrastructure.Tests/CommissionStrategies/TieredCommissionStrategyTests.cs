using Muvit.Domain.ValueObjects;
using Muvit.Infrastructure.CommissionStrategies;
using Xunit;

namespace Muvit.Infrastructure.Tests.CommissionStrategies;

public class TieredCommissionStrategyTests
{
    private readonly TieredCommissionStrategy _strategy = new();

    [Fact]
    public void GetCommissionRate_SmallBooking_ReturnsHighestTier()
    {
        Assert.Equal(20m, _strategy.GetCommissionRate(new Money(100m, "ZAR")));
    }

    [Fact]
    public void GetCommissionRate_MidBooking_ReturnsMiddleTier()
    {
        Assert.Equal(15m, _strategy.GetCommissionRate(new Money(1000m, "ZAR")));
    }

    [Fact]
    public void GetCommissionRate_LargeBooking_ReturnsLowestTier()
    {
        Assert.Equal(10m, _strategy.GetCommissionRate(new Money(3000m, "ZAR")));
    }

    [Fact]
    public void GetCommissionRate_ExactlyAtTierBoundary_UsesInclusiveUpperBound()
    {
        Assert.Equal(15m, _strategy.GetCommissionRate(new Money(2000m, "ZAR")));
    }
}