using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ReservationService.Application.Interfaces;

namespace ReservationService.Infrastructure.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out _));
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("MemoryCache hit for key: {Key}", key);

                return Task.FromResult(value);
            }

            _logger.LogDebug("MemoryCache miss for key {Key}", key);

            return Task.FromResult(default(T));
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _memoryCache.Remove(key);
            _logger.LogDebug("MemoryCache remove for key: {Key}", key);

            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var options = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
                options.AbsoluteExpirationRelativeToNow = expiration;

            _memoryCache.Set(key, value, options);
            _logger.LogDebug("MemoryCache set for key: {Key}, TTL: {TTL}s",key,expiration?.TotalSeconds);

            return Task.CompletedTask;
        }
    }
}
