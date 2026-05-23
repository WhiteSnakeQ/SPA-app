using AngleSharp.Dom.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SPA_app.Constants;
using SPA_app.Hubs;
using SPA_app.RabbitMQ.Messages;
using System.Text;
using System.Text.Json;

namespace SPA_app.RabbitMQ.HostedService.CommentCreate
{
	public class CommentCreateSignalRConsumer : BackgroundService
	{
		private readonly IHubContext<CommentsHub> _hub;
		private IConnection _connection;
		private IModel _channel;

        private readonly string QueueName = Queue.Comment.SignalR;

        public CommentCreateSignalRConsumer(IHubContext<CommentsHub> hub, IConnection connection)
		{
			_hub = hub;
			_connection = connection;

            _channel = _connection.CreateModel();

            _channel.BasicQos(0, 10, false);
        }

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
            
            var consumer = new AsyncEventingBasicConsumer(_channel);

			consumer.Received += async (model, ea) =>
			{
				try
				{
					var body = ea.Body.ToArray();
					var json = Encoding.UTF8.GetString(body);
					var message = JsonSerializer.Deserialize<CommentCreatedMessage>(json);

					var comment = message!.Comment;

					if (comment.ParentId == null)
						await _hub.Clients.All.SendAsync("CommentCreated", comment);
					else
						await _hub.Clients.All.SendAsync("ReplyCreated", comment);

                    _channel.BasicAck(ea.DeliveryTag, false);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
                    _channel.BasicNack(ea.DeliveryTag, false, false);
				}
			};

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
			return Task.CompletedTask;
		}

		public override void Dispose()
		{
			_connection?.Dispose();

			base.Dispose();
		}
	}
}
