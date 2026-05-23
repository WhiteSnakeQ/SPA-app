using SPA_app.BackGroundChannel.Comments;
using SPA_app.Constants;
using SPA_app.RabbitMQ.Messages;
using SPA_app.RabbitMQ.Publisher;
using SPA_app.Services.FileS;
using System.Threading.Channels;

namespace SPA_app.BackGroundChannel.Files
{
    public class FileWorker : BackgroundService
    {
        private readonly IMessagePublisher _publisher;
        private readonly Channel<FileCreatedMessage> _channel;

        public FileWorker(IMessagePublisher publisher, Channel<FileCreatedMessage> channel)
        {
            _publisher = publisher;
            _channel = channel;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var reader = _channel.Reader;

            var workers = Enumerable.Range(0, WorkersCount.FileWorkers)
                .Select(async _ =>
                {
                    await foreach (var msg in reader.ReadAllAsync(stoppingToken))
                    {
                        _publisher.Publish(msg, Queue.Files.ExchangeName);
                    }
                });

            return Task.WhenAll(workers);
        }
    }
}
