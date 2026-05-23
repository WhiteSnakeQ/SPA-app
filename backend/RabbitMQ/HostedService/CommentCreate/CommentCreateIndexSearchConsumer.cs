using AngleSharp.Dom;
using AngleSharp.Dom.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SPA_app.Constants;
using SPA_app.Models.ElasticSearchDocuments;
using SPA_app.RabbitMQ.Messages;
using SPA_app.Services.ElasticSearch;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace SPA_app.RabbitMQ.HostedService.CommentCreate
{
	public class CommentCreateIndexSearchConsumer : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private IConnection _connection;
        private readonly List<IModel> _channels = new();
        private readonly Channel<CommentCreatedMessage> _batchChannel;
        private readonly int _consumerCount = Queue.Comment.IndexSearchCount;

        public CommentCreateIndexSearchConsumer(IConnection connection, IServiceScopeFactory scopeFactory, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _connection = connection;
            _consumerCount = config.GetValue("SearchConsumerCount", 2);

            _batchChannel = Channel.CreateBounded<CommentCreatedMessage>(
                new BoundedChannelOptions(1000)
                {
                    FullMode = BoundedChannelFullMode.Wait
                });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            for (int i = 0; i < _consumerCount; i++)
            {
                StartRabbitConsumer(stoppingToken);
            }


            var batcherTasks = new List<Task>();
            for (int i = 0; i < 3; i++)  // 3 параллельных батчера
            {
                batcherTasks.Add(RunBatcher(stoppingToken));
            }

            await Task.WhenAll(batcherTasks);
        }

        private void StartRabbitConsumer(CancellationToken stoppingToken)
        {
            var channel = _connection.CreateModel();
            _channels.Add(channel);

            channel.BasicQos(0, 50, false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<CommentCreatedMessage>(json);

                    await _batchChannel.Writer.WriteAsync(message, stoppingToken);

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            channel.BasicConsume(queue: Queue.Comment.IndexSearch, autoAck: false, consumer: consumer);
        }

        private async Task RunBatcher(CancellationToken stoppingToken)
        {
            var batch = new List<CommentCreatedMessage>(Queue.Comment.IndexSearchInsertAtOnce);

            while (!stoppingToken.IsCancellationRequested)
            {
                batch.Clear();

                while (batch.Count < Queue.Comment.IndexSearchInsertAtOnce && !stoppingToken.IsCancellationRequested)
                {
                    if (_batchChannel.Reader.TryRead(out var message))
                    {
                        batch.Add(message);
                    }
                    else
                    {
                        var timerTask = Task.Delay(100, stoppingToken);
                        var waitTask = _batchChannel.Reader.WaitToReadAsync(stoppingToken).AsTask();

                        var completed = await Task.WhenAny(waitTask, timerTask);

                        if (completed == timerTask)
                        {
                            break;
                        }
                        else
                        {
                            if (await waitTask)
                                continue;
                        }
                    }
                }

                if (batch.Count > 0)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var searchService = scope.ServiceProvider.GetRequiredService<ICommentSearchService>();
                    List<CommentSearchDocument> documents = [];

                    foreach (var item in batch)
                    {
                        documents.Add(new CommentSearchDocument(item.Comment.Id,item.Comment.UserName, item.Comment.Email, item.Comment.Text));
                    }

                    await searchService.BulkIndexComments(documents);
                }
            }
        }
     
		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_connection?.Close();

			return base.StopAsync(cancellationToken);
		}

		public override void Dispose()
		{
			_connection?.Dispose();

			base.Dispose();
		}
	}
}
