namespace SPA_app.Queue
{
    public interface IBackgroundTaskQueue
    {
        Task Queue(Func<CancellationToken, Task> workItem);
        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken token);
    }
}