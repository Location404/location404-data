using Location404.Data.Application.Common.Interfaces;
using Location404.Data.Application.DTOs.Events;
using Location404.Data.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Location404.Data.Infrastructure.Messaging;

public class MatchConsumerService : BackgroundService
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<MatchConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly JsonSerializerOptions _jsonOptions;

    public MatchConsumerService(
        IOptions<RabbitMQSettings> options,
        ILogger<MatchConsumerService> logger,
        IServiceProvider serviceProvider)
    {
        _settings = options.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("RabbitMQ consumer is disabled");
            return;
        }

        _logger.LogInformation("Starting RabbitMQ consumer service");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnsureConnectionAsync(stoppingToken);

                if (_channel != null)
                {
                    await StartConsumingAsync(stoppingToken);
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("RabbitMQ consumer service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RabbitMQ consumer, retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            return;

        try
        {
            _logger.LogInformation("Connecting to RabbitMQ at {HostName}:{Port}", _settings.HostName, _settings.Port);

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            factory.Ssl.Enabled = false;

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.ExchangeDeclareAsync(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken
            );

            await _channel.QueueDeclareAsync(
                queue: _settings.MatchEndedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken
            );

            await _channel.QueueBindAsync(
                queue: _settings.MatchEndedQueue,
                exchange: _settings.ExchangeName,
                routingKey: "match.ended",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Successfully connected to RabbitMQ and declared queue {Queue}", _settings.MatchEndedQueue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
        }
    }

    private async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        if (_channel == null)
            return;

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            // v7: MUST copy body data immediately (memory is library-owned)
            var body = ea.Body.ToArray();
            IServiceScope? scope = null;

            try
            {
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Received message from RabbitMQ: {RoutingKey}", ea.RoutingKey);

                var eventDto = JsonSerializer.Deserialize<GameMatchEndedEventDto>(message, _jsonOptions);

                if (eventDto != null)
                {
                    try
                    {
                        scope = _serviceProvider.CreateScope();
                    }
                    catch (ObjectDisposedException)
                    {
                        _logger.LogWarning("ServiceProvider disposed, cannot process message. Rejecting without requeue.");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        return;
                    }

                    var matchService = scope.ServiceProvider.GetRequiredService<IMatchService>();
                    await matchService.ProcessMatchEndedEventAsync(eventDto);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogInformation("Successfully processed match {MatchId}", eventDto.MatchId);
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize message, sending NACK without requeue");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message, sending NACK without requeue");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
            }
            finally
            {
                scope?.Dispose();
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _settings.MatchEndedQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Started consuming from queue {Queue}", _settings.MatchEndedQueue);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ consumer");

        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
            _channel.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
            _connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
