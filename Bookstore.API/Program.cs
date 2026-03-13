using Asp.Versioning.ApiExplorer;
using Bookstore.API.Extensions;
using Bookstore.Application.Extensions;
using Bookstore.Application.Interfaces;
using Bookstore.Infrastructure.Extensions;
using Serilog;
using System.Globalization;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Bookstore API starting...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration);
    });

    builder.Services.AddApiServices();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddQuartzScheduling();
    builder.Services.AddRateLimiting();
    builder.Services.AddAppServices();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    await app.ApplyDatabaseMigrationsAsync();

    if (app.Environment.IsDevelopment())
    {
        using var seederScope = app.Services.CreateScope();
        var seeder = seederScope.ServiceProvider.GetRequiredService<IIdentitySeeder>();
        await seeder.SeedAsync();
    }

    app.UseForwardedHeaders();

    app.UseExceptionHandler();
    app.UseStatusCodePages();

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseRateLimiter();
    app.UseAuthorization();

    app.MapControllers();

    if (app.Environment.IsDevelopment())
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var groupName in provider.ApiVersionDescriptions.Select(static description => description.GroupName))
            {
                options.SwaggerEndpoint(
                    $"/swagger/{groupName}/swagger.json",
                    groupName.ToUpperInvariant());
            }
        });
    }

    Log.Information("Bookstore API started successfully");
    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Bookstore API failed to start");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
