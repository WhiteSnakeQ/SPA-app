using SPA_app.Queue;

namespace SPA_приложение.Queue;

public sealed class QueuedHostedService : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;

    public QueuedHostedService(IBackgroundTaskQueue queue)
    {
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("EVENT ExecuteAsync  FILEUPLOADED");
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("EVENT WAITING TASK  FILEUPLOADED");

            try
            {
                var workItem = await _queue.DequeueAsync(stoppingToken);
                Console.WriteLine("TASK RECEIVED");
                await workItem(stoppingToken);
                Console.WriteLine("TASK COMPLETE");
            }
            catch (Exception ex)
            {

            }
        }
    }
}