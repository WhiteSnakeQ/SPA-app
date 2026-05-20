using SPA_app.Constants;
using SPA_app.Events.Interface;
using SPA_app.RabbitMQ.Messages;
using SPA_app.RabbitMQ.Publisher;
using SPA_app.Services.CacheS;
using SPA_приложение.Models;

namespace SPA_app.Events.CommentCreated
{
    public class CommentCreatedHandler : IEventHandler<CommentCreatedEvent>
    {
        private readonly IMessagePublisher _messagePublisher;

        public CommentCreatedHandler(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        public Task Handle(CommentCreatedEvent @event)
        {
            _messagePublisher.Publish(new CommentIndexMessage
            {
                CommentId = @event.Comment.Id
            }, QueueNames.IndexComment);

            return Task.CompletedTask;
        }
    }
}
