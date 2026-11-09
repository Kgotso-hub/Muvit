using Microsoft.Extensions.Configuration;
using Muvit.Application.Interfaces.Services;
using Muvit.Domain.ValueObjects;

namespace Muvit.Infrastructure.CommissionStrategies;

public sealed class FlatRateCommissionStrategy : ICommissionStrategy
{
    private readonly decimal _ratePercentage;

    public FlatRateCommissionStrategy(IConfiguration configuration)
    {
        var raw = configuration["Commission:FlatRatePercentage"]
            ?? throw new InvalidOperationException("Commission:FlatRatePercentage must be configured.");

        _ratePercentage = decimal.Parse(raw);
    }

    // grossAmount is unused here by design — a flat rate doesn't vary by booking size.
    // TieredCommissionStrategy is where grossAmount actually matters.
    public decimal GetCommissionRate(Money grossAmount) => _ratePercentage;
}