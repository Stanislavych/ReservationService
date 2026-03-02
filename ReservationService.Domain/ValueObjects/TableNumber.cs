namespace ReservationService.Domain.ValueObjects
{
    public record TableNumber
    {
        public int Value { get;}

        private TableNumber(int value)
        {
            Value = value;
        }

        public static TableNumber Create(int value)
        {
            if (value <= 0 || value > 100)
                throw new ArgumentException("Table number must be between 1 and 100");

            return new TableNumber(value);
        }
    }
}
