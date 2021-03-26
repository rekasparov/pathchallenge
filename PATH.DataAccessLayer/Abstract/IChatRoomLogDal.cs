using PATH.Repository.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PATH.DataAccessLayer.Abstract
{
    public interface IChatRoomLogDal<T> : IBaseRepository<T>
        where T : class, new()
    {
    }
}
