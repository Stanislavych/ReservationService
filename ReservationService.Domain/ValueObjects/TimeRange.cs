namespace ReservationService.Domain.ValueObjects
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

            return new TimeRange(start, end);
        }
    }
}
