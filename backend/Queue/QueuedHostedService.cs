using SPA_app.Queue;

namespace SPA_приложение.Queue;

public sealed class QueuedHostedService
    : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;

    public QueuedHostedService(IBackgroundTaskQueue queue)
    {
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync(stoppingToken);

            await workItem(stoppingToken);
        }
    }
}