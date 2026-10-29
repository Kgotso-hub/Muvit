using Muvit.Domain.Entities;

namespace Muvit.Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<Payment?> GetByBookingIdAsync(Guid bookingId);
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
}