using Moq;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Application.Services;
using Muvit.Domain.Entities;
using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;
using Xunit;

namespace Muvit.Application.Tests.Services;

public class NegotiationServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly NegotiationService _sut;

    public NegotiationServiceTests()
    {
        _sut = new NegotiationService(_bookingRepository.Object);
    }

    [Fact]
    public async Task SubmitOfferAsync_ExistingBooking_SubmitsOfferAndSaves()
    {
        var booking = CreateRequestedBooking(out var customerId);
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        var offer = await _sut.SubmitOfferAsync(booking.Id, customerId, new Money(500m, "ZAR"), customerId);

        Assert.Equal(OfferType.Initial, offer.Type);
        _bookingRepository.Verify(r => r.UpdateAsync(booking), Times.Once);
    }

    [Fact]
    public async Task SubmitOfferAsync_BookingNotFound_ThrowsAndNeverSaves()
    {
        var bookingId = Guid.NewGuid();
        _bookingRepository.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync((Booking?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SubmitOfferAsync(bookingId, Guid.NewGuid(), new Money(500m, "ZAR"), Guid.NewGuid()));

        _bookingRepository.Verify(r => r.UpdateAsync(It.IsAny<Booking>()), Times.Never);
    }

    private static Booking CreateRequestedBooking(out Guid customerId)
    {
        customerId = Guid.NewGuid();
        var pickup = CreateAddress(-26.2041m, 28.0473m);
        var dropoff = CreateAddress(-25.7479m, 28.2293m);
        return Booking.RequestMove(customerId, pickup, dropoff, 58.3);
    }

    private static Address CreateAddress(decimal lat, decimal lon) =>
        new("123 Main Street", "Johannesburg", "South Africa",
            stateProvince: "Gauteng", postalCode: "2000", coordinate: new GeoCoordinate(lat, lon));
}