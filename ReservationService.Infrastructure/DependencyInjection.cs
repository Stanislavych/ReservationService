using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Services;
using ReservationService.Infrastructure.Data;
using ReservationService.Infrastructure.Data.Repositories;

namespace ReservationService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(opts => opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IReservationCreationService, ReservationCreationService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ITableRepository, TableRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();

            services.AddScoped<IUnitOfWork,UnitOfWork>();

            return services;
        }
    }
}
