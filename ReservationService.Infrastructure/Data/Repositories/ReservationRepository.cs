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
    }
}
