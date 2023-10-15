using DefaultNuget.Dto;
using Microsoft.Extensions.Caching.Distributed;

namespace DefaultNuget.Service
{
    public interface IDistributedService
    {
        Task SetCacheAsync<TEntity>(string key, List<TEntity> itemList, DistributedCacheEntryOptions options) where TEntity : DistributedBase;
        Task SetCacheStringAsync(string key, string value, DistributedCacheEntryOptions options);
        Task<TEntity> GetCachedDataValueAsync<TEntity>(string key, string value) where TEntity : DistributedBase;
        Task<List<TEntity>> GetCachedDataByKeyAsync<TEntity>(string key) where TEntity : DistributedBase;
        Task<string> GetCachedString(string key);
        Task RemoveCacheAsync(string key);
    }
}