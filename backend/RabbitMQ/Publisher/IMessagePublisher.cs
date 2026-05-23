namespace SPA_app.RabbitMQ.Publisher
{
    public interface IMessagePublisher
    {
        void Publish<T>(T message, string exchangeName, string routingKey = "");
    }
}
