using SPA_app.Constants;
using SPA_app.RabbitMQ.Messages;
using SPA_app.RabbitMQ.Publisher;
using System.Threading.Channels;

namespace SPA_app.BackGroundChannel.Comments
{
	public class CommentWorker : BackgroundService
	{
		private readonly IMessagePublisher _publisher;
		private readonly Channel<CommentCreatedMessage> _channel;

		public CommentWorker(IMessagePublisher publisher, Channel<CommentCreatedMessage> channel)
		{
			_publisher = publisher;
			_channel = channel;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var reader = _channel.Reader;

			var workers = Enumerable.Range(0, WorkersCount.CommentWorkers)
				.Select(async _ =>
				{
					await foreach (var msg in reader.ReadAllAsync(stoppingToken))
					{
						_publisher.Publish(msg, Queue.Comment.ExchangeName);
					}
				});

			return Task.WhenAll(workers);
		}
	}
}
