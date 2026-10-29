using Muvit.Application.Interfaces.Repositories;
using Muvit.Domain.Entities;
using Muvit.Domain.ValueObjects;

namespace Muvit.Application.Services;

public sealed class NegotiationService
{
    private readonly IBookingRepository _bookingRepository;

    public NegotiationService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<PriceOffer> SubmitOfferAsync(Guid bookingId, Guid offeredByUserId, Money amount, Guid changedByUserId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new InvalidOperationException($"No booking found with id {bookingId}.");

        var offer = booking.SubmitOffer(offeredByUserId, amount, changedByUserId);

        await _bookingRepository.UpdateAsync(booking);

        return offer;
    }

    public async Task AcceptOfferAsync(Guid bookingId, Guid offerId, Guid driverId, Guid changedByUserId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new InvalidOperationException($"No booking found with id {bookingId}.");

        booking.AcceptOffer(offerId, driverId, changedByUserId);

        await _bookingRepository.UpdateAsync(booking);
    }

    public async Task RejectOfferAsync(Guid bookingId, Guid offerId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new InvalidOperationException($"No booking found with id {bookingId}.");

        booking.RejectOffer(offerId);

        await _bookingRepository.UpdateAsync(booking);
    }
}