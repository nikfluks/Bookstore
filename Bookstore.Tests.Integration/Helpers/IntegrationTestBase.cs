using Bookstore.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Tests.Integration.Helpers
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly AppDbContext DbContext;
        protected readonly string DatabaseName;

        protected IntegrationTestBase()
        {
            DatabaseName = $"BookstoreTest_{Guid.NewGuid():N}";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer($"Data Source=.;Initial Catalog={DatabaseName};Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Command Timeout=30")
                .Options;

            DbContext = new AppDbContext(options);

            DbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            try
            {
                DbContext.Database.EnsureDeleted();
            }
            catch
            {

            }
            finally
            {
                DbContext.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
