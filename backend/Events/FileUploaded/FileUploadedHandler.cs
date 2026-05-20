using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;
using SPA_app.Constants;
using SPA_app.Events.CommentCreated;
using SPA_app.Events.Interface;
using SPA_app.RabbitMQ.Messages;
using SPA_app.RabbitMQ.Publisher;
using SPA_app.Services.ImageS;
using SPA_приложение.Enums;
using SPA_приложение.Exceptions;

namespace SPA_app.Events.FileUploaded
{
    public class FileUploadedHandler : IEventHandler<FileUploadedEvent>
    {
        private readonly IMessagePublisher _publisher;
        public FileUploadedHandler(IMessagePublisher publisher)
        {
            _publisher = publisher;
        }

        public Task Handle(FileUploadedEvent @event)
        {
            if (@event.FileType == FileType.Image)
            {

                _publisher.Publish(new ImageResizeMessage
                {
                    FullPath = @event.FilePath,
                    FileExt = @event.Ext
                }, QueueNames.ImageResize);
            }
            return Task.CompletedTask;
        }
    }
}
