using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Infrastructure.Data;

namespace ReservationService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(opts => opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}
