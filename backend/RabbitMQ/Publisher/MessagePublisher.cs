using RabbitMQ.Client;
using SPA_app.Constants;
using System.Text;
using System.Text.Json;

namespace SPA_app.RabbitMQ.Publisher
{
    public class RabbitMqPublisher : IMessagePublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqPublisher()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: QueueNames.ImageResize, durable: true, exclusive: false, autoDelete: false);
        }

        public void Publish<T>(T message)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: "", routingKey: QueueNames.ImageResize, basicProperties: properties, body: body);
        }
        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}
