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
    private IModel? _channel;
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
                EnsureConnection();

                if (_channel != null)
                {
                    StartConsuming();
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

    private void EnsureConnection()
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
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(5),
                SocketReadTimeout = TimeSpan.FromSeconds(5),
                SocketWriteTimeout = TimeSpan.FromSeconds(5)
            };

            // Disable SSL for non-SSL RabbitMQ server
            factory.Ssl.Enabled = false;

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
            );

            _channel.QueueDeclare(
                queue: _settings.MatchEndedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            _channel.QueueBind(
                queue: _settings.MatchEndedQueue,
                exchange: _settings.ExchangeName,
                routingKey: "match.ended"
            );

            _logger.LogInformation("Successfully connected to RabbitMQ and declared queue {Queue}", _settings.MatchEndedQueue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
        }
    }

    private void StartConsuming()
    {
        if (_channel == null)
            return;

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            IServiceScope? scope = null;
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Received message from RabbitMQ: {RoutingKey}", ea.RoutingKey);

                var eventDto = JsonSerializer.Deserialize<GameMatchEndedEventDto>(message, _jsonOptions);

                if (eventDto != null)
                {
                    // Create scope - check if provider is not disposed
                    try
                    {
                        scope = _serviceProvider.CreateScope();
                    }
                    catch (ObjectDisposedException)
                    {
                        _logger.LogWarning("ServiceProvider disposed, cannot process message. Rejecting without requeue.");
                        _channel?.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    var matchService = scope.ServiceProvider.GetRequiredService<IMatchService>();

                    await matchService.ProcessMatchEndedEventAsync(eventDto);

                    _channel?.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("Successfully processed match {MatchId}", eventDto.MatchId);
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize message, sending NACK without requeue");
                    _channel?.BasicNack(ea.DeliveryTag, false, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message, sending NACK without requeue");
                _channel?.BasicNack(ea.DeliveryTag, false, false); // Don't requeue to avoid infinite loop
            }
            finally
            {
                scope?.Dispose();
            }
        };

        _channel.BasicConsume(
            queue: _settings.MatchEndedQueue,
            autoAck: false,
            consumer: consumer
        );

        _logger.LogInformation("Started consuming from queue {Queue}", _settings.MatchEndedQueue);
    }

    public override void Dispose()
    {
        _logger.LogInformation("Disposing RabbitMQ consumer");

        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection");
        }

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
