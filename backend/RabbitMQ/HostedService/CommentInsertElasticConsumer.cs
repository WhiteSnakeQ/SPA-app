using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SPA_app.Constants;
using SPA_app.RabbitMQ.Messages;
using SPA_app.Services.ElasticSearch;
using SPA_приложение.Data;
using SPA_приложение.Models;
using System.Text;
using System.Text.Json;

namespace SPA_app.RabbitMQ.HostedService
{
    public class CommentInsertElasticConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IModel? _channel;
        public CommentInsertElasticConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: QueueNames.IndexComment, durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<CommentIndexMessage>(json);
                    using var scope = _scopeFactory.CreateScope();

                    var searchService = scope.ServiceProvider.GetRequiredService<ICommentSearchService>();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var comment = await db.Comments.FirstOrDefaultAsync(x => x.Id == message!.CommentId);
                    if (comment != null)
                        await searchService.IndexComment(comment);

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(queue: QueueNames.IndexComment, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _channel?.Close();
            _connection?.Close();

            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();

            base.Dispose();
        }
    }
}
