using Bookstore.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Bookstore.Tests.Integration.Helpers;

internal sealed class BookstoreWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"BookstoreTest_{Guid.NewGuid():N}";

    private const string TestJwtSecret = "TestSuperSecretKeyThatIsLongEnough123456!";
    private const string TestJwtIssuer = "BookstoreAPI";
    private const string TestJwtAudience = "BookstoreSwaggerClient";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        var connectionString =
            $"Data Source=.;Initial Catalog={_databaseName};Integrated Security=True;Persist Security Info=False;" +
            "Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Command Timeout=30";

        builder.UseSetting("ConnectionStrings:BookstoreDB", connectionString);
        builder.UseSetting("JwtSettings:Secret", TestJwtSecret);
        builder.UseSetting("JwtSettings:Issuer", TestJwtIssuer);
        builder.UseSetting("JwtSettings:Audience", TestJwtAudience);
        builder.UseSetting("JwtSettings:ExpirationMinutes", "10");
    }

    public override async ValueTask DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureDeletedAsync();

        await base.DisposeAsync();
    }
}
