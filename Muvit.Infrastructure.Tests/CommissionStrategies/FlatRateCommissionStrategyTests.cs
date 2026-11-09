using Moq;
using Microsoft.Extensions.Configuration;
using Muvit.Domain.ValueObjects;
using Muvit.Infrastructure.CommissionStrategies;
using Xunit;

namespace Muvit.Infrastructure.Tests.CommissionStrategies;

public class FlatRateCommissionStrategyTests
{
    [Fact]
    public void GetCommissionRate_ReturnsConfiguredRate_RegardlessOfAmount()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["Commission:FlatRatePercentage"]).Returns("15");
        var strategy = new FlatRateCommissionStrategy(configuration.Object);

        var smallBookingRate = strategy.GetCommissionRate(new Money(50m, "ZAR"));
        var largeBookingRate = strategy.GetCommissionRate(new Money(5000m, "ZAR"));

        Assert.Equal(15m, smallBookingRate);
        Assert.Equal(15m, largeBookingRate);
    }

    [Fact]
    public void Constructor_MissingConfiguredRate_Throws()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["Commission:FlatRatePercentage"]).Returns((string?)null);

        Assert.Throws<InvalidOperationException>(() => new FlatRateCommissionStrategy(configuration.Object));
    }
}