using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Application.Interfaces;
using ReservationService.Domain.Abstractions;
using ReservationService.Infrastructure.Data;
using ReservationService.Infrastructure.Data.Repositories;
using ReservationService.Infrastructure.Services;

namespace ReservationService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(opts => opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IReservationValidationService, ReservationValidationService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ITableRepository, TableRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();

            services.AddScoped<IUnitOfWork>(sp =>
            {
                var context = sp.GetRequiredService<ApplicationDbContext>();

                return new UnitOfWork(context);
            });

            return services;
        }
    }
}
