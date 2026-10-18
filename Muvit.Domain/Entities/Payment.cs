using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;

namespace Muvit.Domain.Entities;

public sealed class Payment
{
    public Guid Id { get; }
    public Guid BookingId { get; }
    public Guid CustomerId { get; }
    public Money Amount { get; }
    public string? GatewayName { get; }
    public string? GatewayReference { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime CreatedAt { get; }

    private Payment(Guid bookingId, Guid customerId, Money amount, string? gatewayName)
    {
        Id = Guid.NewGuid();
        BookingId = bookingId;
        CustomerId = customerId;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
        GatewayName = gatewayName;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static Payment Initiate(Guid bookingId, Guid customerId, Money amount, string? gatewayName = null)
        => new(bookingId, customerId, amount, gatewayName);

    public void MarkCompleted(string gatewayReference)
    {
        EnsureStatus(PaymentStatus.Pending, "completed");

        if (string.IsNullOrWhiteSpace(gatewayReference))
            throw new ArgumentException("Gateway reference is required to mark a payment completed.", nameof(gatewayReference));

        GatewayReference = gatewayReference;
        Status = PaymentStatus.Completed;
        PaidAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        EnsureStatus(PaymentStatus.Pending, "failed");
        Status = PaymentStatus.Failed;
    }

    public void Refund()
    {
        EnsureStatus(PaymentStatus.Completed, "refunded");
        Status = PaymentStatus.Refunded;
    }

    private void EnsureStatus(PaymentStatus required, string action)
    {
        if (Status != required)
            throw new InvalidOperationException($"Cannot mark a {Status} payment as {action} — it must be {required} first.");
    }
}