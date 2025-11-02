namespace Bookstore.API.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddControllers();

            services.AddOpenApi("bookstore");
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Bookstore API", Version = "v1" });
            });

            return services;
        }
    }
}
