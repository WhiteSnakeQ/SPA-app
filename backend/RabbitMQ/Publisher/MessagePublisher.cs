using RabbitMQ.Client;
using SPA_app.Constants;
using System.Text;
using System.Text.Json;

namespace SPA_app.RabbitMQ.Publisher
{
    public class MessagePublisher : IMessagePublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessagePublisher()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Publish<T>(T message, string queueName)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: body);
        }
        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}
