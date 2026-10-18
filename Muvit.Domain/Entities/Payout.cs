using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;

namespace Muvit.Domain.Entities;

public sealed class Payout
{
    public Guid Id { get; }
    public Guid PaymentId { get; }
    public Guid BookingId { get; }
    public Guid DriverId { get; }
    public Money GrossAmount { get; }
    public decimal CommissionRate { get; }
    public Money CommissionAmount { get; }
    public Money NetPayoutAmount { get; }
    public PayoutStatus Status { get; private set; }
    public Guid? ProcessedByAdminId { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime CreatedAt { get; }

    private Payout(Guid paymentId, Guid bookingId, Guid driverId, Money grossAmount, decimal commissionRate)
    {
        if (commissionRate < 0 || commissionRate > 100)
            throw new ArgumentOutOfRangeException(nameof(commissionRate), "Commission rate must be a percentage between 0 and 100.");

        Id = Guid.NewGuid();
        PaymentId = paymentId;
        BookingId = bookingId;
        DriverId = driverId;
        GrossAmount = grossAmount ?? throw new ArgumentNullException(nameof(grossAmount));
        CommissionRate = commissionRate;
        CommissionAmount = grossAmount.Multiply(commissionRate / 100m);
        NetPayoutAmount = grossAmount.Subtract(CommissionAmount);
        Status = PayoutStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static Payout Create(Guid paymentId, Guid bookingId, Guid driverId, Money grossAmount, decimal commissionRate)
        => new(paymentId, bookingId, driverId, grossAmount, commissionRate);

    public void MarkProcessed(Guid processedByAdminId)
    {
        EnsurePending();
        ProcessedByAdminId = processedByAdminId;
        Status = PayoutStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        EnsurePending();
        Status = PayoutStatus.Failed;
    }

    private void EnsurePending()
    {
        if (Status != PayoutStatus.Pending)
            throw new InvalidOperationException($"Cannot change a {Status} payout — it must be Pending first.");
    }
}