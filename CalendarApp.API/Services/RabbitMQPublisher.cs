using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Text;

namespace CalendarApp.API.Services;

public class RabbitMQPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMQPublisher> _logger;

    public RabbitMQPublisher(IConfiguration configuration, ILogger<RabbitMQPublisher> logger)
    {
        _logger = logger;

        try
        {
            var rabbitMQHost = configuration["RabbitMQ:Host"] ?? "localhost";
            var rabbitMQPort = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
            var rabbitMQUsername = configuration["RabbitMQ:Username"] ?? "guest";
            var rabbitMQPassword = configuration["RabbitMQ:Password"] ?? "guest";

            var factory = new ConnectionFactory
            {
                HostName = rabbitMQHost,
                Port = rabbitMQPort,
                UserName = rabbitMQUsername,
                Password = rabbitMQPassword,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = rabbitMQHost
                }
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ connection");
            throw;
        }
    }

    public void PublishMessage<T>(string queueName, T message) where T : class
    {
        try
        {
            // Declare queue (idempotent operation)
            _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            ).GetAwaiter().GetResult();

            var messageBody = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json"
            };

            _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body
            ).GetAwaiter().GetResult();

            _logger.LogInformation("Message published to queue {QueueName}: {MessageType}", queueName, typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to queue {QueueName}", queueName);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
