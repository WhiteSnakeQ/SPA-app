using Microsoft.AspNetCore.SignalR;
using SPA_app.Constants;
using SPA_app.Events.Interface;
using SPA_app.Hubs;
using SPA_app.Services.CacheS;

namespace SPA_app.Events.CommentCreated
{
    public sealed class CommentCreatedCacheClean : IEventHandler<CommentCreatedEvent>
    {
        private readonly ICacheService _cacheService;

        public CommentCreatedCacheClean(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task Handle(CommentCreatedEvent @event)
        {
            if (@event.Comment.ParentId == null)
            {
                foreach (var key in CacheKeys.FirstPageKeys())
                    await _cacheService.RemoveAsync(key);
            }
        }
    }
}
