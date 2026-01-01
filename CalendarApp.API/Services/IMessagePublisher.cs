namespace CalendarApp.API.Services;

public interface IMessagePublisher
{
    void PublishMessage<T>(string queueName, T message) where T : class;
}
