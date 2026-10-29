using Muvit.Application.Interfaces.Repositories;
using Muvit.Application.Interfaces.Services;
using Muvit.Domain.Entities;
using Muvit.Domain.ValueObjects;

namespace Muvit.Application.Services;

public sealed class PayoutService
{
    private readonly IPayoutRepository _payoutRepository;
    private readonly ICommissionStrategy _commissionStrategy;

    public PayoutService(IPayoutRepository payoutRepository, ICommissionStrategy commissionStrategy)
    {
        _payoutRepository = payoutRepository;
        _commissionStrategy = commissionStrategy;
    }

    public async Task<Payout> CalculatePayoutAsync(Guid paymentId, Guid bookingId, Guid driverId, Money grossAmount)
    {
        var commissionRate = _commissionStrategy.GetCommissionRate(grossAmount);

        var payout = Payout.Create(paymentId, bookingId, driverId, grossAmount, commissionRate);

        await _payoutRepository.AddAsync(payout);

        return payout;
    }

    public async Task<Payout> ProcessPayoutAsync(Guid payoutId, Guid processedByAdminId)
    {
        var payout = await _payoutRepository.GetByIdAsync(payoutId)
            ?? throw new InvalidOperationException($"No payout found with id {payoutId}.");

        payout.MarkProcessed(processedByAdminId);

        await _payoutRepository.UpdateAsync(payout);

        return payout;
    }
}