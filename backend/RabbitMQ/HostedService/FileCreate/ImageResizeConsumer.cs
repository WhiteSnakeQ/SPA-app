using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SPA_app.Constants;
using SPA_app.Hubs;
using SPA_app.RabbitMQ.Messages;
using SPA_app.Services.ImageS;
using SPA_приложение.Data;
using SPA_приложение.Models;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace SPA_app.RabbitMQ.HostedService.FileCreate
{
	public class ImageResizeConsumer : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly AppDbContext _appDbContext;

		private IConnection _connection;
		private readonly List<IModel> _channels = new();
		private readonly int _consumerCount = Queue.Files.ResizeImageCount;

		public ImageResizeConsumer(IServiceScopeFactory scopeFactory, IConnection connection)
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
						var body = ea.Body.ToArray();
						var json = Encoding.UTF8.GetString(body);
						var message = JsonSerializer.Deserialize<FileCreatedMessage>(json);
						using var scope = _scopeFactory.CreateScope();
						var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();

						await imageService.ResizeImage(message!.File.FileUrl, message.FileExt);

                        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<CommentsHub>>();
                        await hubContext.Clients.All.SendAsync("FileReady", message.File);

                        channel.BasicAck(ea.DeliveryTag, false);
					}
					catch (Exception ex)
					{
						channel.BasicNack(ea.DeliveryTag, false, true);
					}
				};

				channel.BasicConsume(queue: Queue.Files.ResizeImage, autoAck: false, consumer: consumer);
			}
			return Task.CompletedTask;
		}
		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_connection?.Dispose();

			return base.StopAsync(cancellationToken);
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
