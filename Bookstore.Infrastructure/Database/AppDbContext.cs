using Microsoft.EntityFrameworkCore;

namespace Bookstore.Infrastructure.Database
{
    internal class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
