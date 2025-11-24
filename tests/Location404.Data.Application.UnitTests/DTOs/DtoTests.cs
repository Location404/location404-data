using FluentAssertions;
using Location404.Data.Application.DTOs;
using Location404.Data.Application.DTOs.Events;
using Location404.Data.Application.DTOs.Requests;
using Location404.Data.Application.DTOs.Responses;

namespace Location404.Data.Application.UnitTests.DTOs;

public class DtoTests
{
    [Fact]
    public void CoordinateDto_ShouldInitializeCorrectly()
    {
        // Act
        var dto = new CoordinateDto(-23.55, -46.63);

        // Assert
        dto.X.Should().Be(-23.55);
        dto.Y.Should().Be(-46.63);
        dto.Latitude.Should().Be(-23.55);
        dto.Longitude.Should().Be(-46.63);
    }

    [Fact]
    public void CreateLocationRequest_ShouldInitializeWithAllProperties()
    {
        // Act
        var request = new CreateLocationRequest(
            Latitude: -23.55,
            Longitude: -46.63,
            Name: "Test Location",
            Country: "Brasil",
            Region: "SP",
            Heading: 90,
            Pitch: 10,
            Tags: new List<string> { "urban" }
        );

        // Assert
        request.Latitude.Should().Be(-23.55);
        request.Longitude.Should().Be(-46.63);
        request.Name.Should().Be("Test Location");
        request.Country.Should().Be("Brasil");
        request.Region.Should().Be("SP");
        request.Heading.Should().Be(90);
        request.Pitch.Should().Be(10);
        request.Tags.Should().Contain("urban");
    }

    [Fact]
    public void LocationResponse_ShouldInitializeWithAllProperties()
    {
        // Act
        var response = new LocationResponse(
            Id: Guid.NewGuid(),
            Coordinate: new CoordinateDto(-23.55, -46.63),
            Name: "Test",
            Country: "Brasil",
            Region: "SP",
            Heading: 90,
            Pitch: 10,
            TimesUsed: 5,
            AveragePoints: 4500.0,
            DifficultyRating: 3,
            Tags: new List<string> { "tag1" },
            IsActive: true
        );

        // Assert
        response.Name.Should().Be("Test");
        response.Country.Should().Be("Brasil");
        response.TimesUsed.Should().Be(5);
        response.IsActive.Should().BeTrue();
    }

    [Fact]
    public void PlayerStatsResponse_ShouldInitializeWithAllProperties()
    {
        // Act
        var response = new PlayerStatsResponse(
            PlayerId: Guid.NewGuid(),
            TotalMatches: 10,
            Wins: 6,
            Losses: 3,
            Draws: 1,
            WinRate: 60.0,
            TotalRoundsPlayed: 30,
            TotalPoints: 45000,
            HighestScore: 5000,
            AveragePointsPerRound: 1500.0,
            AverageDistanceErrorKm: 50.0,
            RankingPoints: 1200,
            LastMatchAt: DateTime.UtcNow
        );

        // Assert
        response.PlayerId.Should().NotBeEmpty();
        response.TotalMatches.Should().Be(10);
        response.Wins.Should().Be(6);
        response.Losses.Should().Be(3);
        response.Draws.Should().Be(1);
        response.WinRate.Should().Be(60.0);
        response.TotalRoundsPlayed.Should().Be(30);
        response.TotalPoints.Should().Be(45000);
        response.HighestScore.Should().Be(5000);
        response.AveragePointsPerRound.Should().Be(1500.0);
        response.AverageDistanceErrorKm.Should().Be(50.0);
        response.RankingPoints.Should().Be(1200);
        response.LastMatchAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GameRoundEventDto_ShouldInitializeWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        var playerAId = Guid.NewGuid();
        var playerBId = Guid.NewGuid();

        // Act
        var dto = new GameRoundEventDto(
            Id: id,
            GameMatchId: matchId,
            RoundNumber: 1,
            PlayerAId: playerAId,
            PlayerBId: playerBId,
            PlayerAPoints: 2500,
            PlayerBPoints: 2300,
            GameResponse: new CoordinateDto(-23.55, -46.63),
            PlayerAGuess: new CoordinateDto(-23.56, -46.64),
            PlayerBGuess: new CoordinateDto(-23.54, -46.62),
            GameRoundEnded: true
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.GameMatchId.Should().Be(matchId);
        dto.RoundNumber.Should().Be(1);
        dto.PlayerAPoints.Should().Be(2500);
        dto.PlayerBPoints.Should().Be(2300);
        dto.GameRoundEnded.Should().BeTrue();
    }

    [Fact]
    public void GameMatchEndedEventDto_ShouldInitializeWithAllProperties()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var playerAId = Guid.NewGuid();
        var playerBId = Guid.NewGuid();
        var winnerId = playerAId;
        var loserId = playerBId;

        // Act
        var dto = new GameMatchEndedEventDto(
            MatchId: matchId,
            PlayerAId: playerAId,
            PlayerBId: playerBId,
            WinnerId: winnerId,
            LoserId: loserId,
            PlayerATotalPoints: 5000,
            PlayerBTotalPoints: 4500,
            PointsEarned: 100,
            PointsLost: 50,
            StartTime: DateTime.UtcNow.AddMinutes(-5),
            EndTime: DateTime.UtcNow,
            Rounds: new List<GameRoundEventDto>()
        );

        // Assert
        dto.MatchId.Should().Be(matchId);
        dto.PlayerAId.Should().Be(playerAId);
        dto.WinnerId.Should().Be(winnerId);
        dto.PlayerATotalPoints.Should().Be(5000);
        dto.PlayerBTotalPoints.Should().Be(4500);
        dto.Rounds.Should().BeEmpty();
    }

