using PATH.DataTransferObject;
using PATH.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PATH.Extension
{
    public static class ChatRoomLogExtension
    {
        public static ChatRoomLog ToEntity(this ChatRoomLogDto dto)
        {
            return new ChatRoomLog()
            {
                Id = Guid.NewGuid(),
                ChatRoomName = dto.ChatRoomName,
                LogType = dto.LogType,
                Username = dto.Username,
                MessageDate = dto.MessageDate,
                Message = dto.Message
            };
        }

        public static ChatRoomLogDto ToDto(this ChatRoomLog entity)
        {
            return new ChatRoomLogDto()
            {
                Id = entity.Id,
                ChatRoomName = entity.ChatRoomName,
                LogType = entity.LogType,
                Username = entity.Username,
                MessageDate = entity.MessageDate,
                Message = entity.Message
            };
        }
    }
}
