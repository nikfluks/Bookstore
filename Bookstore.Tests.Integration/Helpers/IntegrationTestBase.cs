using Bookstore.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Tests.Integration.Helpers;

public abstract class IntegrationTestBase : IDisposable
{
    public AppDbContext DbContext { get; }
    private readonly string databaseName;
    private bool disposed;

    protected IntegrationTestBase()
    {
        databaseName = $"BookstoreTest_{Guid.NewGuid():N}";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer($"Data Source=.;Initial Catalog={databaseName};Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Command Timeout=30")
            .Options;

        DbContext = new AppDbContext(options);

        DbContext.Database.EnsureCreated();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                try
                {
                    DbContext.Database.EnsureDeleted();
                }
                finally
                {
                    DbContext.Dispose();
                }
            }
            // No unmanaged resources to clean up

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

