using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReservationService.Application.Interfaces;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Services;
using ReservationService.Infrastructure.Data;
using ReservationService.Infrastructure.Data.Configurations;
using ReservationService.Infrastructure.Data.Repositories;
using ReservationService.Infrastructure.Services;
using StackExchange.Redis;

namespace ReservationService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(opts => opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IJWTService, JWTService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IReservationCreationService, ReservationCreationService>();
            services.AddScoped<IReservationUpdateService, ReservationUpdateService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<ITableRepository, TableRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddScoped<IUnitOfWork,UnitOfWork>();
            services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();

            services.AddHostedService<OutboxPublisher>();

            var redisSettings = configuration.GetSection("Redis").Get<RedisSettings>();
            var cacheSettings = configuration.GetSection("Cache").Get<CacheSettings>();

            services.Configure<RedisSettings>(configuration.GetSection("Redis"));
            services.AddMemoryCache();

            if (redisSettings?.Enabled == true)
            {
                try
                {
                    var connectionString = redisSettings.ConnectionString;
                    var redis = ConnectionMultiplexer.Connect(connectionString);

                    services.AddSingleton<IConnectionMultiplexer>(redis);
                    services.AddSingleton<ICacheService>(sp =>
                    {
                        var memoryCache = sp.GetRequiredService<IMemoryCache>();
                        var logger = sp.GetRequiredService<ILogger<MemoryCacheService>>();
                        var memoryCacheService = new MemoryCacheService(memoryCache, logger);

                        var redisLogger = sp.GetRequiredService<ILogger<RedisCacheService>>();
                        var options = sp.GetRequiredService<IOptions<RedisSettings>>();
                        var redisMultiplexer = sp.GetRequiredService<IConnectionMultiplexer>();

                        return new RedisCacheService(redisMultiplexer, options, redisLogger, memoryCacheService);
                    });

                    services.AddSingleton<ICacheService>(sp => sp.GetRequiredService<ICacheService>());
                }
                catch (Exception ex)
                {
                    var logger = services.BuildServiceProvider().GetService<ILogger<RedisCacheService>>();
                    
                    logger?.LogError(ex, "Failed to connect to Redis. Falling back to MemoryCache only.");

                    services.AddSingleton<ICacheService, MemoryCacheService>();
                }
            }
            else
            {
                services.AddSingleton<ICacheService, MemoryCacheService>();
            }

            return services;
        }
    }
}
