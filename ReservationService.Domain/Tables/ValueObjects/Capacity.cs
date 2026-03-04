namespace ReservationService.Domain.Tables.ValueObjects
{
    public record Capacity
    {
        public int Value { get;}

        private Capacity(int value)
        {
            Value = value;
        }

        public static Capacity Create(int value) 
        {
            if (value <= 0 || value > 20)
                throw new ArgumentException("Capacity must be between 1 and 20");

            return new Capacity(value);
        }
    }
}
