namespace Bookstore.API
{
    public static class ServiceCollectionExtensions
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
