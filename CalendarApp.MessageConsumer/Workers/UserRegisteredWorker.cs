using CalendarApp.MessageConsumer.Models;
using CalendarApp.MessageConsumer.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace CalendarApp.MessageConsumer.Workers;

public class UserRegisteredWorker : BackgroundService
{
    private readonly ILogger<UserRegisteredWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string QueueName = "user-registered";

    public UserRegisteredWorker(
        ILogger<UserRegisteredWorker> logger,
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
            // Debug: Log all environment variables containing RabbitMQ
            var allEnvVars = Environment.GetEnvironmentVariables();
            _logger.LogInformation("[DEBUG] Environment variables containing 'RabbitMQ':");
            foreach (var key in allEnvVars.Keys)
            {
                if (key.ToString().Contains("RabbitMQ", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("[DEBUG ENV] {Key} = {Value}", key, allEnvVars[key]);
                }
            }

            // Try Railway environment variable format first, then fall back to colon notation
            var rabbitMQHost = Environment.GetEnvironmentVariable("RabbitMQ__Host")
                ?? _configuration["RabbitMQ:Host"] ?? "localhost";
            var rabbitMQPort = int.Parse(Environment.GetEnvironmentVariable("RabbitMQ__Port")
                ?? _configuration["RabbitMQ:Port"] ?? "5672");
            var rabbitMQUsername = Environment.GetEnvironmentVariable("RabbitMQ__Username")
                ?? _configuration["RabbitMQ:Username"] ?? "guest";
            var rabbitMQPassword = Environment.GetEnvironmentVariable("RabbitMQ__Password")
                ?? _configuration["RabbitMQ:Password"] ?? "guest";
            var rabbitMQVirtualHost = Environment.GetEnvironmentVariable("RabbitMQ__VirtualHost")
                ?? _configuration["RabbitMQ:VirtualHost"] ?? "/";

            _logger.LogInformation("[DEBUG] RabbitMQ Config - Host: {Host}, Port: {Port}, VirtualHost: {VHost}, Username: {User}",
                rabbitMQHost, rabbitMQPort, rabbitMQVirtualHost, rabbitMQUsername);

            var factory = new ConnectionFactory
            {
                HostName = rabbitMQHost,
                Port = rabbitMQPort,
                UserName = rabbitMQUsername,
                Password = rabbitMQPassword,
                VirtualHost = rabbitMQVirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = rabbitMQHost
                }
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("UserRegisteredWorker connected to RabbitMQ");
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
                var message = JsonConvert.DeserializeObject<UserRegisteredMessage>(messageJson);

                if (message != null)
                {
                    _logger.LogInformation("Processing user registration for {Username} ({Email})",
                        message.Username, message.Email);

                    await _emailService.SendWelcomeEmailAsync(message.Email, message.Username);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    _logger.LogInformation("Welcome email sent successfully to {Email}", message.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user registered message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await _channel.BasicConsumeAsync(QueueName, false, consumer, stoppingToken);

        _logger.LogInformation("UserRegisteredWorker started consuming messages");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UserRegisteredWorker stopping");

        if (_channel != null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection != null)
            await _connection.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
