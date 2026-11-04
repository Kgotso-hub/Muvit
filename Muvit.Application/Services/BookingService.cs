using Muvit.Application.Interfaces.Repositories;
using Muvit.Application.Interfaces.Services;
using Muvit.Domain.Entities;
using Muvit.Domain.ValueObjects;
using Muvit.Domain.Enums;

namespace Muvit.Application.Services;

public sealed class BookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IDistanceCalculator _distanceCalculator;
    private readonly INotificationService _notificationService;

    public BookingService(IBookingRepository bookingRepository, IDistanceCalculator distanceCalculator, INotificationService notificationService)
    {
        _bookingRepository = bookingRepository;
        _distanceCalculator = distanceCalculator;
        _notificationService = notificationService;
    }

    public async Task<Booking> RequestMoveAsync(Guid customerId, Address pickup, Address dropoff)
    {
        var pickupCoordinate = pickup.Coordinate
            ?? throw new ArgumentException("Pickup address must have a coordinate to request a move.", nameof(pickup));
        var dropoffCoordinate = dropoff.Coordinate
            ?? throw new ArgumentException("Dropoff address must have a coordinate to request a move.", nameof(dropoff));

        var distanceKm = await _distanceCalculator.GetDistanceKmAsync(pickupCoordinate, dropoffCoordinate);

        var booking = Booking.RequestMove(customerId, pickup, dropoff, distanceKm);
        await _bookingRepository.AddAsync(booking);

        await _notificationService.NotifyAsync(booking.CustomerId, "Booking Confirmed",
            $"Your move request has been received. Estimated distance: {distanceKm:F1} km.");

        return booking;
    }

    public async Task AssignDriverAsync(Guid bookingId, Guid driverId, Money agreedPrice, Guid changedByUserId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new InvalidOperationException($"No booking found with id {bookingId}.");

        booking.AssignDriver(driverId, agreedPrice, changedByUserId);

        await _bookingRepository.UpdateAsync(booking);
    }

    public async Task UpdateJobStatusAsync(Guid bookingId, BookingStatus newStatus, Guid changedByUserId, string? note = null)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new InvalidOperationException($"No booking found with id {bookingId}.");

        booking.UpdateStatus(newStatus, changedByUserId, note);

        await _bookingRepository.UpdateAsync(booking);

        await _notificationService.NotifyAsync(booking.CustomerId, "Booking Status Updated",
            $"Your booking status has changed to {newStatus}.");
    }

    public async Task<Money?> CancelAsync(Guid bookingId, Guid changedByUserId, Money? cancellationFee, string? note = null)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new InvalidOperationException($"No booking found with id {bookingId}.");

        var fee = booking.Cancel(changedByUserId, cancellationFee, note);

        await _bookingRepository.UpdateAsync(booking);

        return fee;
    }
}