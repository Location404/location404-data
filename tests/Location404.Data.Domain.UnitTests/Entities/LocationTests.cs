using FluentAssertions;
using Location404.Data.Domain.Entities;
using Location404.Data.Domain.ValueObjects;

namespace Location404.Data.Domain.UnitTests.Entities;

public class LocationTests
{
    private readonly Coordinate _testCoordinate = new(40.7128, -74.0060);

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateLocation()
    {
        var id = Guid.NewGuid();
        var name = "Times Square";
        var country = "USA";
        var region = "New York";

        var location = new Location(id, _testCoordinate, name, country, region);

        location.Id.Should().Be(id);
        location.Coordinate.Should().Be(_testCoordinate);
        location.Name.Should().Be(name);
        location.Country.Should().Be(country);
        location.Region.Should().Be(region);
        location.IsActive.Should().BeTrue();
        location.TimesUsed.Should().Be(0);
        location.AveragePoints.Should().BeNull();
        location.Tags.Should().BeEmpty();
    }

    [Fact]
    public void SetStreetViewParams_ShouldUpdateHeadingAndPitch()
    {
        var location = new Location(Guid.NewGuid(), _testCoordinate, "Test", "Country", "Region");
        var heading = 90;
        var pitch = 10;

        location.SetStreetViewParams(heading, pitch);

        location.Heading.Should().Be(heading);
        location.Pitch.Should().Be(pitch);
        location.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateStats_FirstTime_ShouldSetAveragePointsToScore()
    {
        var location = new Location(Guid.NewGuid(), _testCoordinate, "Test", "Country", "Region");
        var points = 4500;

        location.UpdateStats(points);

        location.TimesUsed.Should().Be(1);
        location.AveragePoints.Should().Be(points);
        location.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateStats_MultipleTimes_ShouldCalculateCorrectAverage()
    {
        var location = new Location(Guid.NewGuid(), _testCoordinate, "Test", "Country", "Region");

        location.UpdateStats(5000);
        location.UpdateStats(3000);
        location.UpdateStats(4000);

        location.TimesUsed.Should().Be(3);
        location.AveragePoints.Should().Be(4000);
    }

    [Fact]
    public void SetDifficulty_WithValidRating_ShouldSetDifficulty()
    {
        var location = new Location(Guid.NewGuid(), _testCoordinate, "Test", "Country", "Region");

        location.SetDifficulty(3);

        location.DifficultyRating.Should().Be(3);
        location.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetDifficulty_WithRatingBelowOne_ShouldThrowArgumentException()
    {
        var location = new Location(Guid.NewGuid(), _testCoordinate, "Test", "Country", "Region");

        var act = () => location.SetDifficulty(0);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Difficulty rating must be between 1 and 5");
    }

    [Fact]
    public void SetDifficulty_WithRatingAboveFive_ShouldThrowArgumentException()
    {
        var location = new Location(Guid.NewGuid(), _testCoordinate, "Test", "Country", "Region");

        var act = () => location.SetDifficulty(6);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Difficulty rating must be between 1 and 5");
    }

    [Fact]
    public void AddTag_WithNewTag_ShouldAddTagToList()
    {
        var location = new Location(Guid.NewGuid(), _testCoordinate, "Test", "Country", "Region");
        var tag = "urban";

        location.AddTag(tag);

        location.Tags.Should().Contain(tag);
        location.Tags.Should().HaveCount(1);
        location.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddTag_WithDuplicateTag_ShouldNotAddAgain()
    {
        var location = new Location(Guid.NewGuid(), _testCoordinate, "Test", "Country", "Region");
        var tag = "urban";

        location.AddTag(tag);
        location.AddTag(tag);

        location.Tags.Should().HaveCount(1);
    }

    [Fact]
    public void AddTag_WithMultipleDifferentTags_ShouldAddAllTags()
    {
        var location = new Location(Guid.NewGuid(), _testCoordinate, "Test", "Country", "Region");

        location.AddTag("urban");
        location.AddTag("historic");
        location.AddTag("tourist");

        location.Tags.Should().HaveCount(3);
        location.Tags.Should().Contain(new[] { "urban", "historic", "tourist" });
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var location = new Location(Guid.NewGuid(), _testCoordinate, "Test", "Country", "Region");

        location.Deactivate();

        location.IsActive.Should().BeFalse();
        location.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_AfterDeactivate_ShouldSetIsActiveToTrue()
    {
        var location = new Location(Guid.NewGuid(), _testCoordinate, "Test", "Country", "Region");
        location.Deactivate();

        location.Activate();

        location.IsActive.Should().BeTrue();
        location.UpdatedAt.Should().NotBeNull();
    }
}
