using Microsoft.EntityFrameworkCore;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Domain.Entities;
using Muvit.Infrastructure.Data;

namespace Muvit.Infrastructure.Repositories;

public class SqlPaymentRepository : IPaymentRepository
{
    private readonly MuvitDbContext _context;

    public SqlPaymentRepository(MuvitDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
        => await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Payment?> GetByBookingIdAsync(Guid bookingId)
        => await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }
}