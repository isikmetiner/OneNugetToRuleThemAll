namespace DefaultNuget.Cache
{
    public interface ICacheable
    {
        Task TryReloadCacheAsync();
        int GetItemCount();
        Type GetCacheEntity();
        object GetPolicy();
    }
}