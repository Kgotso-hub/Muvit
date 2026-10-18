using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;

namespace Muvit.Domain.Entities;

public sealed class PriceOffer
{
    public Guid Id { get; }
    public Guid OfferedByUserId { get; }
    public Money Amount { get; }
    public OfferType Type { get; }
    public OfferStatus Status { get; private set; }
    public DateTime CreatedAt { get; }

    internal PriceOffer(Guid offeredByUserId, Money amount, OfferType type)
    {
        Id = Guid.NewGuid();
        OfferedByUserId = offeredByUserId;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
        Type = type;
        Status = OfferStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    internal void Accept()
    {
        EnsurePending();
        Status = OfferStatus.Accepted;
    }

    internal void Reject()
    {
        EnsurePending();
        Status = OfferStatus.Rejected;
    }

    internal void MarkCountered()
    {
        EnsurePending();
        Status = OfferStatus.Countered;
    }

    private void EnsurePending()
    {
        if (Status != OfferStatus.Pending)
            throw new InvalidOperationException($"Cannot change a {Status} offer — only a Pending offer can be accepted, rejected, or countered.");
    }
}