using AngleSharp.Dom.Events;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SPA_app.Constants;
using SPA_app.Hubs;
using SPA_app.RabbitMQ.Messages;
using SPA_app.Services.CacheS;
using SPA_app.Services.ImageS;
using SPA_приложение.Data;
using SPA_приложение.Models;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace SPA_app.RabbitMQ.HostedService.CommentCreate
{
	public class CommentCreateCacheCleanConsumer : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private IConnection _connection;
        private readonly List<IModel> _channels = new();
        private readonly int _consumerCount = Queue.Comment.CacheCleanCount;

        public CommentCreateCacheCleanConsumer(IServiceScopeFactory scopeFactory, IConnection connection)
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

                channel.BasicQos(0, 10, false);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        var message = JsonSerializer.Deserialize<CommentCreatedMessage>(json);
                        using var scope = _scopeFactory.CreateScope();
                        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

                        var comment = message!.Comment;

                        var keysToRemove = new List<string>();
                        keysToRemove.AddRange(CacheKeys.FirstPageKeys());

                        if (comment.ParentId != null)
                        {
                            keysToRemove.Add(CacheKeys.ReplyCacheKey(comment.ParentId ?? 0));
                        }

                        await cacheService.RemoveManyAsync(keysToRemove);

                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };

                channel.BasicConsume(queue: Queue.Comment.CacheClean, autoAck: false, consumer: consumer);
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

