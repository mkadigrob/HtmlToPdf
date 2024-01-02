using HtmlToPdfService.Data;
using HtmlToPdfService.Services;
using Microsoft.EntityFrameworkCore;

namespace HtmlToPdfService.Helpers
{
    public static class ServiceCollectionHelper
    {
        public static void AddDbContext(this IServiceCollection services)
        {
            services.AddDbContextFactory<JobContext>((serviceProvider, builder) =>
            {
                var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("DataContext");
                builder
                    .UseSqlServer(connectionString);
            });
            services.AddTransient<IJobRepository, JobRepository>();
        }

        public static void AddServices(this IServiceCollection services)
        {
            services.AddHostedServices();
            services.AddTransient<IStorageService, StorageService>();
        }

        private static void AddHostedServices(this IServiceCollection services)
        {
            services.AddTransient<Worker>();
            services.AddHostedService<WorkerService>();
        }
    }
}
