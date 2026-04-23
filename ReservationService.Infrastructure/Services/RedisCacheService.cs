using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReservationService.Application.Interfaces;
using ReservationService.Infrastructure.Data.Configurations;
using StackExchange.Redis;
using System.Text.Json;

namespace ReservationService.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly RedisSettings _settings;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly ICacheService _fallbackCache;

        public RedisCacheService(IConnectionMultiplexer redis, IOptions<RedisSettings> settings,
            ILogger<RedisCacheService> logger, ICacheService fallbackCache = null)
        {
            _redis = redis;
            _settings = settings.Value;
            _logger = logger;
            _fallbackCache = fallbackCache;
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = _redis.GetDatabase();
                var fullKey = $"{_settings.InstanceName}{key}";

                return await db.KeyExistsAsync(fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis error on exists for key: {Key}", key);

                return false;
            }
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = _redis.GetDatabase();
                var fullKey = $"{_settings.InstanceName}{key}";
                var value = await db.StringGetAsync(fullKey);

                if (value.HasValue)
                {
                    _logger.LogDebug("Redis HIT for key: {Key}", fullKey);

                    return JsonSerializer.Deserialize<T>(value.ToString());
                }

                _logger.LogDebug("Redis MISS for key: {Key}", fullKey);

                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis error on GET for key: {Key}", key);

                if (_fallbackCache != null)
                {
                    _logger.LogWarning("Falling back to MemoryCache for key: {Key}", key);
                    return await _fallbackCache.GetAsync<T>(key, cancellationToken);
                }

                return default;
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = _redis.GetDatabase();
                var fullKey = $"{_settings.InstanceName}{key}";

                await db.KeyDeleteAsync(fullKey);

                _logger.LogDebug("Redis remove for key: {Key}", fullKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis error on remove for key: {Key}", key);

                if (_fallbackCache != null)
                {
                    await _fallbackCache.RemoveAsync(key, cancellationToken);
                }
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = _redis.GetDatabase();
                var fullKey = $"{_settings.InstanceName}{key}";
                var json = JsonSerializer.Serialize(value);
                var expiry = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);

                await db.StringSetAsync(fullKey, json, expiry);

                _logger.LogDebug("Redis set for key: {Key}, TTL: {TTL}s", fullKey, expiry.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis error on set for key: {Key}", key);

                if (_fallbackCache != null)
                {
                    _logger.LogWarning("Falling back to MemoryCache for key: {Key}", key);

                    await _fallbackCache.SetAsync(key, value, expiration, cancellationToken);
                }
            }
        }
    }
}
