using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PATH.BusinessLayer.Abstract;
using PATH.DataTransferObject;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PATH.WebApp
{
    public interface IMessageModel
    {
        string Username { get; set; }
        string MessageDate { get; set; }
        string Message { get; set; }
    }

    public class LogModel : IMessageModel
    {
        public string ChatRoomName { get; set; }
        public LogTypeEnum LogType { get; set; }
        public string Username { get; set; }
        public string MessageDate { get; set; }
        public string Message { get; set; }
    }

    public class MessageModel : IMessageModel
    {
        public string Username { get; set; }
        public string MessageDate { get; set; }
        public string Message { get; set; }
    }

    public enum LogTypeEnum : int
    {
        JOIN = 1,
        LEAVE = 2,
        MESSAGE = 3
    }

    public enum ErrorCodeEnum : int
    {
        ERROR100 = 0, // Nickname is not available
        ERROR101 = 1, // User has not signed in yet
        ERROR102 = 2, // Chat room not found
        ERROR103 = 4  // User is not a room member
    }

    public class ChatHub : Hub
    {
        private static RedisManagerPool _redisManagerPool;

        private static RedisManagerPool redisManagerPool
        {
            get
            {
                if (_redisManagerPool == null)
                {
                    _redisManagerPool = new RedisManagerPool("localhost:6379");
                }
                return _redisManagerPool;
            }
        }

        private static Dictionary<int, string> chatRoomList = new Dictionary<int, string>()
        {
            { 1, "Istanbul" },
            { 2, "Ankara" },
            { 3, "Izmir" }
        };

        private static Dictionary<int, Dictionary<string, string>> chatRoomMemberList = new Dictionary<int, Dictionary<string, string>>();

        private static Dictionary<string, string> memberList = new Dictionary<string, string>();

        private IChatRoomLogBl chatRoomLogBl;

        public ChatHub(IChatRoomLogBl chatRoomLogBl)
        {
            this.chatRoomLogBl = chatRoomLogBl;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;

            leaveFromChatOrChatRoom(connectionId, false);

            return base.OnDisconnectedAsync(exception);
        }

        public Task LoginRequest(string username)
        {
            string connectionId = Context.ConnectionId;

            if (memberList.Any(x => x.Value == username)) return sendUnsuccessfullResponse(connectionId, ErrorCodeEnum.ERROR100);

            memberList.Add(connectionId, username);

            return Clients.Client(connectionId).SendAsync("LoginRequestSuccessfull", username);
        }

        public Task JoinTheChatRoomRequest(string username, string chatRoomName)
        {
            string connectionId = Context.ConnectionId;

            leaveFromChatOrChatRoom(connectionId, true);

            if (!checkUserLoginByUserCridential(connectionId, username)) return sendUnsuccessfullResponse(connectionId, ErrorCodeEnum.ERROR101);

            if (!chechChatRoomByChatRoomName(chatRoomName)) return sendUnsuccessfullResponse(connectionId, ErrorCodeEnum.ERROR102);

            KeyValuePair<int, string> chatRoom = getChatRoomByChatRoomName(chatRoomName);

            int chatRoomId = chatRoom.Key;

            return joinToChatRoom(connectionId, username, chatRoomId);
        }

        public Task SendMessageToChatRoomRequest(string username, string chatRoomName, string message)
        {
            string connectionId = Context.ConnectionId;

            if (!checkUserLoginByUserCridential(connectionId, username)) return sendUnsuccessfullResponse(connectionId, ErrorCodeEnum.ERROR100);

            if (!chechChatRoomByChatRoomName(chatRoomName)) return sendUnsuccessfullResponse(connectionId, ErrorCodeEnum.ERROR101);

            KeyValuePair<int, string> chatRoom = getChatRoomByChatRoomName(chatRoomName);

            int chatRoomId = chatRoom.Key;

            if (!chechChatRoomMemberByUserCridential(chatRoomId, connectionId, username)) return sendUnsuccessfullResponse(connectionId, ErrorCodeEnum.ERROR103);

            List<string> membersOfChatRoom = getMembersOfChatRoomByChatRoomId(chatRoomId);

            string messageDate = DateTime.Now.ToString("HH:mm:ss");

            addMessageToRedis(chatRoomName, username, messageDate, message);

            return Clients.Clients(membersOfChatRoom).SendAsync("HasNewMessageOnChatRoom", username, chatRoomName, messageDate, message);
        }

        public Task GetChatRoomListRequest()
        {
            string connectionId = Context.ConnectionId;

            string jsonChatRoomList = getJsonChatRoomList();

            return Clients.Client(connectionId).SendAsync("GetChatRoomListRequestSuccessfull", jsonChatRoomList);
        }

        private void addMessageToRedis(string chatRoomName, string username, string messageDate, string message)
        {
            lock (redisManagerPool)
            {
                using (IRedisClient redisClient = redisManagerPool.GetClient())
                {
                    MessageModel messageModel = new MessageModel()
                    {
                        Username = username,
                        MessageDate = messageDate,
                        Message = message
                    };

                    string jsonMessageModel = JsonConvert.SerializeObject(messageModel);

                    redisClient.AddItemToList(chatRoomName, jsonMessageModel);
                }
            }
        }

        private List<MessageModel> getMessageFromRedis(string chatRoomName)
        {
            lock (redisManagerPool)
            {
                using (IRedisClient redisClient = redisManagerPool.GetClient())
                {
                    List<string> chatRoomHistory = redisClient.GetAllItemsFromList(chatRoomName);

                    List<MessageModel> messageModelList = chatRoomHistory.Select(x => JsonConvert.DeserializeObject<MessageModel>(x)).ToList();

                    return messageModelList;
                }
            }
        }

        private bool checkUserLoginConnectionId(string connectionId)
        {
            return memberList.Any(x => x.Key == connectionId);
        }

        private bool checkUserLoginByUserCridential(string connectionId, string username)
        {
            return memberList.Any(x => x.Key == connectionId && x.Value == username);
        }

        private bool chechChatRoomByChatRoomName(string chatRoomName)
        {
            return chatRoomList.Any(x => x.Value == chatRoomName);
        }

        private bool chechChatRoomMemberByConnectionId(string connectionId)
        {
            return chatRoomMemberList.Any(x => x.Value.Any(y => y.Key == connectionId));
        }

        private bool chechChatRoomMemberByUserCridential(int chatRoomId, string connectionId, string username)
        {
            return chatRoomMemberList.Where(x => x.Key == chatRoomId).Any(x => x.Value.Any(y => y.Key == connectionId && y.Value == username));
        }

        private Task sendUnsuccessfullResponse(string connectionId, ErrorCodeEnum errorCode)
        {
            switch (errorCode)
            {
                case ErrorCodeEnum.ERROR100:
                    return Clients.Client(connectionId).SendAsync("HasAnErrorOccurred", "ERROR100");
                case ErrorCodeEnum.ERROR101:
                    return Clients.Client(connectionId).SendAsync("HasAnErrorOccurred", "ERROR101");
                case ErrorCodeEnum.ERROR102:
                    return Clients.Client(connectionId).SendAsync("HasAnErrorOccurred", "ERROR102");
                case ErrorCodeEnum.ERROR103:
                    return Clients.Client(connectionId).SendAsync("HasAnErrorOccurred", "ERROR103");
                default:
                    return Clients.Client(connectionId).SendAsync("HasAnErrorOccurred", "UNKNOWN");
            }
        }

        private string getUsernameByConnectionId(string connectionId)
        {
            return memberList.FirstOrDefault(x => x.Key == connectionId).Value;
        }

        private List<string> getMembersOfChatRoomByChatRoomId(int chatRoomId)
        {
            return chatRoomMemberList.FirstOrDefault(x => x.Key == chatRoomId).Value.Select(x => x.Key).ToList();
        }

        private KeyValuePair<int, string> getChatRoomByChatRoomName(string chatRoomName)
        {
            return chatRoomList.FirstOrDefault(x => x.Value == chatRoomName);
        }

        private string getChatRoomNameByChatRoomId(int chatRoomId)
        {
            return chatRoomList.FirstOrDefault(x => x.Key == chatRoomId).Value;
        }

        private KeyValuePair<int, Dictionary<string, string>> getChatRoomWithMembersByChatRoomId(int chatRoomId)
        {
            return chatRoomMemberList.FirstOrDefault(x => x.Key == chatRoomId);
        }

        private void leaveFromChatOrChatRoom(string connectionId, bool leaveActiveChatRoomOnly)
        {
            string username = string.Empty;

            if (checkUserLoginConnectionId(connectionId))
            {
                username = getUsernameByConnectionId(connectionId);

                if (!leaveActiveChatRoomOnly) memberList.Remove(connectionId);
            }

            if (chechChatRoomMemberByConnectionId(connectionId))
            {
                List<int> chatRoomIds = chatRoomMemberList.Where(x => x.Value.ContainsKey(connectionId)).Select(x => x.Key).ToList();

                chatRoomIds.ForEach(x =>
                {
                    chatRoomMemberList.FirstOrDefault(y => y.Key == x).Value.Remove(connectionId);

                    List<string> membersOfChatRoom = getMembersOfChatRoomByChatRoomId(x);

                    string chatRoomName = getChatRoomNameByChatRoomId(x);

                    string messageDate = DateTime.Now.ToString("HH:mm:ss");

                    string message = $"{username} has just left from chat room";

                    Clients.Clients(membersOfChatRoom).SendAsync("HasDisconnection", "Server", chatRoomName, messageDate, message);
                });
            }
        }

        private Task joinToChatRoom(string connectionId, string username, int chatRoomId)
        {
            if (!chatRoomMemberList.Any(x => x.Key == chatRoomId)) chatRoomMemberList.Add(chatRoomId, new Dictionary<string, string>() { { connectionId, username } });

            KeyValuePair<int, Dictionary<string, string>> chatRoomWithMembers = getChatRoomWithMembersByChatRoomId(chatRoomId);

            if (!chatRoomWithMembers.Value.Any(x => x.Key == connectionId && x.Value == username)) chatRoomWithMembers.Value.Add(connectionId, username);

            List<string> membersOfChatRoom = getMembersOfChatRoomByChatRoomId(chatRoomId);

            string chatRoomName = getChatRoomNameByChatRoomId(chatRoomId);

            string messageDate = DateTime.Now.ToString("HH:mm:ss");

            string message = $"{username} has just joined to chat room";

            Clients.Clients(membersOfChatRoom).SendAsync("HasNewConnection", "Server", chatRoomName, messageDate, message);

            string jsonChatRoomHistory = getJsonChatRoomHistoryByChatRoomId(chatRoomId);

            return Clients.Client(connectionId).SendAsync("JoinChatRoomRequestSuccessfull", username, chatRoomName, jsonChatRoomHistory);
        }

        private string getJsonChatRoomList()
        {
            var objectChatRoomList = chatRoomList.Select(x => new
            {
                Id = x.Key,
                Name = x.Value
            }).ToList();

            string jsonChatRoomList = JsonConvert.SerializeObject(objectChatRoomList);

            return jsonChatRoomList;
        }

        private string getJsonChatRoomHistoryByChatRoomId(int chatRoomId)
        {
            string chatRoomName = getChatRoomNameByChatRoomId(chatRoomId);

            List<MessageModel> messageModelList = getMessageFromRedis(chatRoomName);

            string jsonChatRoomHistory = JsonConvert.SerializeObject(messageModelList);

            return jsonChatRoomHistory;
        }

        private async Task<int> WriteLogToDb(ChatRoomLogDto dto)
        {
            chatRoomLogBl.AddNew(dto);
            return await chatRoomLogBl.SaveChanges();
        }
    }
}
