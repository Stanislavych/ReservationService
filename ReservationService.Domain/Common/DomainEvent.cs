using ReservationService.Domain.Abstractions;

namespace ReservationService.Domain.Common
{
    public abstract class DomainEvent : IDomainEvent
    {
      public DateTime OccurredAt { get; protected set; }

        protected DomainEvent()
        {
            OccurredAt = DateTime.UtcNow;
        }
    }
}
