using Microsoft.AspNetCore.SignalR;
using SPA_app.Constants;
using SPA_app.Events.Interface;
using SPA_app.Hubs;
using SPA_app.Services.CacheS;

namespace SPA_app.Events.CommentCreated
{
    public sealed class CommentCreatedCacheCleanHandler : IEventHandler<CommentCreatedEvent>
    {
        private readonly ICacheService _cacheService;

        public CommentCreatedCacheCleanHandler(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task Handle(CommentCreatedEvent @event)
        {
            foreach (var key in CacheKeys.FirstPageKeys())
                await _cacheService.RemoveAsync(key);
            if (@event.Comment.ParentId != null)
            {
                string cacheKey = CacheKeys.ReplyCacheKey(@event.Comment.ParentId ?? 0);
                await _cacheService.RemoveAsync(cacheKey);
            }
        }
    }
}
