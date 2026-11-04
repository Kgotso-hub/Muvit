using Microsoft.EntityFrameworkCore;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Domain.Entities;
using Muvit.Domain.Enums;
using Muvit.Infrastructure.Data;

namespace Muvit.Infrastructure.Repositories;

public class SqlPayoutRepository : IPayoutRepository
{
    private readonly MuvitDbContext _context;

    public SqlPayoutRepository(MuvitDbContext context)
    {
        _context = context;
    }

    public async Task<Payout?> GetByIdAsync(Guid id)
        => await _context.Payouts.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IReadOnlyList<Payout>> GetByDriverIdAsync(Guid driverId)
        => await _context.Payouts.Where(p => p.DriverId == driverId).ToListAsync();

    public async Task<IReadOnlyList<Payout>> GetByStatusAsync(PayoutStatus status)
        => await _context.Payouts.Where(p => p.Status == status).ToListAsync();

    public async Task AddAsync(Payout payout)
    {
        await _context.Payouts.AddAsync(payout);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payout payout)
    {
        _context.Payouts.Update(payout);
        await _context.SaveChangesAsync();
    }
}