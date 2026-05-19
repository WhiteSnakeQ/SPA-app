using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;
using SPA_app.Events.CommentCreated;
using SPA_app.Events.Interface;
using SPA_app.Queue;
using SPA_app.Services.ImageS;
using SPA_приложение.Enums;
using SPA_приложение.Exceptions;

namespace SPA_app.Events.FileUploaded
{
    public class FileUploadedHandler : IEventHandler<FileUploadedEvent>
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;

        public FileUploadedHandler(IBackgroundTaskQueue queue, IServiceScopeFactory scopeFactory)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
        }

        public Task Handle(FileUploadedEvent @event)
        {
            Console.WriteLine("EVENT HANDLE  FILEUPLOADED");

            if (@event.FileType == FileType.Image)
            {

                _queue.Queue(async token =>
                {
                    Console.WriteLine("EVENT Queue  FILEUPLOADED");

                    using var scope = _scopeFactory.CreateScope();

                    var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();

                    await imageService.ResizeImage(@event.FilePath, @event.FileType, @event.ext);
                });
            }

            return Task.CompletedTask;
        }
    }
}
