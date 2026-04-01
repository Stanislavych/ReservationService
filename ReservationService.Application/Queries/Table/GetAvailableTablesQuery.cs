using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Reservations.ValueObjects;

namespace ReservationService.Application.Queries.Table
{
    public record GetAvailableTablesQuery(DateTime Date, TimeSpan Time, int GuestsCount, TimeSpan? Duration = null) : IRequest<IEnumerable<TableDto>>
    {
        public class Handler : IRequestHandler<GetAvailableTablesQuery, IEnumerable<TableDto>>
        {
            private readonly ITableRepository _tableRepository;
            private readonly IMemoryCache _memoryCache;
            private readonly ILogger<Handler> _logger;
            private readonly TimeSpan _defaultDuration = TimeSpan.FromHours(2);
            private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(2);

            public Handler(ITableRepository tableRepository, IMemoryCache memoryCache, ILogger<Handler> logger)
            {
                _tableRepository = tableRepository;
                _memoryCache = memoryCache;
                _logger = logger;
            }

            public async Task<IEnumerable<TableDto>> Handle(GetAvailableTablesQuery request, CancellationToken cancellationToken)
            {
                var cacheKey = GetHashKey(request);

                if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<TableDto> cachedTables))
                {
                    _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);

                    return cachedTables;
                }

                _logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);

                var utcDate = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
                var duration = request.Duration ?? _defaultDuration;
                var startTime = utcDate.Add(request.Time);
                var endTime = startTime.Add(duration);

                var requestedTime = TimeRange.Create(startTime, endTime);

                if (IsWithinWorkingHours(requestedTime))
                {
                    _logger.LogWarning("Requested time {Start} - {End} is outside working hours",
                        requestedTime.Start, requestedTime.End);

                    return Enumerable.Empty<TableDto>();
                }

                var availableTables = await _tableRepository.GetAvailableTablesAsync(requestedTime, request.GuestsCount, cancellationToken);

                var result = availableTables.Select(t => new TableDto(
                    t.Id,
                    t.Number.Value,
                    t.Type,
                    t.Zone,
                    t.Capacity.Value
                    )).ToList();

                _memoryCache.Set(cacheKey, result, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheDuration,
                    Priority = CacheItemPriority.Normal
                });

                _logger.LogInformation("📦 Cached result for key: {CacheKey} with TTL: {CacheDuration} minutes",
                    cacheKey, _cacheDuration.TotalMinutes);

                return result;
            }

            private bool IsWithinWorkingHours(TimeRange time)
            {
                var openTime = TimeSpan.FromHours(9);
                var closedTime = TimeSpan.FromHours(23);

                return time.Start.TimeOfDay >= openTime &&
                    time.End.TimeOfDay <= closedTime;
            }

            private string GetHashKey(GetAvailableTablesQuery request)
            {
                return $"available_tables_{request.Date:yyyyMMdd}_{request.Time:hh\\:mm}_{request.GuestsCount}_{request.Duration?.TotalMinutes ?? 120}";
            }
        }
    }
}
