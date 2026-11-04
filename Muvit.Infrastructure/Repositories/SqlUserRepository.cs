using Microsoft.EntityFrameworkCore;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Domain.Entities;
using Muvit.Infrastructure.Data;

namespace Muvit.Infrastructure.Repositories;

public class SqlUserRepository : IUserRepository
{
    private readonly MuvitDbContext _context;

    public SqlUserRepository(MuvitDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}