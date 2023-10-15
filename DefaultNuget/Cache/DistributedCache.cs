using Newtonsoft.Json;
using DefaultNuget.Dto;
using DefaultNuget.Service;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace DefaultNuget.Cache
{
    public class DistributedCache<TEntity> : IDisposable, ICache<TEntity> where TEntity : DistributedBase
    {
        private DistributedCacheEntryOptions DistributedCacheEntryOptions { get; set; }
        private List<TEntity> ItemList { get; set; }
        private int ItemCount { get; set; }
        private readonly string key = typeof(TEntity).Name + "Cache";
        protected readonly IServiceScopeFactory serviceScopeFactory;

        public DistributedCache(IServiceScopeFactory _serviceScopeFactory)
        {
            serviceScopeFactory = _serviceScopeFactory;
        }

        public async Task<TEntity> TryGetSingleItemAsync()
        {
            List<TEntity> itemList = await TryGetListAsync();
            return await Task.FromResult(itemList?.FirstOrDefault());
        }

        public async Task<TEntity> TryGetItemAsync(Func<TEntity, bool> predicate)
        {
            List<TEntity> itemList = await TryGetListAsync();
            return await Task.FromResult(itemList?.FirstOrDefault(predicate));
        }

        public async Task<IEnumerable<TEntity>> TryGetItemListAsync(Func<TEntity, bool> predicate)
        {
            List<TEntity> itemList = await TryGetListAsync();
            return await Task.FromResult(itemList.Where(predicate));
        }

        public async Task<List<TEntity>> TryGetListAsync()
        {
            if (ItemList == null || !ItemList.Any())
            {
                ItemList = await GetList();
            }

            if (ItemList == null || DistributedCacheEntryOptions != null && DistributedCacheEntryOptions.AbsoluteExpiration < DateTime.Now)
            {
                await TryReloadCacheAsync();
                ItemList = await GetList(true);
            }

            return ItemList;
        }

        public async Task<List<TEntity>> GetList(bool isRecursed = false)
        {
            List<TEntity> itemList = null;

            using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
            {
                itemList = await serviceScope.ServiceProvider.GetRequiredService<IDistributedService>().GetCachedDataByKeyAsync<TEntity>(key);
            }

            if (itemList == null && isRecursed)
            {
                itemList = new();
            }

            return itemList;
        }

        public async Task TryPushItem(TEntity item)
        {
            await TryPushItems(new List<TEntity> { item });
        }

        public async Task TryPushItems(List<TEntity> items)
        {
            List<TEntity> itemList = await TryGetListAsync();
            itemList ??= new();
            DistributedCacheEntryOptions = SetPolicy();
            itemList.AddRange(items);
            await SetCacheList(itemList);
        }

        public virtual Task<List<TEntity>> GetEntityListAsync()
        {
            return Task.FromResult(new List<TEntity>());
        }

        public async Task TryReloadCacheAsync()
        {
            List<TEntity> temporaryItemList = await GetEntityListAsync();
            DistributedCacheEntryOptions = SetPolicy();

            if (!temporaryItemList.Any())
            {
                return;
            }

            await SetCacheList(temporaryItemList);
        }

        public int GetItemCount()
        {
            return ItemCount;
        }

        public Type GetCacheEntity()
        {
            return typeof(TEntity);
        }

        public object GetPolicy()
        {
            return DistributedCacheEntryOptions;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected async Task SetCacheList(List<TEntity> itemList)
        {
            using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
            {
                ItemCount = itemList == null ? ItemCount : itemList.Count;
                ItemList = null;

                serviceScope.ServiceProvider.GetRequiredService<IDistributedService>().SetCacheStringAsync(key, JsonConvert.SerializeObject(itemList), DistributedCacheEntryOptions);
            }
        }

        protected virtual DistributedCacheEntryOptions SetPolicy()
        {
            return new DistributedCacheEntryOptions();
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}