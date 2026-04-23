namespace ReservationService.Infrastructure.Data.Configurations
{
    public class RedisSettings
    {
        public bool Enabled { get; set; } = true;
        public string ConnectionString { get; set; } = "localhost:6379";
        public string InstanceName { get; set; } = "ReservationService:";
        public int DefaultExpirationMinutes { get; set; } = 2;
    }

    public class CacheSettings
    {
        public string Provider { get; set; } = "Redis";
        public bool FallbackToMemory { get; set; } = true;
    }
}
