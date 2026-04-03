namespace ReservationService.Domain.Common
{
    public class IdempotencyRecord
    {
        public Guid Id { get; private set; }
        public string Key { get; private set; }
        public string CommandType { get; private set; }
        public string Response { get; private set; }
        public int StatusCode { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public bool IsSuccessful { get; private set; }
        public string ErrorMessage { get; private set; }

        private IdempotencyRecord()
        {
            
        }

        public IdempotencyRecord(string key, string commandType, string response, int statusCode, bool isSuccessful, string errorMessage = null)
        {
            Id = Guid.NewGuid();
            Key = key;
            CommandType = commandType;
            Response = response;
            StatusCode = statusCode;
            CreatedAt = DateTime.UtcNow;
            ProcessedAt = DateTime.UtcNow;
            ErrorMessage = errorMessage;
            IsSuccessful = isSuccessful;
        }
    }
}
