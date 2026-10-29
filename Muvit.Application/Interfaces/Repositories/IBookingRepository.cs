using Muvit.Domain.Entities;

namespace Muvit.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Booking>> GetByCustomerIdAsync(Guid customerId);
    Task<IReadOnlyList<Booking>> GetByDriverIdAsync(Guid driverId);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
}