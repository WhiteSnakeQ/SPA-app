using Microsoft.AspNetCore.SignalR;
using SPA_app.Events.Interface;
using SPA_app.Hubs;

namespace SPA_app.Events.CommentCreated
{
    public sealed class CommentCreatedSignalRHandler : IEventHandler<CommentCreatedEvent>
    {
        private readonly IHubContext<CommentsHub> _hub;

        public CommentCreatedSignalRHandler(IHubContext<CommentsHub> hub)
        {
            _hub = hub;
        }

        public async Task Handle(CommentCreatedEvent @event)
        {
            if (@event.Comment.ParentId == null)
                await _hub.Clients.All.SendAsync("CommentCreated", @event.Comment);
            else
                await _hub.Clients.All.SendAsync("ReplyCreated", @event.Comment);
        }
    }
}
