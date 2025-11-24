using FluentAssertions;
using Location404.Data.Infrastructure.Configuration;

namespace Location404.Data.Infrastructure.UnitTests.Configuration;

public class RabbitMQSettingsTests
{
    [Fact]
    public void RabbitMQSettings_ShouldHaveDefaultValues()
    {
        // Act
        var settings = new RabbitMQSettings();

        // Assert
        settings.Enabled.Should().BeTrue();
        settings.HostName.Should().Be("localhost");
        settings.Port.Should().Be(5672);
        settings.UserName.Should().Be("guest");
        settings.Password.Should().Be("guest");
        settings.VirtualHost.Should().Be("/");
        settings.ExchangeName.Should().Be("game-events");
        settings.MatchEndedQueue.Should().Be("match-ended");
        settings.RoundEndedQueue.Should().Be("round-ended");
    }

    [Fact]
    public void RabbitMQSettings_ShouldAllowPropertyChanges()
    {
        // Arrange
        var settings = new RabbitMQSettings();

        // Act
        settings.Enabled = false;
        settings.HostName = "rabbitmq.example.com";
        settings.Port = 5673;
        settings.UserName = "admin";
        settings.Password = "secret";
        settings.VirtualHost = "/production";
        settings.ExchangeName = "custom-events";
        settings.MatchEndedQueue = "custom-match-queue";
        settings.RoundEndedQueue = "custom-round-queue";

        // Assert
        settings.Enabled.Should().BeFalse();
        settings.HostName.Should().Be("rabbitmq.example.com");
        settings.Port.Should().Be(5673);
        settings.UserName.Should().Be("admin");
        settings.Password.Should().Be("secret");
        settings.VirtualHost.Should().Be("/production");
        settings.ExchangeName.Should().Be("custom-events");
        settings.MatchEndedQueue.Should().Be("custom-match-queue");
        settings.RoundEndedQueue.Should().Be("custom-round-queue");
    }
}
