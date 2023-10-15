using DefaultNuget.Entity;
using DefaultNuget.Repository;
using Microsoft.EntityFrameworkCore;

namespace DefaultNuget.UnitOfWork
{
    public interface IUnitOfWork<T> : IDisposable where T : DbContext
    {
        IRepository<TRepo, T> GetRepository<TRepo>() where TRepo : Base;
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}