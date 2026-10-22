using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace QuickCart.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        // Synchronous operations (preserved for backward compatibility)
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includedProperties = null, bool tracking = true);
        T? Get(Expression<Func<T, bool>> filter, string? includedProperties = null, bool tracked = false);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        // Asynchronous operations (high-throughput non-blocking I/O)
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includedProperties = null, bool tracking = true, CancellationToken cancellationToken = default);
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includedProperties = null, bool tracked = false, CancellationToken cancellationToken = default);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
    }
}
