using DefaultNuget.Entity;
using DefaultNuget.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DefaultNuget.UnitOfWork
{
    public class UnitOfWork<T> : IUnitOfWork<T> where T : DbContext
    {
        private IDbContextTransaction transaction;
        private readonly T context;

        public UnitOfWork(T _context)
        {
            context = _context;
        }

        public IRepository<TRepo, T> GetRepository<TRepo>() where TRepo : Base
        {
            return new Repository<TRepo, T>(context);
        }

        public async Task BeginTransactionAsync()
        {
            if (IsInTransaction())
            {
                await RollbackAsync();
            }

            transaction = await context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (IsInTransaction())
            {
                await transaction.CommitAsync();
                await transaction.DisposeAsync();
                transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (IsInTransaction())
            {
                await transaction.RollbackAsync();
                await transaction.DisposeAsync();
                transaction = null;
            }
        }

        private bool IsInTransaction()
        {
            if (transaction == null)
            {
                return false;
            }

            return transaction.TransactionId != Guid.Empty;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction.Dispose();
                transaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}