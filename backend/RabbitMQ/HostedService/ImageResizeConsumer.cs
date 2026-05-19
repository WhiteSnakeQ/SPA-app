using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SPA_app.Constants;
using SPA_app.RabbitMQ.Messages;
using SPA_app.Services.ImageS;
using System.Text;
using System.Text.Json;

namespace SPA_app.RabbitMQ.HostedService
{
    public class ImageResizeConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IModel? _channel;
        public ImageResizeConsumer(IServiceScopeFactory scopeFactory)
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

            _channel.QueueDeclare(queue: QueueNames.ImageResize, durable: true, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<ImageResizeMessage>(json);
                    using var scope = _scopeFactory.CreateScope();
                    var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();

                    await imageService.ResizeImage(message!.FullPath, message.FileExt);
                    Console.WriteLine("IMAGE RESIZE COMPLETE");

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(queue: QueueNames.ImageResize, autoAck: false, consumer: consumer);
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
