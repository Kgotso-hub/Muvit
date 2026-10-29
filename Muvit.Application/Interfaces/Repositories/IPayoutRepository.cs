using Muvit.Domain.Entities;
using Muvit.Domain.Enums;

namespace Muvit.Application.Interfaces.Repositories;

public interface IPayoutRepository
{
    Task<Payout?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Payout>> GetByDriverIdAsync(Guid driverId);
    Task<IReadOnlyList<Payout>> GetByStatusAsync(PayoutStatus status);
    Task AddAsync(Payout payout);
    Task UpdateAsync(Payout payout);
}