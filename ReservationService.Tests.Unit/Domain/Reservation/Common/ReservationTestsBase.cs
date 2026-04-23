using ReservationService.Domain.Reservations.ValueObjects;

namespace ReservationService.Tests.Unit.Domain.Reservation.Common
{
    public abstract class ReservationTestsBase
    {
        protected readonly GuestsCount _validGuestsCount;
        protected readonly TimeRange _validTimeRange;
        protected readonly DateTime _createdAt;
        protected readonly string _validName = "John Doe";
        protected readonly int _validTableId = 1;
        protected readonly int _validUserId = 1;

        protected ReservationTestsBase()
        {
            _validGuestsCount = GuestsCount.Create(2);
            _validTimeRange = TimeRange.Create(
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow.AddHours(3));
            _createdAt = DateTime.UtcNow;
        }

        protected ReservationService.Domain.Reservations.Reservation CreateValidReservation()
        {
            return new ReservationService.Domain.Reservations.Reservation(
                _validName,
                _validGuestsCount,
                _validTimeRange,
                string.Empty,
                _validTableId,
                _validUserId,
                _createdAt);
        }
    }
}
