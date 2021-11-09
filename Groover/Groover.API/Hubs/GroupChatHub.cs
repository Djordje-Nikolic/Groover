using AutoMapper;
using Groover.API.Models.Requests;
using Groover.API.Models.Responses;
using Groover.API.Services.Interfaces;
using Groover.BL.Handlers.Requirements;
using Groover.BL.Models.Chat.DTOs;
using Groover.BL.Models.Exceptions;
using Groover.BL.Services.Interfaces;
using Groover.IdentityDB.MySqlDb.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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
        private readonly IGroupChatService _groupChatService;
        private readonly IMapper _mapper;
        private readonly ILogger<GroupChatHub> _logger;


        //Add logging
        public GroupChatHub(IGroupChatService groupChatService,
                            IMapper mapper,
                            ILogger<GroupChatHub> logger) : base()
        {
            _groupChatService = groupChatService;
            _mapper = mapper;
            _logger = logger;
        }

        public async override Task OnConnectedAsync()
        {
            var userId = GetUserId();

            await Groups.AddToGroupAsync(Context.ConnectionId, GenerateUserGroupName(userId));

            await base.OnConnectedAsync();
        }

        public async Task OpenGroupConnection(string groupId)
        {
            if (!IsGroupMember(groupId))
                throw new HubException("Unauthorized: not_member");

            var userId = GetUserId();

            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

            //Notify group that I have connected
            await Clients.Group(groupId).SendAsync("ConnectedToGroup", groupId, userId);
        }

        //TODO: The "who is online and who isnt" feature doesnt work with multiple connections per user (find a solution eventually)
        public async Task CloseGroupConnection(string groupId)
        {
            var userId = GetUserId();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);

            await Clients.Group(groupId).SendAsync("DisconnectedFromGroup", groupId, userId);
        }

        public async Task NotifyConnection(string groupId, string userToNotifyId)
        {
            if (!IsGroupMember(groupId))
                throw new HubException("Unauthorized: not_member");

            var senderId = GetUserId();

            //Notify new user that I AM connected
            var userGroupName = GenerateUserGroupName(userToNotifyId);
            await Clients.Group(userGroupName).SendAsync("AlreadyConnectedToGroup", groupId, senderId);
        }

        public async Task SendTextMessage(TextMessageRequest messageData)
        {
            if (!IsGroupMember(messageData.GroupId.ToString()))
                throw new HubException("Unauthorized: not_member");

            var senderId = GetUserId();
            if (!int.TryParse(senderId, out int userId))
                throw new HubException("Unauthorized: bad_id");

            try
            {
                _logger.LogInformation($"Attempting to send a text message: Group ID: {messageData.GroupId} Sender ID: {userId}");

                TextMessageDTO textMessageDTO = _mapper.Map<TextMessageDTO>(messageData);
                textMessageDTO.SenderId = userId;
                textMessageDTO.Type = BL.Models.Chat.MessageType.Text;

                //call group chat service
                FullMessageDTO addedMessageDTO = await _groupChatService.AddTextMessageAsync(textMessageDTO);

                _logger.LogInformation($"Successfully sent a text message: Group ID: {messageData.GroupId} Sender ID: {userId} Message ID: {addedMessageDTO.Id}");

                //call notification service
                var notificationData = _mapper.Map<FullMessageResponse>(addedMessageDTO);
                await OnGroupMessageAddedAsync(notificationData);
            }
            catch (BadRequestException e)
            {
                throw new HubException($"BadRequest: {e.ErrorCode}", e);
            }
            catch (GrooverException e)
            {
                throw new HubException($"Internal: {e.ClientMessage}", e);
            }
            catch (Exception e)
            {
                throw new HubException($"Internal", e);
            }
        }

        public async Task SendImageMessage(ImageMessageRequest messageData)
        {
            if (!IsGroupMember(messageData.GroupId.ToString()))
                throw new HubException("Unauthorized: not_member");

            var senderId = GetUserId();
            if (!int.TryParse(senderId, out int userId))
                throw new HubException("Unauthorized: bad_id");

            try
            {
                _logger.LogInformation($"Attempting to send an image message: Group ID: {messageData.GroupId} Sender ID: {userId}");

                ImageMessageDTO imageMessageDTO = _mapper.Map<ImageMessageDTO>(messageData);
                imageMessageDTO.SenderId = userId;
                imageMessageDTO.Type = BL.Models.Chat.MessageType.Image;

                //call group chat service
                FullMessageDTO addedMessageDTO = await _groupChatService.AddImageMessageAsync(imageMessageDTO);

                _logger.LogInformation($"Successfully sent an image message: Group ID: {messageData.GroupId} Sender ID: {userId} Message ID: {addedMessageDTO.Id}");

                //call notification service
                var notificationData = _mapper.Map<FullMessageResponse>(addedMessageDTO);
                await OnGroupMessageAddedAsync(notificationData);
            }
            catch (BadRequestException e)
            {
                throw new HubException($"BadRequest: {e.ErrorCode}", e);
            }
            catch (GrooverException e)
            {
                throw new HubException($"Internal: {e.ClientMessage}", e);
            }
            catch (Exception e)
            {
                throw new HubException($"Internal", e);
            }
        }

        private bool IsGroupMember(string groupId)
        {
            var claims = Context.User ?? throw new HubException("Unauthorized: invalid_claims");
            var isInGroup = claims.FindFirst(cl => cl.Type == GroupClaimTypeConstants.GetConstant(GroupRole.Member) &&
                                   cl.Value == groupId) != null;

            return isInGroup;
        }

        private string GetUserId()
        {
            var claims = Context.User ?? throw new HubException("Unauthorized: invalid_claims");
            var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new HubException("Unauthorized: bad_id");

            return userId;
        }

        private async Task OnGroupMessageAddedAsync(FullMessageResponse message)
        {
            string groupId = message.GroupId.ToString();

            await Clients.Group(groupId).SendAsync("GroupMessageAdded", message);
        }
        internal static string GenerateUserGroupName(string userId)
        {
            return $"user_{userId}";
        }
    }
}
