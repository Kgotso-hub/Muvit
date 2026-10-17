using Microsoft.EntityFrameworkCore;

namespace Muvit.Infrastructure.Data;

public class MuvitDbContext : DbContext
{
    public MuvitDbContext(DbContextOptions<MuvitDbContext> options) : base(options)
    {
    }
}