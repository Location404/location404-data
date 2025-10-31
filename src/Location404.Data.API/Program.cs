using Location404.Data.Infrastructure;
using Location404.Data.Infrastructure.Data;
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
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
        Console.WriteLine("✅ Database seeded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Database seeding failed (service will still start): {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "geo-data-service",
    timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck")
.WithTags("Health");

app.Run();
