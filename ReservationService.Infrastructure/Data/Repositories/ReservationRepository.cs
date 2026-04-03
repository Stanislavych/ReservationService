using Microsoft.EntityFrameworkCore;
using ReservationService.Domain.Abstractions;
using ReservationService.Domain.Reservations;
using ReservationService.Domain.Reservations.Enums;
using ReservationService.Domain.Reservations.ValueObjects;

namespace ReservationService.Infrastructure.Data.Repositories
{
    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(ApplicationDbContext context) : base(context)
        {
               
        }

        public async Task<bool> HasConflictingReservationsAsync(int tableId, TimeRange timeRange, CancellationToken cancellationToken = default)
        {
            return await _context.Reservations
                .AnyAsync(r=>r.TableId == tableId &&
                            r.Status != ReservationStatus.Cancelled &&
                            r.ReservationTime.Start < timeRange.End &&
                            timeRange.Start < r.ReservationTime.End,
                        cancellationToken);
        }

        public async Task<bool> UpdateWithVersionAsync(Reservation reservation, long expectedVersion, CancellationToken cancellationToken = default)
        {
            var newVersion = expectedVersion + 1;

            var sql = @"
                UPDATE ""Reservations""
                SET 
                    ""Name"" = {0},
                    guests_count = {1},
                    start_time = {2},
                    end_time = {3},
                    ""Wish"" = {4},
                    status = {5},
                    ""Version"" = {6}
                WHERE ""Id"" = {7} AND ""Version"" = {8}";

            var parameters = new object[]
            {
                reservation.Name,
                reservation.GuestsCount.Value,
                reservation.ReservationTime.Start,
                reservation.ReservationTime.End,
                reservation.Wish ?? string.Empty,
                reservation.Status.ToString(),
                newVersion,
                reservation.Id,
                expectedVersion
            };

            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);

            if (rowsAffected > 0)
            {
                reservation.IncrementVersion();
                return true;
            }

            return false;
        }
    }
}
