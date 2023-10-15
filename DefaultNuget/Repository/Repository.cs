using DefaultNuget.Enum;
using DefaultNuget.Validator;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DefaultNuget.Repository
{
    public class Repository<T, TContext> : IRepository<T, TContext> where T : class where TContext : DbContext
    {
        public TContext context { get; }

        public Repository(TContext _context)
        {
            Guard.NotNull(_context, nameof(DbContext));
            context = _context;
        }

        public async Task<bool> TryInsertItemAsync(T item)
        {
            await context.Set<T>().AddAsync(item);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> TryInsertItemsAsync(ICollection<T> items) 
        { 
            await context.Set<T>().AddRangeAsync(items);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> TryUpdateItemAsync(T item)
        {
            context.Set<T>().Update(item);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> TryUpdateItemsAsync(ICollection<T> items)
        {
            context.Set<T>().UpdateRange(items);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> TryDeleteItemAsync(T item)
        {
            context.Set<T>().Remove(item);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> TryDeleteItemsAsync(ICollection<T> items)
        {
            context.Set<T>().RemoveRange(items);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<T> GetItemAsync(Expression<Func<T, bool>> predicate, TransactionType transactionType = TransactionType.Execute, params string[] includeProps)
        {
            IQueryable<T> queryable = context.Set<T>();

            if (transactionType == TransactionType.ReadOnly)
            {
                queryable = queryable.AsNoTracking();
            }

            if (includeProps != null && includeProps.Length > 0)
            {
                foreach (var item in includeProps)
                {
                    queryable = queryable.Include(item);
                }
            }

            return await queryable.FirstOrDefaultAsync(predicate);
        }

        public async Task<IQueryable<T>> GetFilteredItemsAsync(Expression<Func<T, bool>> predicate, TransactionType transactionType = TransactionType.Execute, params string[] includeProps)
        {
            IQueryable<T> queryable = context.Set<T>();

            if (transactionType == TransactionType.ReadOnly)
            {
                queryable = queryable.AsNoTracking();
            }

            if (includeProps != null && includeProps.Length > 0)
            {
                foreach (var item in includeProps)
                {
                    queryable = queryable.Include(item);
                }
            }

            return await Task.FromResult(queryable.Where(predicate));
        }

        public async Task<IQueryable<T>> GetAllItemsAsync(TransactionType transactionType = TransactionType.Execute, params string[] includeProps)
        {
            IQueryable<T> queryable = context.Set<T>();

            if (transactionType == TransactionType.ReadOnly)
            {
                queryable = queryable.AsNoTracking();
            }

            if (includeProps != null && includeProps.Length > 0)
            {
                foreach (var item in includeProps)
                {
                    queryable = queryable.Include(item);
                }
            }

            return await Task.FromResult(queryable);
        }

        public async Task<IQueryable<T>> FromRawQueryAsync(string query)
        {
            return await Task.FromResult(context.Set<T>().FromSqlRaw(query));
        }

        public async Task<int> ExecuteQueryAsync(string query)
        {
            return await Task.FromResult(context.Database.ExecuteSqlRaw(query));
        }

        public async Task<int> TruncatePartitionAsync(string schema, string tableName, int partitionStatusOrder)
        {
            return await ExecuteQueryAsync($"TRUNCATE TABLE {schema}.{tableName} WITH (PARTITIONS ({partitionStatusOrder}))");
        }
    }
}