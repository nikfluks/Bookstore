using Bookstore.API.Jobs;
using Quartz;

namespace Bookstore.API.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
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

        public static IServiceCollection AddQuartzScheduling(this IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                var jobKey = new JobKey("BookImportJob");
                q.AddJob<BookImportJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                   .ForJob(jobKey)
                   .WithIdentity($"{jobKey.Name}-trigger")
                   .WithCronSchedule("0 0 * * * ?") // every hour, every day
                   .WithDescription("Runs book import every hour"));
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            return services;
        }
    }
}
