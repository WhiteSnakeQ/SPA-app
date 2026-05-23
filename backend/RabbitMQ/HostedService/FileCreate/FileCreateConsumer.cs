using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SPA_app.Constants;
using SPA_app.RabbitMQ.Messages;
using SPA_app.Services.ImageS;
using SPA_приложение.Data;
using SPA_приложение.Models;
using System.Text;
using System.Text.Json;

namespace SPA_app.RabbitMQ.HostedService.FileCreate
{
	public class FileCreateConsumer : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;

		private IConnection _connection;
		private readonly List<IModel> _channels = new();
		private readonly int _consumerCount = Queue.Files.FileCreatedCount;

		public FileCreateConsumer(IServiceScopeFactory scopeFactory, IConnection connection)
		{
			_scopeFactory = scopeFactory;
			_connection = connection;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			for (int i = 0; i < _consumerCount; i++)
			{
				var channel = _connection.CreateModel();
				_channels.Add(channel);

				channel.BasicQos(0, 5, false);

				var consumer = new AsyncEventingBasicConsumer(channel);
				consumer.Received += async (model, ea) =>
				{
					try
					{
						using var scope = _scopeFactory.CreateScope();
						var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

						var body = ea.Body.ToArray();
						var json = Encoding.UTF8.GetString(body);
						var message = JsonSerializer.Deserialize<FileCreatedMessage>(json);

						var file = new CommentFile(message.File.CommentId, message.File.FileUrl, message.File.FileName, message.File.Size, null, message.File.Type);

						await appDbContext.CommentsFiles.AddAsync(file);
						await appDbContext.SaveChangesAsync();

						channel.BasicAck(ea.DeliveryTag, false);
					}
                    catch (Exception ex)
                    {
                        channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };

				channel.BasicConsume(queue: Queue.Files.FileCreated, autoAck: false, consumer: consumer);
			}

			return Task.CompletedTask;
		}

        public override void Dispose()
        {
            foreach (var channel in _channels)
            {
                if (channel.IsOpen)
                    channel.Close();
                channel?.Dispose();
            }
            _channels.Clear();

            _connection?.Dispose();
            base.Dispose();
        }
    }
}
