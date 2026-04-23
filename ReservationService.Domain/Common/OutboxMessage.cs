namespace ReservationService.Domain.Common
{
    public class OutboxMessage
    {
        public Guid Id { get; private set; }
        public string EventType { get; private set; }
        public string Payload { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public bool IsPublished { get; private set; }
        public int RetryCount { get; private set; }
        public string Error { get; private set; }

        private OutboxMessage() { }

        public OutboxMessage(string eventType, string payload)
        {
            Id = Guid.NewGuid();
            EventType = eventType;
            Payload = payload;
            CreatedAt = DateTime.UtcNow;
            IsPublished = false;
            RetryCount = 0;
        }

        public void MarkAsPublished()
        {
            IsPublished = true;
            ProcessedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string error)
        {
            RetryCount++;
            Error = error;
        }
    }
}
