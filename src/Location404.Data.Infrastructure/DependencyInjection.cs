using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Location404.Data.Infrastructure.Context;
using Location404.Data.Infrastructure.Configuration;
using Location404.Data.Infrastructure.Repositories;
using Location404.Data.Infrastructure.Messaging;
using Location404.Data.Application.Common.Interfaces;
using Location404.Data.Application.Services;

namespace Location404.Data.Infrastructure;

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
