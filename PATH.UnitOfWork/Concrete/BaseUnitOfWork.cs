using Microsoft.EntityFrameworkCore;
using PATH.DataAccessLayer.Abstract;
using PATH.DataAccessLayer.Concrete;
using PATH.Entity;
using PATH.UnitOfWork.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PATH.UnitOfWork.Concrete
{
    public class BaseUnitOfWork : IBaseUnitOfWork
    {
        public IChatRoomLogDal<ChatRoomLog> ChatRoomLog => new ChatRoomLogDal(dbContext);

        public async Task<int> SaveChanges()
        {
            return await dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(dbContext);
        }

        private static DbContext dbContext
        {
            get
            {
                return new PATHCHATContext();
            }
        }
    }
}
