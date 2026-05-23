namespace SPA_app.Services.CacheS
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);

        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        Task RemoveAsync(string key);
        Task RemoveManyAsync(IEnumerable<string> cacheKeys);
    }
}
