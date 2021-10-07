using Groover.AvaloniaUI.Models.DTOs;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Utils
{
    public class ConnectionHandlersWrapper
    {
        private HubConnection _connection;

        public void SetConnection(HubConnection connection)
        {
            _connection = connection;
        }

        public void CleanUpHandlers()
        {
            _connection.Remove("GroupMessageAdded");
            _connection.Remove("GroupCreated");
            _connection.Remove("GroupDeleted");
            _connection.Remove("GroupUpdated");
            _connection.Remove("UserJoined");
            _connection.Remove("LoggedInUserJoined");
            _connection.Remove("UserUpdated");
            _connection.Remove("LoggedInUserUpdated");
            _connection.Remove("UserLeft");
            _connection.Remove("UserRoleUpdated");
            _connection.Remove("UserInvited");
            _connection.Remove("ConnectedToGroup");
            _connection.Remove("DisconnectedFromGroup");
        }

        public void GroupMessageAdded(Action<Message> handler) => _connection.On<Message>("GroupMessageAdded", handler);
        public void GroupMessageAdded(Func<Message, Task> handler) => _connection.On<Message>("GroupMessageAdded", handler);
        public void GroupCreated(Action<UserGroup> handler) => _connection.On<UserGroup>("GroupCreated", handler);
        public void GroupCreated(Func<UserGroup, Task> handler) => _connection.On<UserGroup>("GroupCreated", handler);
        public void GroupDeleted(Action<string> handler) => _connection.On<string>("GroupDeleted", handler);
        public void GroupDeleted(Func<string, Task> handler) => _connection.On<string>("GroupDeleted", handler);
        public void GroupUpdated(Action<Group> handler) => _connection.On<Group>("GroupUpdated", handler);
        public void GroupUpdated(Func<Group, Task> handler) => _connection.On<Group>("GroupUpdated", handler);
        public void UserJoined(Action<string, GroupUser> handler) => _connection.On<string, GroupUser>("UserJoined", handler);
        public void UserJoined(Func<string, GroupUser, Task> handler) => _connection.On<string, GroupUser>("UserJoined", handler);
        public void LoggedInUserJoined(Action<UserGroup> handler) => _connection.On<UserGroup>("LoggedInUserJoined", handler);
        public void LoggedInUserJoined(Func<UserGroup, Task> handler) => _connection.On<UserGroup>("LoggedInUserJoined", handler);
        public void UserLeft(Action<string, string> handler) => _connection.On<string, string>("UserLeft", handler);
        public void UserLeft(Func<string, string, Task> handler) => _connection.On<string, string>("UserLeft", handler);
        public void UserInvited(Action<byte[], Group, string> handler) => _connection.On<byte[], Group, string>("UserInvited", handler);
        public void UserInvited(Func<byte[], Group, string, Task> handler) => _connection.On<byte[], Group, string>("UserInvited", handler);
        public void UserRoleUpdated(Action<string, string, string> handler) => _connection.On<string, string, string>("UserRoleUpdated", handler);
        public void UserRoleUpdated(Func<string, string, string, Task> handler) => _connection.On<string, string, string>("UserRoleUpdated", handler);
        public void UserUpdated(Action<string, User> handler) => _connection.On<string, User>("UserUpdated", handler);
        public void UserUpdated(Func<string, User, Task> handler) => _connection.On<string, User>("UserUpdated", handler);
        public void LoggedInUserUpdated(Action<User> handler) => _connection.On<User>("LoggedInUserUpdated", handler);
        public void LoggedInUserUpdated(Func<User, Task> handler) => _connection.On<User>("LoggedInUserUpdated", handler);
        public void ConnectedToGroup(Action<string, string> handler) => _connection.On<string, string>("ConnectedToGroup", handler);
        public void ConnectedToGroup(Func<string, string, Task> handler) => _connection.On<string, string>("ConnectedToGroup", handler);
        public void DisconnectedFromGroup(Action<string, string> handler) => _connection.On<string, string>("DisconnectedFromGroup", handler);
        public void DisconnectedFromGroup(Func<string, string, Task> handler) => _connection.On<string, string>("DisconnectedFromGroup", handler);
    }
}
