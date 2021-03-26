using PATH.DataTransferObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PATH.BusinessLayer.Abstract
{
    public interface IChatRoomLogBl
    {
        void AddNew(ChatRoomLogDto dto);
        Task<int> SaveChanges();
    }
}
