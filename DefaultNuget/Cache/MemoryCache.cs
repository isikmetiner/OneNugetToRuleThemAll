using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace DefaultNuget.Cache
{
    public class MemoryCache<TEntity> : IDisposable, ICache<TEntity> where TEntity : class
    {
        private MemoryCacheEntryOptions MemoryCacheEntryOptions { get; set; }
        private readonly object cacheLock = new();
        private readonly string key = typeof(TEntity).Name + "Cache";
        private readonly IMemoryCache memoryCache;
        protected readonly IServiceScopeFactory serviceScopeFactory;

        public MemoryCache(IMemoryCache _memoryCache, IServiceScopeFactory _serviceScopeFactory)
        {
            serviceScopeFactory = _serviceScopeFactory;
            memoryCache = _memoryCache;
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
            return await Task.FromResult(itemList?.Where(predicate));
        }

        public async Task<List<TEntity>> TryGetListAsync()
        {
            List<TEntity> itemList = await GetList();
            if (itemList == null || MemoryCacheEntryOptions != null && MemoryCacheEntryOptions.AbsoluteExpiration < DateTime.Now)
            {
                await TryReloadCacheAsync();
                itemList = await GetList(true);
            }

            return itemList;
        }

        public Task<List<TEntity>> GetList(bool isRecursed = false)
        {
            List<TEntity> itemList = (List<TEntity>)memoryCache.Get(key);
            if (itemList == null && isRecursed)
            {
                itemList = new();
            }

            return Task.FromResult(itemList);
        }

        public async Task TryPushItem(TEntity item)
        {
            await TryPushItems(new List<TEntity>() { item });
        }

        public async Task TryPushItems(List<TEntity> items)
        {
            List<TEntity> itemList = await TryGetListAsync();
            itemList ??= new();
            itemList.AddRange(items);
            SetCacheList(itemList);
        }

        public virtual Task<List<TEntity>> GetEntityListAsync()
        {
            return Task.FromResult(new List<TEntity>());
        }

        public async Task TryReloadCacheAsync()
        {
            List<TEntity> temporaryItemList = await GetEntityListAsync();
            MemoryCacheEntryOptions = SetPolicy();

            if (!temporaryItemList.Any())
            {
                return;
            }

            SetCacheList(temporaryItemList);
        }

        public int GetItemCount()
        {
            throw new NotImplementedException();
        }

        public Type GetCacheEntity()
        {
            return typeof(TEntity);
        }

        public object GetPolicy()
        {
            return MemoryCacheEntryOptions;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void SetCacheList(List<TEntity> itemList)
        {
            lock (cacheLock)
            {
                memoryCache.Set(key, itemList, MemoryCacheEntryOptions);
            }
        }

        protected virtual MemoryCacheEntryOptions SetPolicy()
        {
            return null;
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}