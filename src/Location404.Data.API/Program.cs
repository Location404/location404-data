using Location404.Data.Infrastructure;
using Location404.Data.Infrastructure.Context;
using Location404.Data.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Shared.Observability.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddGeoDataInfrastructure(builder.Configuration);
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddOpenTelemetryObservability(builder.Configuration, options =>
{
    options.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
});

builder.Services.AddObservabilityHealthChecks(builder.Configuration, checks =>
{
    var postgresConnection = builder.Configuration.GetConnectionString("GeoDataDatabase");
    if (!string.IsNullOrEmpty(postgresConnection))
    {
        checks.AddNpgSql(postgresConnection, name: "postgres", tags: new[] { "ready", "db" }, timeout: TimeSpan.FromSeconds(3));
    }
});

var originsString = builder.Configuration.GetValue<string>("Cors:AllowedOrigins") ?? "";
var allowedOrigins = originsString.Split(',', StringSplitOptions.RemoveEmptyEntries);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        // Aplicar migrations automaticamente
        var context = scope.ServiceProvider.GetRequiredService<GeoDataDbContext>();
        Console.WriteLine("🔄 Applying database migrations...");
        await context.Database.MigrateAsync();
        Console.WriteLine("✅ Database migrations applied successfully");

        // Seed inicial
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
        Console.WriteLine("✅ Database seeded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Database initialization failed (service will still start): {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapControllers();
app.MapObservabilityHealthChecks();

app.Run();
