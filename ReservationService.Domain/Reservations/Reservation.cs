using ReservationService.Domain.Common;
using ReservationService.Domain.Exceptions;
using ReservationService.Domain.Reservations.Enums;
using ReservationService.Domain.Reservations.ValueObjects;

namespace ReservationService.Domain.Reservations
{
    public class Reservation : AggregateRoot
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public GuestsCount GuestsCount { get; private set; } = null!;
        public TimeRange ReservationTime { get; private set; } = null!;
        public string Wish { get; private set; } = string.Empty;
        public ReservationStatus Status { get; private set; }

        public int TableId { get; private set; }
        public int UserId { get; private set; }

        public long Version { get; private set; } = 1;

        private Reservation()
        {

        }

        internal Reservation(string name, GuestsCount guestsCount, TimeRange timeRange, string wish, int tableId, int userId)
        {
            if (guestsCount == null)
                throw new DomainException("GuestsCount is required");
            if (timeRange == null)
                throw new DomainException("TimeRange is required");
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Name is required");
            if (tableId <= 0)
                throw new DomainException("TableId is required");
            if (userId <= 0)
                throw new DomainException("UserId is required");

            Name = name;
            GuestsCount = guestsCount;
            ReservationTime = timeRange;
            Wish = wish ?? string.Empty;
            TableId = tableId;
            UserId = userId;
            Status = ReservationStatus.PendingPayment;
        }

        public void Confirm()
        {
            if (Status != ReservationStatus.PendingPayment)
                throw new InvalidOperationException("Can only confirm pending payment reservations");

            Status = ReservationStatus.Confirmed;
        }

        public void Cancel()
        {
            if (Status == ReservationStatus.Cancelled)
                return;

            if (Status == ReservationStatus.Completed)
                throw new InvalidOperationException("Cannot cancel completed reservation");

            Status = ReservationStatus.Cancelled;
        }

        public void Complete()
        {
            if (Status != ReservationStatus.Confirmed)
                throw new InvalidOperationException("Only confirmed reservation can be complited");

            Status = ReservationStatus.Completed;
        }

        public void IncrementVersion()
        {
            Version++;
        }
    }
}