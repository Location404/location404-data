using Microsoft.EntityFrameworkCore;
using Location404.Data.Domain.Entities;

namespace Location404.Data.Infrastructure.Context;

public class GeoDataDbContext(DbContextOptions<GeoDataDbContext> options) : DbContext(options)
{
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<GameMatch> Matches => Set<GameMatch>();
    public DbSet<GameRound> Rounds => Set<GameRound>();
    public DbSet<PlayerStats> PlayerStats => Set<PlayerStats>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Region).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Owned type for Coordinate
            entity.OwnsOne(e => e.Coordinate, coord =>
            {
                coord.Property(c => c.X).IsRequired().HasColumnName("Latitude");
                coord.Property(c => c.Y).IsRequired().HasColumnName("Longitude");
            });

            entity.Property(e => e.Tags)
                .HasColumnType("jsonb")
                .HasDefaultValue(new List<string>());

            entity.HasIndex(e => e.Country);
            entity.HasIndex(e => e.Region);
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<GameMatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlayerAId).IsRequired();
            entity.Property(e => e.PlayerBId).IsRequired();
            entity.Property(e => e.IsCompleted).IsRequired();
            entity.Property(e => e.StartedAt).IsRequired();

            entity.HasMany(e => e.Rounds)
                .WithOne()
                .HasForeignKey(r => r.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.PlayerAId);
            entity.HasIndex(e => e.PlayerBId);
            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => e.StartedAt);
        });

        modelBuilder.Entity<GameRound>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MatchId).IsRequired();
            entity.Property(e => e.RoundNumber).IsRequired();
            entity.Property(e => e.LocationId).IsRequired();
            entity.Property(e => e.IsCompleted).IsRequired();

            entity.OwnsOne(e => e.CorrectAnswer, coord =>
            {
                coord.Property(c => c.X).IsRequired().HasColumnName("CorrectAnswerLatitude");
                coord.Property(c => c.Y).IsRequired().HasColumnName("CorrectAnswerLongitude");
            });

            entity.OwnsOne(e => e.PlayerAGuess, coord =>
            {
                coord.Property(c => c.X).HasColumnName("PlayerAGuessLatitude");
                coord.Property(c => c.Y).HasColumnName("PlayerAGuessLongitude");
            });

            entity.OwnsOne(e => e.PlayerBGuess, coord =>
            {
                coord.Property(c => c.X).HasColumnName("PlayerBGuessLatitude");
                coord.Property(c => c.Y).HasColumnName("PlayerBGuessLongitude");
            });

            entity.HasIndex(e => e.MatchId);
            entity.HasIndex(e => e.LocationId);
        });

        modelBuilder.Entity<PlayerStats>(entity =>
        {
            entity.HasKey(e => e.PlayerId);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => e.RankingPoints).IsDescending();
            entity.HasIndex(e => e.LastMatchAt);
        });
    }
}
