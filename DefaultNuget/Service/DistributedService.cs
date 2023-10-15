using Newtonsoft.Json;
using DefaultNuget.Dto;
using Microsoft.Extensions.Caching.Distributed;

namespace DefaultNuget.Service
{
    public class DistributedService : IDistributedService
    {
        private readonly IDistributedCache distributedCache;

        public DistributedService(IDistributedCache _distributedCache)
        {
            distributedCache = _distributedCache;
        }

        public async Task SetCacheAsync<TEntity>(string key, List<TEntity> itemList, DistributedCacheEntryOptions options) where TEntity : DistributedBase
        {
            if (itemList != null)
            {
                string serializedItems = JsonConvert.SerializeObject(itemList);
                await distributedCache.SetStringAsync($"MyProjectName_{key}", serializedItems, options ?? new());
            }
        }

        public async Task SetCacheStringAsync(string key, string value, DistributedCacheEntryOptions options)
        {
            await distributedCache.SetStringAsync($"MyProjectName_{key}", value, options ?? new());
        }

        public async Task<TEntity> GetCachedDataValueAsync<TEntity>(string key, string value) where TEntity : DistributedBase
        {
            List<TEntity> itemList = await GetCachedDataByKeyAsync<TEntity>(key);
            return itemList?.FirstOrDefault(x => x.Key == value);
        }

        public async Task<List<TEntity>> GetCachedDataByKeyAsync<TEntity>(string key) where TEntity : DistributedBase
        {
            List<TEntity> itemList = default;
            string response = await distributedCache.GetStringAsync($"MyProjectName_{key}");

            if (!string.IsNullOrEmpty(response))
            {
                itemList = JsonConvert.DeserializeObject<List<TEntity>>(response);
            }

            return itemList;
        }

        public async Task<string> GetCachedString(string key)
        {
            return await distributedCache.GetStringAsync($"MyProjectName_{key}");
        }

        public async Task RemoveCacheAsync(string key)
        {
            await distributedCache.RemoveAsync($"MyProjectName_{key}");
        }
    }
}