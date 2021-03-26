using System;
using System.Collections.Generic;

namespace PATH.DataTransferObject
{
    public partial class ChatRoomLogDto
    {
        public Guid Id { get; set; }
        public string ChatRoomName { get; set; }
        public int LogType { get; set; }
        public string Username { get; set; }
        public string MessageDate { get; set; }
        public string Message { get; set; }
    }
}
