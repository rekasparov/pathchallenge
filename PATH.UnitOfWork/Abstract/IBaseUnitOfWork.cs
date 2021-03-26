using PATH.DataAccessLayer.Abstract;
using PATH.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PATH.UnitOfWork.Abstract
{
    public interface IBaseUnitOfWork : IDisposable
    {
        IChatRoomLogDal<ChatRoomLog> ChatRoomLog { get; }
        Task<int> SaveChanges();
    }
}
