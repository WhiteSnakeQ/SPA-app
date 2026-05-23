using RabbitMQ.Client;
using SPA_app.Constants;

namespace SPA_app.Extensions.Init
{
	public class RabbitMQInit
	{
		private readonly IConnection _connection;

		public RabbitMQInit(IConnection connection)
		{
			_connection = connection;
		}

		public void Init()
		{
			using var channel = _connection.CreateModel();

			channel.ExchangeDeclare(Queue.Comment.ExchangeName, ExchangeType.Fanout, durable: true);

			channel.QueueDeclare(Queue.Comment.SignalR, durable: true, exclusive: false, autoDelete: false);
			channel.QueueDeclare(Queue.Comment.CacheClean, durable: true, false, false);
			channel.QueueDeclare(Queue.Comment.IndexSearch, durable: true, false, false);

			channel.QueueBind(Queue.Comment.SignalR, Queue.Comment.ExchangeName, "");
			channel.QueueBind(Queue.Comment.CacheClean, Queue.Comment.ExchangeName, "");
			channel.QueueBind(Queue.Comment.IndexSearch, Queue.Comment.ExchangeName, "");

			channel.ExchangeDeclare(Queue.Files.ExchangeName, ExchangeType.Fanout, durable: true);

			channel.QueueDeclare(Queue.Files.ResizeImage, durable: true, false, false);
			channel.QueueDeclare(Queue.Files.FileCreated, durable: true, false, false);

			channel.QueueBind(Queue.Files.ResizeImage, Queue.Files.ExchangeName, "");
            channel.QueueBind(Queue.Files.FileCreated, Queue.Files.ExchangeName, "");
        }
	}
}
