using Microsoft.EntityFrameworkCore;
using PATH.DataAccessLayer.Abstract;
using PATH.Entity;
using PATH.Repository.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PATH.DataAccessLayer.Concrete
{
    public class ChatRoomLogDal : BaseRepository<ChatRoomLog>, IChatRoomLogDal<ChatRoomLog>
    {
        public ChatRoomLogDal(DbContext dbContext)
            : base(dbContext)
        {

        }
    }
}
