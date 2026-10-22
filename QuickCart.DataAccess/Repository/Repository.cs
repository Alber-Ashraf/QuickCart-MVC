using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuickCart.Data;
using QuickCart.DataAccess.Repository.IRepository;

namespace QuickCart.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly QuickCartDbContext _db;
        internal DbSet<T> dbSet;

        public Repository(QuickCartDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await dbSet.AddAsync(entity, cancellationToken);
        }

        private IQueryable<T> BuildQuery(Expression<Func<T, bool>>? filter = null, string? includedProperties = null, bool tracking = true)
        {
            IQueryable<T> query = tracking ? dbSet : dbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includedProperties))
            {
                foreach (var includeProperty in includedProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return query;
        }

        public T? Get(Expression<Func<T, bool>> filter, string? includedProperties = null, bool tracked = false)
        {
            return BuildQuery(filter, includedProperties, tracked).FirstOrDefault();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includedProperties = null, bool tracked = false, CancellationToken cancellationToken = default)
        {
            return await BuildQuery(filter, includedProperties, tracked).FirstOrDefaultAsync(cancellationToken);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includedProperties = null, bool tracking = true)
        {
            return BuildQuery(filter, includedProperties, tracking).ToList();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includedProperties = null, bool tracking = true, CancellationToken cancellationToken = default)
        {
            return await BuildQuery(filter, includedProperties, tracking).ToListAsync(cancellationToken);
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
    }
}
