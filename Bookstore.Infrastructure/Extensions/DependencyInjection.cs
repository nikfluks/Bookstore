using Bookstore.Application.Interfaces;
using Bookstore.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Bookstore.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("BookstoreDB"));
            });

            services.AddScoped<IAppDbContext, AppDbContext>();

            return services;
        }

        public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Log.Information("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");
        }
    }
}
