using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using blazorApp7.Data;

namespace ChatFunctions {
    public interface IChatHubFunctions {
        Task SendMessage(string user, string message);
        Task LogOn(string user, string groupid);
        Task LogOff(string user, string groupid);
        Task UserListRequest(string groupid);
        Task GroupListRequest();
        Task GroupStatRequest();
    }
    public interface IChatClientFunctions {
        Task ReceiveMessage(string User, string Message, string ConnectionId, string Color);
        Task LoggedOn(string ConnectionId, string User, string Color);
        Task LoggedOff(string ConnectionId, string User, string Color);
        Task UserListReply(List<string> UserList);
        Task FromServer(string ServerMessage);
        Task GroupListReply(List<GroupInfo> GroupList);
        Task GroupStatReply(List<GroupUserInfo> GroupUserList);
    }
}