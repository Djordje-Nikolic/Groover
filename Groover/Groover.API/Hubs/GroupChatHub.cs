using Groover.API.Hubs.Interfaces;
using Groover.API.Services.Interfaces;
using Groover.BL.Handlers.Requirements;
using Groover.BL.Models.Exceptions;
using Groover.BL.Services.Interfaces;
using Groover.IdentityDB.MySqlDb.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Groover.API.Hubs
{
    [Authorize]
    public class GroupChatHub : Hub
    {
        //private readonly IGroupService _groupService;
        //private readonly INotificationService _notificationService;

        //public GroupChatHub(IGroupService groupService,
        //                    INotificationService notificationService) : base()
        //{
        //    _groupService = groupService;
        //    _notificationService = notificationService;
        //}

        public async override Task OnConnectedAsync()
        {
            var userId = GetUserId();

            await Groups.AddToGroupAsync(Context.ConnectionId, GenerateUserGroupName(userId));

            await base.OnConnectedAsync();
        }

        public async Task OpenGroupConnection(string groupId)
        {
            var claims = Context.User ?? throw new HubException("Unauthorized: invalid_claims");
            var isInGroup = claims.FindFirst(cl => cl.Type == GroupClaimTypeConstants.GetConstant(GroupRole.Member) &&
                                   cl.Value == groupId) != null;

            if (!isInGroup)
                throw new HubException("Unauthorized: not_a_member");

            var userId = GetUserId();

            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

            //Notify group that I have connected
            await Clients.Group(groupId).SendAsync("ConnectedToGroup", groupId, userId);
        }

        //TODO: The "who is online and who isnt" feature doesnt work with multiple connections per user (find a solution eventually)
        public async Task CloseGroupConnection(string groupId)
        {
            var claims = Context.User ?? throw new HubException("Unauthorized: invalid_claims");

            var userId = GetUserId();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);

            await Clients.Group(groupId).SendAsync("DisconnectedFromGroup", groupId, userId);
        }

        public async Task NotifyConnection(string groupId, string userToNotifyId)
        {
            var claims = Context.User ?? throw new HubException("Unauthorized: invalid_claims");
            var isInGroup = claims.FindFirst(cl => cl.Type == GroupClaimTypeConstants.GetConstant(GroupRole.Member) &&
                                   cl.Value == groupId) != null;

            if (!isInGroup)
                throw new HubException("Unauthorized: not_a_member");

            var senderId = GetUserId();

            //Notify new user that I AM connected
            var userGroupName = GenerateUserGroupName(userToNotifyId);
            await Clients.Group(userGroupName).SendAsync("ConnectedToGroup", groupId, senderId);
        }

        private string GetUserId()
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new HubException("Unauthorized: bad_id");

            return userId;
        }

        public static string GenerateUserGroupName(string userId)
        {
            return $"user_{userId}";
        }
    }
}
