namespace GeoDataService.Infrastructure.Configuration;

public class RabbitMQSettings
{
    public bool Enabled { get; set; } = true;
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "game-events";
    public string MatchEndedQueue { get; set; } = "match-ended";
    public string RoundEndedQueue { get; set; } = "round-ended";
}
