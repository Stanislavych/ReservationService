namespace ReservationService.Domain.ValueObjects
{
    public record GuestsCount
    {
        public int Value { get; }

        private GuestsCount(int value)
        {
            Value = value;
        }

        public static GuestsCount Create(int value)
        {
            if (value <= 0 || value > 20)
                throw new ArgumentException("Guests count must be between 1 and 20");

            return new GuestsCount(value);
        }
    }
}
