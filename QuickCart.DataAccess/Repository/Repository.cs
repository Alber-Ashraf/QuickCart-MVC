using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
            _db.Products.Include(u => u.Category);
        }

        // This method is used to add a new entity to the database
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        // This method is used to get a single entity from the database
        public T Get(Expression<Func<T, bool>> filter, string? includedProperties = null, bool tracked = false)
        {
            IQueryable<T> query;
            // If tracked is true, we want to use the tracked version of the entity
            if (tracked)
            {
                query = dbSet;
            }
            // If tracked is false, we want to use the untracked version of the entity
            else
            {
                query = dbSet.AsNoTracking();
            }

            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includedProperties))
            {
                foreach (var includeProperty in includedProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }
            return query.FirstOrDefault();
        }

        // This method is used to get all entities from the database
        public IEnumerable<T> GetAll(string? includedProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (!string.IsNullOrEmpty(includedProperties))
            {
                foreach (var includeProperty in includedProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }
            return query.ToList();
        }

        // This method is used to remove an entity from the database
        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        // This method is used to remove a range of entities from the database
        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
