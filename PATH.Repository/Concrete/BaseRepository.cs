using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
            using (IDbContextTransaction dbContextTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    dbContext.Remove(entity).State = EntityState.Deleted;
                    dbContextTransaction.Commit();
                }
                catch
                {
                    dbContextTransaction.Rollback();
                }
            }
        }

        public void Insert(T entity)
        {
            using (IDbContextTransaction dbContextTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    dbContext.Add(entity).State = EntityState.Added;
                    dbContextTransaction.Commit();
                }
                catch
                {
                    dbContextTransaction.Rollback();
                }
            }
        }

        public IQueryable<T> Select(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null) return dbContext.Set<T>().AsQueryable();
            else return dbContext.Set<T>().Where(predicate);
        }

        public void Update(T entity)
        {
            using (IDbContextTransaction dbContextTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    dbContext.Update(entity).State = EntityState.Modified;
                    dbContextTransaction.Commit();
                }
                catch
                {
                    dbContextTransaction.Rollback();
                }
            }
        }
    }
}
