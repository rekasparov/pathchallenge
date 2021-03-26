using System;
using System.Collections.Generic;

#nullable disable

namespace PATH.Entity
{
    public partial class ChatRoomLog
    {
        public Guid Id { get; set; }
        public string ChatRoomName { get; set; }
        public int LogType { get; set; }
        public string Username { get; set; }
        public string MessageDate { get; set; }
        public string Message { get; set; }
    }
}
