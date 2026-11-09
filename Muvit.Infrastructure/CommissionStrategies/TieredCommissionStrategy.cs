using Muvit.Application.Interfaces.Services;
using Muvit.Domain.ValueObjects;

namespace Muvit.Infrastructure.CommissionStrategies;

// Illustrative placeholder tiers — NOT the active commission model (M0 decided flat rate; see
// FlatRateCommissionStrategy, which is what's actually registered in DI). This class exists to
// prove ICommissionStrategy genuinely supports more than one implementation — a real Strategy
// pattern, not just an interface with one option — without any change to PayoutService. Revisit
// these specific thresholds/rates with real business numbers before ever activating this strategy.
public sealed class TieredCommissionStrategy : ICommissionStrategy
{
    public decimal GetCommissionRate(Money grossAmount) => grossAmount.Amount switch
    {
        < 500m => 20m,
        <= 2000m => 15m,
        _ => 10m
    };

    ///// Simple version of GetCommissionRate above, for reference:
    //
    // public decimal GetCommissionRate(Money grossAmount)
    // {
    //     if (grossAmount.Amount < 500m)
    //     {
    //         return 20m;
    //     }
    //     else if (grossAmount.Amount <= 2000m)
    //     {
    //         return 15m;
    //     }
    //     else
    //     {
    //         return 10m;
    //     }
    // }
}