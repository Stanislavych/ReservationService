using MediatR;
using ReservationService.Application.DTOs;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Reservations.ValueObjects;

namespace ReservationService.Application.Queries.Table
{
    public record GetAvailableTablesQuery(DateTime Date,TimeSpan Time ,int GuestsCount, TimeSpan? Duration = null) : IRequest<IEnumerable<TableDto>>
    {
        public class Handler : IRequestHandler<GetAvailableTablesQuery, IEnumerable<TableDto>>
        {
            private readonly ITableRepository _tableRepository;
            private readonly TimeSpan _defaultDuration = TimeSpan.FromHours(2);

            public Handler(ITableRepository tableRepository)
            {
                _tableRepository = tableRepository;
            }

            public async Task<IEnumerable<TableDto>> Handle(GetAvailableTablesQuery request, CancellationToken cancellationToken)
            {
                var utcDate = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
                var duration = request.Duration ?? _defaultDuration;
                var startTime = utcDate.Add(request.Time);
                var endTime = startTime.Add(duration);

                var requestedTime = TimeRange.Create(startTime, endTime);

                if (IsWithinWorkingHours(requestedTime))
                    return Enumerable.Empty<TableDto>();

                var availableTables = await _tableRepository.GetAvailableTablesAsync(requestedTime, request.GuestsCount, cancellationToken);

                return availableTables.Select(t => new TableDto(
                    t.Number,
                    t.Type,
                    t.Zone,
                    t.Capacity
                    ));
            }

            private bool IsWithinWorkingHours(TimeRange time)
            {
                var openTime = TimeSpan.FromHours(9);
                var closedTime = TimeSpan.FromHours(23);

                return time.Start.TimeOfDay >= openTime &&
                    time.End.TimeOfDay <= closedTime;
            }
        }
    }
}
