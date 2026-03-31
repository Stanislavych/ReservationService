namespace ReservationService.Infrastructure.Data.Configurations
{
    public class RabbitMqSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "admin";
        public string Password { get; set; } = "admin";
        public string VirtualHost { get; set; } = "/";

        public ExchangeSettings Exchange { get; set; } = new();
        public Dictionary<string, string> Queues { get; set; } = new();
        public RetrySettings RetrySettings { get; set; } = new();
    }

    public class ExchangeSettings
    {
        public string Name { get; set; } = "reservation_events";
        public string Type { get; set; } = "topic";
        public bool Durable { get; set; } = true;
    }

    public class RetrySettings
    {
        public int MaxRetries { get; set; } = 3;
        public int InitialDelaySeconds { get; set; } = 2;
        public int MaxDelaySeconds { get; set; } = 30;
        public int DelayMultiplier { get; set; } = 2;
    }
}
