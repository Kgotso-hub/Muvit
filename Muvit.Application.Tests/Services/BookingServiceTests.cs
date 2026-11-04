using Moq;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Application.Interfaces.Services;
using Muvit.Application.Services;
using Muvit.Domain.Entities;
using Muvit.Domain.Enums;
using Muvit.Domain.ValueObjects;
using Xunit;

namespace Muvit.Application.Tests.Services;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly Mock<IDistanceCalculator> _distanceCalculator = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly BookingService _sut; //"system under test”

    public BookingServiceTests()
    {
        _sut = new BookingService(_bookingRepository.Object, _distanceCalculator.Object, _notificationService.Object);
    }

    [Fact]
    public async Task RequestMoveAsync_ValidAddresses_CalculatesDistanceAndSavesBooking()
    {
        var customerId = Guid.NewGuid();
        var pickup = CreateAddress(-26.2041m, 28.0473m);
        var dropoff = CreateAddress(-25.7479m, 28.2293m);

        _distanceCalculator
            .Setup(d => d.GetDistanceKmAsync(pickup.Coordinate!, dropoff.Coordinate!))
            .ReturnsAsync(58.3);

        var booking = await _sut.RequestMoveAsync(customerId, pickup, dropoff);

        Assert.Equal(58.3, booking.DistanceKm);
        Assert.Equal(customerId, booking.CustomerId);
        _bookingRepository.Verify(r => r.AddAsync(booking), Times.Once);
        _notificationService.Verify(n => n.NotifyAsync(customerId, "Booking Confirmed", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RequestMoveAsync_PickupMissingCoordinate_ThrowsAndNeverCallsDistanceCalculator()
    {
        var customerId = Guid.NewGuid();
        var pickup = CreateAddress(coordinate: null);
        var dropoff = CreateAddress(-25.7479m, 28.2293m);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.RequestMoveAsync(customerId, pickup, dropoff));

        _distanceCalculator.Verify(d => d.GetDistanceKmAsync(It.IsAny<GeoCoordinate>(), It.IsAny<GeoCoordinate>()), Times.Never);
        _bookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
        _notificationService.Verify(n => n.NotifyAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateJobStatusAsync_ValidTransition_UpdatesStatusAndNotifiesCustomer()
    {
        var customerId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var pickup = CreateAddress(-26.2041m, 28.0473m);
        var dropoff = CreateAddress(-25.7479m, 28.2293m);
        var booking = Booking.RequestMove(customerId, pickup, dropoff, 58.3);
        booking.AssignDriver(driverId, new Money(500m, "ZAR"), driverId);

        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id)).ReturnsAsync(booking);

        await _sut.UpdateJobStatusAsync(booking.Id, BookingStatus.EnRoute, driverId);

        Assert.Equal(BookingStatus.EnRoute, booking.Status);
        _bookingRepository.Verify(r => r.UpdateAsync(booking), Times.Once);
        _notificationService.Verify(n => n.NotifyAsync(customerId, "Booking Status Updated", It.IsAny<string>()), Times.Once);
    }

    private static Address CreateAddress(decimal lat, decimal lon) =>
        new("123 Main Street", "Johannesburg", "South Africa",
            stateProvince: "Gauteng", postalCode: "2000", coordinate: new GeoCoordinate(lat, lon));

    private static Address CreateAddress(GeoCoordinate? coordinate) =>
        new("123 Main Street", "Johannesburg", "South Africa",
            stateProvince: "Gauteng", postalCode: "2000", coordinate: coordinate);
}