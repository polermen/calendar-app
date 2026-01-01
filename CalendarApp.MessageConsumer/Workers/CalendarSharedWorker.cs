using CalendarApp.MessageConsumer.Models;
using CalendarApp.MessageConsumer.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace CalendarApp.MessageConsumer.Workers;

public class CalendarSharedWorker : BackgroundService
{
    private readonly ILogger<CalendarSharedWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string QueueName = "calendar-shared";

    public CalendarSharedWorker(
        ILogger<CalendarSharedWorker> logger,
        IConfiguration configuration,
        EmailService emailService)
    {
        _logger = logger;
        _configuration = configuration;
        _emailService = emailService;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var rabbitMQHost = _configuration["RabbitMQ:Host"] ?? "localhost";
            var rabbitMQPort = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672");
            var rabbitMQUsername = _configuration["RabbitMQ:Username"] ?? "guest";
            var rabbitMQPassword = _configuration["RabbitMQ:Password"] ?? "guest";

            var factory = new ConnectionFactory
            {
                HostName = rabbitMQHost,
                Port = rabbitMQPort,
                UserName = rabbitMQUsername,
                Password = rabbitMQPassword,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("CalendarSharedWorker connected to RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            _logger.LogWarning("Channel is null, cannot start consuming messages");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonConvert.DeserializeObject<CalendarSharedMessage>(messageJson);

                if (message != null)
                {
                    _logger.LogInformation("Processing calendar share from {Owner} to {Spectator}",
                        message.OwnerUsername, message.SpectatorEmail);

                    await _emailService.SendCalendarSharedEmailAsync(
                        message.SpectatorEmail,
                        message.OwnerUsername,
                        message.OwnerEmail
                    );

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogInformation("Calendar shared email sent successfully to {Email}", message.SpectatorEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing calendar shared message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await _channel.BasicConsumeAsync(QueueName, false, consumer, stoppingToken);

        _logger.LogInformation("CalendarSharedWorker started consuming messages");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CalendarSharedWorker stopping");

        if (_channel != null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection != null)
            await _connection.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
