using Groover.API.Hubs;
using Groover.API.Models.Responses;
using Groover.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<GroupChatHub> _hubContext;

        public NotificationService(IHubContext<GroupChatHub> hubContext)
        {
            this._hubContext = hubContext;
        }

        public async Task ForceTokenRefreshAsync(string userId)
        {
            string userGroupName = GroupChatHub.GenerateUserGroupName(userId);
            await _hubContext.Clients.Group(userGroupName).SendAsync("ForceTokenRefresh", userId);
        }

        public async Task GroupCreatedAsync(UserGroupResponse newGroup, string userId)
        {
            string userGroupName = GroupChatHub.GenerateUserGroupName(userId);
            await _hubContext.Clients.Group(userGroupName).SendAsync("GroupCreated", newGroup);
        }

        public async Task UserLeftGroupAsync(string groupId, string userId)
        {
            await _hubContext.Clients.Group(groupId).SendAsync("UserLeft", groupId, userId);
        }

        public async Task UserJoinedGroupAsync(string groupId, GroupUserLiteResponse groupUserData, UserGroupResponse userGroupData)
        {
            //Notify all user connections
            string userGroupName = GroupChatHub.GenerateUserGroupName(groupUserData.User.Id.ToString());
            await _hubContext.Clients.Group(userGroupName).SendAsync("LoggedInUserJoined", userGroupData);

            await _hubContext.Clients.Group(groupId).SendAsync("UserJoined", groupId, groupUserData);
        }

        public async Task UserInvitedAsync(string token, GroupLiteResponse group, string userId)
        {
            string userGroupName = GroupChatHub.GenerateUserGroupName(userId);
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
            await _hubContext.Clients.Group(userGroupName).SendAsync("UserInvited", tokenBytes, group, userId);
        }

        public async Task UserRoleUpdatedAsync(string groupId, string userId, string newRole)
        {
            await _hubContext.Clients.Group(groupId).SendAsync("UserRoleUpdated", groupId, userId, newRole);
        }

        public async Task UserUpdatedAsync(UserDataResponse user, List<string> groupIds)
        {
            //Notify all user connections
            string userGroupName = GroupChatHub.GenerateUserGroupName(user.Id.ToString());
            await _hubContext.Clients.Group(userGroupName).SendAsync("LoggedInUserUpdated", user);

            //Notify all groups containing the user
            foreach (var groupId in groupIds)
            {
                await _hubContext.Clients.Group(groupId).SendAsync("UserUpdated", groupId, user);
            }
        }

        public async Task GroupUpdatedAsync(GroupDataResponse group)
        {
            await _hubContext.Clients.Group(group.Id.ToString()).SendAsync("GroupUpdated", group);
        }

        public async Task GroupDeletedAsync(string groupId)
        {
            await _hubContext.Clients.Group(groupId).SendAsync("GroupDeleted", groupId);
        }
    }
}
