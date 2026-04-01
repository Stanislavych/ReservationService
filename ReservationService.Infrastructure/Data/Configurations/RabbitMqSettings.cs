namespace ReservationService.Infrastructure.Data.Configurations
{
    public class RabbitMqSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "admin";
        public string Password { get; set; } = "admin";
        public ExchangeSettings Exchange { get; set; } = new();
        public Dictionary<string, string> Queues { get; set; } = new();
    }

    public class ExchangeSettings
    {
        public string Name { get; set; } = "reservation_events";
        public string Type { get; set; } = "topic";
        public bool Durable { get; set; } = true;
    }
}
