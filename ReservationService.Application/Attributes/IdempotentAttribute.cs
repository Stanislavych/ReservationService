namespace ReservationService.Application.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class IdempotentAttribute : Attribute
    {
        public bool Enabled { get; set; } = true;
    }
}
