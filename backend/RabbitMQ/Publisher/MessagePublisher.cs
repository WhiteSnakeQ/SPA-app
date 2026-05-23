using RabbitMQ.Client;
using SPA_app.Constants;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace SPA_app.RabbitMQ.Publisher
{
	public class MessagePublisher : IMessagePublisher, IDisposable
	{
		private readonly IConnection _connection;
		private IModel _channel;

		public MessagePublisher(IConnection connection)
		{
			_connection = connection;
            _channel = _connection.CreateModel();
        }

		public void Publish<T>(T message, string exchangeName, string routingKey = "")
		{
			var json = JsonSerializer.Serialize(message);
			var body = Encoding.UTF8.GetBytes(json);

			var props = _channel.CreateBasicProperties();
			props.Persistent = true;

            _channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: props, body: body);
		}

		public void Dispose()
		{
			_channel?.Dispose();
			_connection?.Dispose();
		}
	}
}
