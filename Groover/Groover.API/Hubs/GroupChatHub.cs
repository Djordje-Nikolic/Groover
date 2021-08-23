using Groover.API.Hubs.Interfaces;
using Groover.BL.Handlers.Requirements;
using Groover.BL.Models.Exceptions;
using Groover.DB.MySqlDb.Entities;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Groover.API.Hubs
{
    public class GroupChatHub : Hub
    {
        public async override Task OnConnectedAsync()
        {
            var userId = GetUserId();

            await Groups.AddToGroupAsync(Context.ConnectionId, GenerateUserGroupName(userId));

            await base.OnConnectedAsync();
        }

        public async Task OpenGroupConnection(string groupId)
        {
            var claims = Context.User ?? throw new UnauthorizedException("Undefined claims.", "invalid_claims");
            var isInGroup = claims.FindFirst(cl => cl.Type == GroupClaimTypeConstants.GetConstant(GroupRole.Member) &&
                                   cl.Value == groupId) != null;

            if (!isInGroup)
                throw new UnauthorizedException("User is not a member of this group.", "not_a_member");

            var userId = GetUserId();

            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

            await Clients.Group(groupId).SendAsync("ConnectedToGroup", groupId, userId);
        }

        public async Task CloseGroupConnection(string groupId)
        {
            var claims = Context.User ?? throw new UnauthorizedException("Undefined claims.", "invalid_claims");

            var userId = GetUserId();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);

            await Clients.Group(groupId).SendAsync("DisconnectedFromGroup", groupId, userId);
        }

        private string GetUserId()
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
                throw new BadRequestException("Couldn't determine user id.", "bad_id");

            return userId;
        }

        public static string GenerateUserGroupName(string userId)
        {
            return $"user_{userId}";
        }
    }
}
