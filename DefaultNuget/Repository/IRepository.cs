using System.Linq.Expressions;
using DefaultNuget.Enum;
using Microsoft.EntityFrameworkCore;

namespace DefaultNuget.Repository
{
    public interface IRepository<T, TContext> where T : class where TContext : DbContext
    {
        public TContext context { get; }

        Task<bool> TryInsertItemAsync(T item);
        Task<bool> TryInsertItemsAsync(ICollection<T> items);
        Task<bool> TryUpdateItemAsync(T item);
        Task<bool> TryUpdateItemsAsync(ICollection<T> items);
        Task<bool> TryDeleteItemAsync(T item);
        Task<bool> TryDeleteItemsAsync(ICollection<T> items);
        Task<T> GetItemAsync(Expression<Func<T, bool>> predicate, TransactionType transactionType = TransactionType.Execute, params string[] includeProps);
        Task<IQueryable<T>> GetFilteredItemsAsync(Expression<Func<T, bool>> predicate, TransactionType transactionType = TransactionType.Execute, params string[] includeProps);
        Task<IQueryable<T>> GetAllItemsAsync(TransactionType transactionType = TransactionType.Execute, params string[] includeProps);
        Task<IQueryable<T>> FromRawQueryAsync(string query);
        Task<int> ExecuteQueryAsync(string query);
        Task<int> TruncatePartitionAsync(string schema, string tableName, int partitionStatusOrder);
    }
}