    [Fact]
    public void GameMatchResponse_ShouldInitializeWithAllProperties()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var playerAId = Guid.NewGuid();
        var playerBId = Guid.NewGuid();

        // Act
        var response = new GameMatchResponse(
            Id: matchId,
            PlayerAId: playerAId,
            PlayerBId: playerBId,
            PlayerATotalPoints: 5000,
            PlayerBTotalPoints: 4500,
            WinnerId: playerAId,
            LoserId: playerBId,
            StartedAt: DateTime.UtcNow.AddMinutes(-5),
            EndedAt: DateTime.UtcNow,
            IsCompleted: true,
            Rounds: new List<GameRoundResponse>()
        );

        // Assert
        response.Id.Should().Be(matchId);
        response.PlayerAId.Should().Be(playerAId);
        response.PlayerBId.Should().Be(playerBId);
        response.PlayerATotalPoints.Should().Be(5000);
        response.PlayerBTotalPoints.Should().Be(4500);
        response.WinnerId.Should().Be(playerAId);
        response.LoserId.Should().Be(playerBId);
        response.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(10));
        response.EndedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
        response.IsCompleted.Should().BeTrue();
        response.Rounds.Should().BeEmpty();
    }

    [Fact]
    public void GameMatchResponse_WithNullWinner_ShouldHandleNulls()
    {
        // Act
        var response = new GameMatchResponse(
            Id: Guid.NewGuid(),
            PlayerAId: Guid.NewGuid(),
            PlayerBId: Guid.NewGuid(),
            PlayerATotalPoints: 5000,
            PlayerBTotalPoints: 5000,
            WinnerId: null,
            LoserId: null,
            StartedAt: DateTime.UtcNow,
            EndedAt: null,
            IsCompleted: false,
            Rounds: new List<GameRoundResponse>()
        );

        // Assert
        response.WinnerId.Should().BeNull();
        response.LoserId.Should().BeNull();
        response.EndedAt.Should().BeNull();
        response.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void GameRoundResponse_ShouldInitializeWithAllProperties()
    {
        // Arrange
        var roundId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var playerAId = Guid.NewGuid();
        var playerBId = Guid.NewGuid();

        // Act
        var response = new GameRoundResponse(
            Id: roundId,
            RoundNumber: 1,
            LocationId: locationId,
            CorrectAnswer: new CoordinateDto(-23.55, -46.63),
            PlayerAId: playerAId,
            PlayerAGuess: new CoordinateDto(-23.56, -46.64),
            PlayerADistance: 1.5,
            PlayerAPoints: 2500,
            PlayerBId: playerBId,
            PlayerBGuess: new CoordinateDto(-23.54, -46.62),
            PlayerBDistance: 2.0,
            PlayerBPoints: 2300,
            StartedAt: DateTime.UtcNow.AddMinutes(-2),
            EndedAt: DateTime.UtcNow,
            IsCompleted: true
        );

        // Assert
        response.Id.Should().Be(roundId);
        response.RoundNumber.Should().Be(1);
        response.LocationId.Should().Be(locationId);
        response.CorrectAnswer.Should().NotBeNull();
        response.PlayerAId.Should().Be(playerAId);
        response.PlayerAGuess.Should().NotBeNull();
        response.PlayerADistance.Should().Be(1.5);
        response.PlayerAPoints.Should().Be(2500);
        response.PlayerBId.Should().Be(playerBId);
        response.PlayerBGuess.Should().NotBeNull();
        response.PlayerBDistance.Should().Be(2.0);
        response.PlayerBPoints.Should().Be(2300);
        response.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
        response.EndedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
        response.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void GameRoundResponse_WithNullGuesses_ShouldHandleNulls()
    {
        // Act
        var response = new GameRoundResponse(
            Id: Guid.NewGuid(),
            RoundNumber: 1,
            LocationId: Guid.NewGuid(),
            CorrectAnswer: new CoordinateDto(-23.55, -46.63),
            PlayerAId: Guid.NewGuid(),
            PlayerAGuess: null,
            PlayerADistance: null,
            PlayerAPoints: null,
            PlayerBId: Guid.NewGuid(),
            PlayerBGuess: null,
            PlayerBDistance: null,
            PlayerBPoints: null,
            StartedAt: DateTime.UtcNow,
            EndedAt: null,
            IsCompleted: false
        );

        // Assert
        response.PlayerAGuess.Should().BeNull();
        response.PlayerADistance.Should().BeNull();
        response.PlayerAPoints.Should().BeNull();
        response.EndedAt.Should().BeNull();
        response.IsCompleted.Should().BeFalse();
    }
}
