using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Application.Interfaces;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Services;
using ReservationService.Infrastructure.Data;
using ReservationService.Infrastructure.Data.Configurations;
using ReservationService.Infrastructure.Data.Repositories;
using ReservationService.Infrastructure.Services;

namespace ReservationService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(opts => opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IReservationCreationService, ReservationCreationService>();
            services.AddScoped<IReservationUpdateService, ReservationUpdateService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ITableRepository, TableRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddScoped<IUnitOfWork,UnitOfWork>();

            services.AddHostedService<OutboxPublisher>();
            services.AddHostedService<ReservationEventConsumer>();

            services.AddSingleton<RabbitMqConnection>();
            services.AddSingleton<IMessageBroker, RabbitMqMessageBroker>();

            services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMQ"));

            return services;
        }
    }
}
