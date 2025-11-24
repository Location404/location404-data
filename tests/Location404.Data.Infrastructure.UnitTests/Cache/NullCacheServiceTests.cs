using FluentAssertions;
using Location404.Data.Infrastructure.Cache;

namespace Location404.Data.Infrastructure.UnitTests.Cache;

public class NullCacheServiceTests
{
    private readonly NullCacheService _sut;

    public NullCacheServiceTests()
    {
        _sut = new NullCacheService();
    }

    [Fact]
    public async Task GetAsync_ShouldAlwaysReturnNull()
    {
        // Act
        var result = await _sut.GetAsync<string>("any-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ShouldCompleteWithoutError()
    {
        // Act
        Func<Task> act = async () => await _sut.SetAsync("key", "value", TimeSpan.FromMinutes(5));

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveAsync_ShouldCompleteWithoutError()
    {
        // Act
        Func<Task> act = async () => await _sut.RemoveAsync("key");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RemoveByPatternAsync_ShouldCompleteWithoutError()
    {
        // Act
        Func<Task> act = async () => await _sut.RemoveByPatternAsync("pattern*");

        // Assert
        await act.Should().NotThrowAsync();
    }
}
