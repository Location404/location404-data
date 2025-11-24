using FluentAssertions;
using Location404.Data.Infrastructure.Configuration;
using Location404.Data.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Location404.Data.Infrastructure.UnitTests.Messaging;

public class MatchConsumerServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenDisabled_ShouldReturnImmediately()
    {
        // Arrange
        var settings = new RabbitMQSettings { Enabled = false };
        var optionsMock = new Mock<IOptions<RabbitMQSettings>>();
        optionsMock.Setup(x => x.Value).Returns(settings);

        var loggerMock = new Mock<ILogger<MatchConsumerService>>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        var service = new MatchConsumerService(
            optionsMock.Object,
            loggerMock.Object,
            serviceProviderMock.Object
        );

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        var executeTask = service.StartAsync(cts.Token);
        await Task.Delay(50);
        await service.StopAsync(cts.Token);

        // Assert
        executeTask.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var settings = new RabbitMQSettings { Enabled = false };
        var optionsMock = new Mock<IOptions<RabbitMQSettings>>();
        optionsMock.Setup(x => x.Value).Returns(settings);

        var loggerMock = new Mock<ILogger<MatchConsumerService>>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        var service = new MatchConsumerService(
            optionsMock.Object,
            loggerMock.Object,
            serviceProviderMock.Object
        );

        // Act
        Action act = () => service.Dispose();

        // Assert
        act.Should().NotThrow();
    }
}
