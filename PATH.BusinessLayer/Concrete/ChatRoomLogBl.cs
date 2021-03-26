using PATH.BusinessLayer.Abstract;
using PATH.DataTransferObject;
using PATH.Extension;
using PATH.UnitOfWork.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PATH.BusinessLayer.Concrete
{
    public class ChatRoomLogBl : IChatRoomLogBl
    {
        private IBaseUnitOfWork unitOfWork;

        public ChatRoomLogBl(IBaseUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public void AddNew(ChatRoomLogDto dto)
        {
            var entity = dto.ToEntity();
            unitOfWork.ChatRoomLog.Insert(entity);
        }

        public async Task<int> SaveChanges()
        {
            return await unitOfWork.SaveChanges();
        }
    }
}
