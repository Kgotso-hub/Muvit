using System.Linq;
using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;

namespace Muvit.Domain.Entities;

public sealed class Booking
{
    private readonly List<BookingStatusEvent> _statusHistory = new();
    private readonly List<PriceOffer> _priceOffers = new();

    // Maximum number of offer/counter-offer rounds allowed per booking negotiation.
    private const int MaxNegotiationRounds = 5;

    // Every legal next-status for each current status. Anything not listed here is refused —
    // this is what makes an illegal jump like Completed -> Requested impossible.
    private static readonly Dictionary<BookingStatus, BookingStatus[]> AllowedTransitions = new()
    {
        [BookingStatus.Requested] = new[] { BookingStatus.Negotiating, BookingStatus.Accepted, BookingStatus.Cancelled },
        [BookingStatus.Negotiating] = new[] { BookingStatus.Accepted, BookingStatus.Cancelled },
        [BookingStatus.Accepted] = new[] { BookingStatus.EnRoute, BookingStatus.Cancelled },
        [BookingStatus.EnRoute] = new[] { BookingStatus.InProgress, BookingStatus.Cancelled },
        [BookingStatus.InProgress] = new[] { BookingStatus.Completed, BookingStatus.Disputed },
        [BookingStatus.Completed] = new[] { BookingStatus.Disputed },
        [BookingStatus.Cancelled] = Array.Empty<BookingStatus>(),
        [BookingStatus.Disputed] = new[] { BookingStatus.Completed, BookingStatus.Cancelled },
    };

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public Guid? DriverId { get; private set; }
    public Address PickupAddress { get; }
    public Address DropoffAddress { get; }
    public double DistanceKm { get; }
    public Money? AgreedPrice { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime RequestedAt { get; }
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public IReadOnlyList<BookingStatusEvent> StatusHistory => _statusHistory.AsReadOnly();
    public IReadOnlyList<PriceOffer> PriceOffers => _priceOffers.AsReadOnly();

    private Booking(Guid customerId, Address pickup, Address dropoff, double distanceKm)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        PickupAddress = pickup;
        DropoffAddress = dropoff;
        DistanceKm = distanceKm;
        Status = BookingStatus.Requested;
        RequestedAt = DateTime.UtcNow;
    }

    public static Booking RequestMove(Guid customerId, Address pickup, Address dropoff, double distanceKm)
    {
        if (pickup is null) throw new ArgumentNullException(nameof(pickup));
        if (dropoff is null) throw new ArgumentNullException(nameof(dropoff));
        if (distanceKm < 0) throw new ArgumentOutOfRangeException(nameof(distanceKm), "Distance cannot be negative.");

        var booking = new Booking(customerId, pickup, dropoff, distanceKm);
        booking.RecordStatusEvent(BookingStatus.Requested, customerId, "Move requested.");
        return booking;
    }

    // Submits the first offer or a counter-offer. Any still-pending offer is superseded.
    public PriceOffer SubmitOffer(Guid offeredByUserId, Money amount, Guid changedByUserId)
    {
        if (Status != BookingStatus.Requested && Status != BookingStatus.Negotiating)
            throw new InvalidOperationException($"Cannot submit a price offer while the booking is {Status}.");

        if (_priceOffers.Count >= MaxNegotiationRounds)
            throw new InvalidOperationException($"This booking has reached the maximum of {MaxNegotiationRounds} negotiation rounds.");

        var previousPending = _priceOffers.LastOrDefault(o => o.Status == OfferStatus.Pending);
        previousPending?.MarkCountered();

        var type = _priceOffers.Count == 0 ? OfferType.Initial : OfferType.Counter;
        var offer = new PriceOffer(offeredByUserId, amount, type);
        _priceOffers.Add(offer);

        if (Status == BookingStatus.Requested)
        {
            Status = BookingStatus.Negotiating;
            RecordStatusEvent(BookingStatus.Negotiating, changedByUserId, "Negotiation started.");
        }

        return offer;
    }

    // Accepting an offer both closes the negotiation and assigns the driver in one step.
    public void AcceptOffer(Guid offerId, Guid driverId, Guid changedByUserId)
    {
        var offer = _priceOffers.FirstOrDefault(o => o.Id == offerId)
            ?? throw new InvalidOperationException("No such price offer on this booking.");

        if (offer.Status != OfferStatus.Pending)
            throw new InvalidOperationException($"Cannot accept a {offer.Status} offer — only a Pending offer can be accepted.");

        AssignDriver(driverId, offer.Amount, changedByUserId);
        offer.Accept();
    }

    public void RejectOffer(Guid offerId)
    {

        var offer = _priceOffers.FirstOrDefault(o => o.Id == offerId)
            ?? throw new InvalidOperationException("No such price offer on this booking.");

        offer.Reject();
    }

    public void AssignDriver(Guid driverId, Money agreedPrice, Guid changedByUserId)
    {
        EnsureTransitionAllowed(BookingStatus.Accepted);

        DriverId = driverId;
        AgreedPrice = agreedPrice ?? throw new ArgumentNullException(nameof(agreedPrice));
        AcceptedAt = DateTime.UtcNow;
        Status = BookingStatus.Accepted;

        RecordStatusEvent(BookingStatus.Accepted, changedByUserId, "Driver assigned and price agreed.");
    }

    public void UpdateStatus(BookingStatus newStatus, Guid changedByUserId, string? note = null)
    {
        EnsureTransitionAllowed(newStatus);

        Status = newStatus;
        if (newStatus == BookingStatus.Completed)
            CompletedAt = DateTime.UtcNow;

        RecordStatusEvent(newStatus, changedByUserId, note);
    }

    // Returns the fee to charge, or null if cancellation is free at this stage.
    public Money? Cancel(Guid changedByUserId, Money? cancellationFee, string? note = null)
    {
        EnsureTransitionAllowed(BookingStatus.Cancelled);

        var feeApplies = AcceptedAt is not null; // free while negotiating, fee once a driver is assigned

        Status = BookingStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        RecordStatusEvent(BookingStatus.Cancelled, changedByUserId, note ?? "Booking cancelled.");

        return feeApplies ? cancellationFee : null;
    }

    private void EnsureTransitionAllowed(BookingStatus newStatus)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidOperationException($"Cannot transition a booking from {Status} to {newStatus}.");
    }

    private void RecordStatusEvent(BookingStatus status, Guid changedByUserId, string? note)
    {
        _statusHistory.Add(new BookingStatusEvent(status, changedByUserId, note));
    }
}