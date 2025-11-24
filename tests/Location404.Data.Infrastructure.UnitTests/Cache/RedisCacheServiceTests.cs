using FluentAssertions;
using Location404.Data.Infrastructure.Cache;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using System.Net;

namespace Location404.Data.Infrastructure.UnitTests.Cache;

public class RedisCacheServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly Mock<IServer> _serverMock;
    private readonly Mock<ILogger<RedisCacheService>> _loggerMock;
    private readonly RedisCacheService _sut;

    public RedisCacheServiceTests()
    {
        _redisMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        _serverMock = new Mock<IServer>();
        _loggerMock = new Mock<ILogger<RedisCacheService>>();

        _redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);

        _sut = new RedisCacheService(_redisMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ShouldReturnDeserializedValue()
    {
        // Arrange
        var key = "test-key";
        var value = new TestObject { Name = "Test", Value = 123 };
        var json = System.Text.Json.JsonSerializer.Serialize(value);

        _databaseMock.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)json);

        // Act
        var result = await _sut.GetAsync<TestObject>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(123);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var key = "non-existent-key";

        _databaseMock.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _sut.GetAsync<TestObject>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenExceptionOccurs_ShouldReturnNullAndLog()
    {
        // Arrange
        var key = "error-key";

        _databaseMock.Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.SocketFailure, "Connection failed"));

        // Act
        var result = await _sut.GetAsync<TestObject>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithValidData_ShouldSetInRedis()
    {
        // Arrange
        var key = "test-key";
        var value = new TestObject { Name = "Test", Value = 123 };
        var expiration = TimeSpan.FromMinutes(5);

        _databaseMock.Setup(x => x.StringSetAsync(
            key,
            It.IsAny<RedisValue>(),
            expiration,
            false,
            When.Always,
            CommandFlags.None))
            .ReturnsAsync(true);

        // Act
        await _sut.SetAsync(key, value, expiration);

        // Assert
        _databaseMock.Verify(x => x.StringSetAsync(
            key,
            It.IsAny<RedisValue>(),
            expiration,
            false,
            When.Always,
            CommandFlags.None), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WhenExceptionOccurs_ShouldNotThrow()
    {
        // Arrange
        var key = "error-key";
        var value = new TestObject { Name = "Test", Value = 123 };

        _databaseMock.Setup(x => x.StringSetAsync(
            key,
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            false,
            When.Always,
            CommandFlags.None))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.SocketFailure, "Connection failed"));

        // Act
        Func<Task> act = async () => await _sut.SetAsync(key, value, TimeSpan.FromMinutes(5));

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteKey()
    {
        // Arrange
        var key = "test-key";

        _databaseMock.Setup(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _sut.RemoveAsync(key);

        // Assert
        _databaseMock.Verify(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WhenExceptionOccurs_ShouldNotThrow()
    {
        // Arrange
        var key = "error-key";

        _databaseMock.Setup(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.SocketFailure, "Connection failed"));

        // Act
        Func<Task> act = async () => await _sut.RemoveAsync(key);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveByPatternAsync_ShouldDeleteMatchingKeys()
    {
        // Arrange
        var pattern = "test:*";
        var endpoint = new DnsEndPoint("localhost", 6379);
        var keys = new RedisKey[] { "test:1", "test:2" };

        _redisMock.Setup(x => x.GetEndPoints(It.IsAny<bool>()))
            .Returns(new EndPoint[] { endpoint });

        _redisMock.Setup(x => x.GetServer(endpoint, It.IsAny<object>()))
            .Returns(_serverMock.Object);

        _serverMock.Setup(x => x.KeysAsync(
            It.IsAny<int>(),
            pattern,
            It.IsAny<int>(),
            It.IsAny<long>(),
            It.IsAny<int>(),
            It.IsAny<CommandFlags>()))
            .Returns(ToAsyncEnumerable(keys));

        _databaseMock.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _sut.RemoveByPatternAsync(pattern);

        // Assert
        _databaseMock.Verify(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Exactly(2));
    }

    [Fact]
    public async Task RemoveByPatternAsync_WhenNoEndpoints_ShouldNotThrow()
    {
        // Arrange
        var pattern = "test:*";

        _redisMock.Setup(x => x.GetEndPoints(It.IsAny<bool>()))
            .Returns(Array.Empty<EndPoint>());

        // Act
        Func<Task> act = async () => await _sut.RemoveByPatternAsync(pattern);

        // Assert
        await act.Should().NotThrowAsync();
    }

    private static async IAsyncEnumerable<RedisKey> ToAsyncEnumerable(RedisKey[] keys)
    {
        foreach (var key in keys)
        {
            await Task.Yield();
            yield return key;
        }
    }

    private class TestObject
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
