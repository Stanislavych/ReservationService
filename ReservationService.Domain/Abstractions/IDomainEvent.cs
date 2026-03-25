namespace ReservationService.Domain.Abstractions
{
    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}
