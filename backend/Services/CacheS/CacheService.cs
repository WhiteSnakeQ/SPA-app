using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SPA_app.Services.CacheS
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string cacheKey)
        {
            var cached = await _cache.GetStringAsync(cacheKey);

            if (cached is null)
                return default;

            return JsonSerializer.Deserialize<T>(cached);
        }

        public async Task SetAsync<T>(string cacheKey, T obj, TimeSpan? expiration)
        {
            var json = JsonSerializer.Serialize(obj);

            await _cache.SetStringAsync(
                cacheKey,
                json,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        expiration ??
                        TimeSpan.FromMinutes(5)
                });
        }

        public async Task RemoveAsync(string cacheKey)
        {
            await _cache.RemoveAsync(cacheKey);
        }
        public async Task RemoveManyAsync(IEnumerable<string> cacheKeys)
        {
            var keys = cacheKeys.ToList();
            var batches = keys.Chunk(10);

            foreach (var batch in batches)
            {
                await Task.WhenAll(batch.Select(key => _cache.RemoveAsync(key)));
            }
        }
    }
}
