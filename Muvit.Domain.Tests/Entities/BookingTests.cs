using Muvit.Domain.Entities;
using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;
using Xunit;

namespace Muvit.Domain.Tests.Entities;

public class BookingTests
{
    [Fact]
    public void RequestMove_CreatesBookingInRequestedStatus()
    {
        var booking = CreateRequestedBooking(out var customerId, out _);

        Assert.Equal(BookingStatus.Requested, booking.Status);
        Assert.Equal(customerId, booking.CustomerId);
        Assert.Single(booking.StatusHistory);
        Assert.True(booking.DistanceKm > 0);
    }

    [Fact]
    public void SubmitOffer_FirstOffer_IsInitialAndMovesToNegotiating()
    {
        var booking = CreateRequestedBooking(out var customerId, out _);

        var offer = booking.SubmitOffer(customerId, new Money(500m, "ZAR"), customerId);

        Assert.Equal(OfferType.Initial, offer.Type);
        Assert.Equal(BookingStatus.Negotiating, booking.Status);
        Assert.Equal(2, booking.StatusHistory.Count); // Requested + Negotiating
    }

    [Fact]
    public void SubmitOffer_SecondOffer_IsCounterAndSupersedesFirst()
    {
        var booking = CreateRequestedBooking(out var customerId, out var driverId);

        var first = booking.SubmitOffer(customerId, new Money(500m, "ZAR"), customerId);
        var second = booking.SubmitOffer(driverId, new Money(600m, "ZAR"), driverId);

        Assert.Equal(OfferType.Counter, second.Type);
        Assert.Equal(OfferStatus.Countered, first.Status);
        Assert.Equal(OfferStatus.Pending, second.Status);
    }

    [Fact]
    public void AcceptOffer_SetsDriverAndAgreedPrice_AndMovesToAccepted()
    {
        var booking = CreateRequestedBooking(out var customerId, out var driverId);
        var offer = booking.SubmitOffer(customerId, new Money(500m, "ZAR"), customerId);

        booking.AcceptOffer(offer.Id, driverId, driverId);

        Assert.Equal(BookingStatus.Accepted, booking.Status);
        Assert.Equal(driverId, booking.DriverId);
        Assert.Equal(500m, booking.AgreedPrice!.Amount);
        Assert.Equal(OfferStatus.Accepted, offer.Status);
    }

    [Fact]
    public void AcceptOffer_AlreadyCountered_Throws()
    {
        var booking = CreateRequestedBooking(out var customerId, out var driverId);
        var first = booking.SubmitOffer(customerId, new Money(500m, "ZAR"), customerId);
        booking.SubmitOffer(driverId, new Money(600m, "ZAR"), driverId);

        Assert.Throws<InvalidOperationException>(() => booking.AcceptOffer(first.Id, driverId, driverId));
    }

    [Fact]
    public void RejectOffer_MarksOfferRejected()
    {
        var booking = CreateRequestedBooking(out var customerId, out _);
        var offer = booking.SubmitOffer(customerId, new Money(500m, "ZAR"), customerId);

        booking.RejectOffer(offer.Id);

        Assert.Equal(OfferStatus.Rejected, offer.Status);
    }

    [Fact]
    public void UpdateStatus_FollowsValidPath_ThroughToCompleted()
    {
        var booking = CreateAcceptedBooking(out var driverId);

        booking.UpdateStatus(BookingStatus.EnRoute, driverId);
        booking.UpdateStatus(BookingStatus.InProgress, driverId);
        booking.UpdateStatus(BookingStatus.Completed, driverId);

        Assert.Equal(BookingStatus.Completed, booking.Status);
        Assert.NotNull(booking.CompletedAt);
    }

    [Fact]
    public void UpdateStatus_IllegalJump_Throws()
    {
        var booking = CreateAcceptedBooking(out var driverId);
        booking.UpdateStatus(BookingStatus.EnRoute, driverId);
        booking.UpdateStatus(BookingStatus.InProgress, driverId);
        booking.UpdateStatus(BookingStatus.Completed, driverId);

        Assert.Throws<InvalidOperationException>(() => booking.UpdateStatus(BookingStatus.Requested, driverId));
    }

    [Fact]
    public void Cancel_BeforeDriverAssigned_IsFree()
    {
        var booking = CreateRequestedBooking(out var customerId, out _);

        var fee = booking.Cancel(customerId, new Money(50m, "ZAR"));

        Assert.Null(fee);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void Cancel_AfterDriverAssigned_ChargesFee()
    {
        var booking = CreateAcceptedBooking(out var driverId);

        var fee = booking.Cancel(driverId, new Money(50m, "ZAR"));

        Assert.NotNull(fee);
        Assert.Equal(50m, fee!.Amount);
    }

    private static Address CreateAddress(decimal lat, decimal lon) =>
        new("123 Main Street", "Johannesburg", "South Africa",
            stateProvince: "Gauteng", postalCode: "2000", coordinate: new GeoCoordinate(lat, lon));

    private static Booking CreateRequestedBooking(out Guid customerId, out Guid driverId)
    {
        customerId = Guid.NewGuid();
        driverId = Guid.NewGuid();
        var pickup = CreateAddress(-26.2041m, 28.0473m);
        var dropoff = CreateAddress(-25.7479m, 28.2293m);
        return Booking.RequestMove(customerId, pickup, dropoff);
    }

    private static Booking CreateAcceptedBooking(out Guid driverId)
    {
        var booking = CreateRequestedBooking(out var customerId, out driverId);
        var offer = booking.SubmitOffer(customerId, new Money(500m, "ZAR"), customerId);
        booking.AcceptOffer(offer.Id, driverId, driverId);
        return booking;
    }
}