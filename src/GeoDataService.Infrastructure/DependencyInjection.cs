using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GeoDataService.Infrastructure.Context;
using GeoDataService.Infrastructure.Configuration;
using GeoDataService.Infrastructure.Repositories;
using GeoDataService.Infrastructure.Messaging;
using GeoDataService.Application.Common.Interfaces;
using GeoDataService.Application.Services;

namespace GeoDataService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddGeoDataInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddDatabase(services, configuration);
        AddRepositories(services);
        AddServices(services);
        AddRabbitMQ(services, configuration);

        return services;
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("GeoDataDatabase")
            ?? throw new InvalidOperationException("Connection string 'GeoDataDatabase' not found.");

        // Configure Npgsql data source with dynamic JSON support
        var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        // Register data source as singleton
        services.AddSingleton(dataSource);

        services.AddDbContext<GeoDataDbContext>((serviceProvider, options) =>
        {
            var npgsqlDataSource = serviceProvider.GetRequiredService<Npgsql.NpgsqlDataSource>();
            options.UseNpgsql(npgsqlDataSource, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
            });

            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IGameMatchRepository, GameMatchRepository>();
        services.AddScoped<IPlayerStatsRepository, PlayerStatsRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<IPlayerStatsService, PlayerStatsService>();
    }

    private static void AddRabbitMQ(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));

        var rabbitMQSettings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>();

        if (rabbitMQSettings?.Enabled == true)
        {
            services.AddHostedService<MatchConsumerService>();
        }
    }
}
