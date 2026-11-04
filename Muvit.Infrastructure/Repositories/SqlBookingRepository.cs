using Microsoft.EntityFrameworkCore;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Domain.Entities;
using Muvit.Infrastructure.Data;

namespace Muvit.Infrastructure.Repositories;

public class SqlBookingRepository : IBookingRepository
{
    private readonly MuvitDbContext _context;

    public SqlBookingRepository(MuvitDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(Guid id)
        => await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IReadOnlyList<Booking>> GetByCustomerIdAsync(Guid customerId)
        => await _context.Bookings.Where(b => b.CustomerId == customerId).ToListAsync();

    public async Task<IReadOnlyList<Booking>> GetByDriverIdAsync(Guid driverId)
        => await _context.Bookings.Where(b => b.DriverId == driverId).ToListAsync();

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }
}