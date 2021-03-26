using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PATH.Repository.Abstract
{
    public interface IBaseRepository<T>
        where T : class, new()
    {
        IQueryable<T> Select(Expression<Func<T, bool>> predicate = null);
        void Insert(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
