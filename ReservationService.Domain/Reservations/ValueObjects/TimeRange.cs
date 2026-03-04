namespace ReservationService.Domain.Reservations.ValueObjects
{
    public record TimeRange
    {
        public DateTime Start { get; }
        public DateTime End { get; }

        private TimeRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public static TimeRange Create(DateTime start, DateTime end)
        {
            if (end<= start)
                throw new ArgumentException("End time must be after start time");

            if (start <= DateTime.UtcNow)
                throw new ArgumentException("Start time must be in the future");

            return new TimeRange(start, end);
        }
    }
}
