namespace DefaultNuget.Cache
{
    public interface ICache<TEntity> : ICacheable where TEntity : class
    {
        Task<TEntity> TryGetSingleItemAsync();
        Task<TEntity> TryGetItemAsync(Func<TEntity, bool> predicate);
        Task<IEnumerable<TEntity>> TryGetItemListAsync(Func<TEntity, bool> predicate);
        Task<List<TEntity>> TryGetListAsync();
        Task<List<TEntity>> GetList(bool isRecursed = false);
        Task TryPushItem(TEntity item);
        Task TryPushItems(List<TEntity> items);
        Task<List<TEntity>> GetEntityListAsync();
    }
}