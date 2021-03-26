using Microsoft.EntityFrameworkCore;
using PATH.Repository.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PATH.Repository.Concrete
{
    public class BaseRepository<T> : IBaseRepository<T>
        where T : class, new()
    {
        protected DbContext dbContext;

        public BaseRepository(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Delete(T entity)
        {
            dbContext.Remove(entity).State = EntityState.Deleted;
        }

        public void Insert(T entity)
        {
            dbContext.Add(entity).State = EntityState.Added;
        }

        public IQueryable<T> Select(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null) return dbContext.Set<T>().AsQueryable();
            else return dbContext.Set<T>().Where(predicate);
        }

        public void Update(T entity)
        {
            dbContext.Update(entity).State = EntityState.Modified;
        }
    }
}
