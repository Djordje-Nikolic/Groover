using AutoMapper;
using Groover.API.Models.Requests;
using Groover.API.Models.Responses;
using Groover.API.Services.Interfaces;
using Groover.API.Utils;
using Groover.BL.Handlers.Requirements;
using Groover.BL.Models;
using Groover.BL.Models.Chat.DTOs;
using Groover.BL.Models.DTOs;
using Groover.BL.Models.Exceptions;
using Groover.BL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Groover.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class GroupChatController : ControllerBase
    {
        private readonly IGroupChatService _groupChatService;
        private readonly INotificationService _notificationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMapper _mapper;
        private readonly ILogger<GroupChatController> _logger;

        public GroupChatController(IGroupChatService groupChatService,
                               INotificationService notificationService,
                               IAuthorizationService authorizationService,
                               IMapper autoMapper,
                               ILogger<GroupChatController> logger)
        {
            _groupChatService = groupChatService;
            _notificationService = notificationService;
            _authorizationService = authorizationService;
            _mapper = autoMapper;
            _logger = logger;
        }

        //Member
        [HttpGet("getMessages")]
        public async Task<IActionResult> GetMessages(int groupId, int pageSize, string pagingState)
        {
            if (!await IsGroupMemberAsync(groupId))
            {
                throw new UnauthorizedException($"User is not a member of the group {groupId}.", "not_member");
            }

            _logger.LogInformation($"Attempting to fetch messages. Group ID: {groupId} Page Size: {pageSize} Paging State: {pagingState}");

            PageParamsDTO pageParamsDTO = new PageParamsDTO()
            {
                PageSize = pageSize,
                PagingState = pagingState
            };

            PagedDataDTO<ICollection<FullMessageDTO>> pagedData = await _groupChatService.GetAllMessagesAsync(groupId, pageParamsDTO);
            var responseData = _mapper.Map<PagedResponse<ICollection<FullMessageResponse>>>(pagedData);

            return Ok(responseData);
        }

        //Member
        [HttpGet("getMessages")]
        public async Task<IActionResult> GetMessages(int groupId, int pageSize, string pagingState, DateTime createdAfter)
        {
            if (!await IsGroupMemberAsync(groupId))
            {
                throw new UnauthorizedException($"User is not a member of the group {groupId}.", "not_member");
            }

            _logger.LogInformation($"Attempting to fetch messages after a certain UTC time. Group ID: {groupId} Page Size: {pageSize} Paging State: {pagingState} After: {createdAfter}");

            PageParamsDTO pageParamsDTO = new PageParamsDTO()
            {
                PageSize = pageSize,
                PagingState = pagingState
            };

            PagedDataDTO<ICollection<FullMessageDTO>> pagedData = await _groupChatService.GetMessagesAsync(groupId, createdAfter, pageParamsDTO);
            var responseData = _mapper.Map<PagedResponse<ICollection<FullMessageResponse>>>(pagedData);

            return Ok(responseData);
        }

        //Member
        [HttpPost("sendTextMessage")]
        public async Task<IActionResult> SendTextMessage(TextMessageRequest messageData)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                throw new UnauthorizedException("User id undefined.", "bad_id");
            }

            if (!await IsGroupMemberAsync(messageData.GroupId))
            {
                throw new UnauthorizedException($"User is not a member of the group {messageData.GroupId}.", "not_member");
            }

            _logger.LogInformation($"Attempting to send a text message: Group ID: {messageData.GroupId} Sender ID: {userId}");

            TextMessageDTO textMessageDTO = _mapper.Map<TextMessageDTO>(messageData);
            textMessageDTO.SenderId = userId.Value;
            textMessageDTO.Type = BL.Models.Chat.MessageType.Text;

            //call group chat service
            FullMessageDTO addedMessageDTO = await _groupChatService.AddTextMessageAsync(textMessageDTO);

            _logger.LogInformation($"Successfully sent a text message: Group ID: {messageData.GroupId} Sender ID: {userId} Message ID: {addedMessageDTO.Id}");

            //call notification service
            var notificationData = _mapper.Map<FullMessageResponse>(addedMessageDTO);
            await _notificationService.GroupMessageAddedAsync(notificationData);

            return Ok();
        }

        //Member
        [HttpPost("sendImageMessage")]
        public async Task<IActionResult> SendImageMessage(ImageMessageRequest messageData)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                throw new UnauthorizedException("User id undefined.", "bad_id");
            }

            if (!await IsGroupMemberAsync(messageData.GroupId))
            {
                throw new UnauthorizedException($"User is not a member of the group {messageData.GroupId}.", "not_member");
            }

            _logger.LogInformation($"Attempting to send an image message: Group ID: {messageData.GroupId} Sender ID: {userId}");

            ImageMessageDTO imageMessageDTO = _mapper.Map<ImageMessageDTO>(messageData);
            imageMessageDTO.SenderId = userId.Value;
            imageMessageDTO.Type = BL.Models.Chat.MessageType.Image;

            //call group chat service
            FullMessageDTO addedMessageDTO = await _groupChatService.AddImageMessageAsync(imageMessageDTO);

            _logger.LogInformation($"Successfully sent an image message: Group ID: {messageData.GroupId} Sender ID: {userId} Message ID: {addedMessageDTO.Id}");

            //call notification service
            var notificationData = _mapper.Map<FullMessageResponse>(addedMessageDTO);
            await _notificationService.GroupMessageAddedAsync(notificationData);

            return Ok();
        }

        //Member
        [HttpPost("sendTrackMessage")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendTrackMessage([FromForm]IFormFile trackFile, [FromForm]TrackMessageRequest messageData)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                throw new UnauthorizedException("User id undefined.", "bad_id");
            }

            if (!await IsGroupMemberAsync(messageData.GroupId))
            {
                throw new UnauthorizedException($"User is not a member of the group {messageData.GroupId}.", "not_member");
            }

            _logger.LogInformation($"Attempting to send a track message: Group ID: {messageData.GroupId} Sender ID: {userId}");

            TrackMessageDTO trackMessageDTO = _mapper.Map<TrackMessageDTO>(messageData);
            trackMessageDTO.SenderId = userId.Value;
            trackMessageDTO.Type = BL.Models.Chat.MessageType.Track;

            //call group chat service
            FullMessageDTO addedMessageDTO = await _groupChatService.AddTrackMessageAsync(trackMessageDTO, trackFile);

            _logger.LogInformation($"Successfully sent a track message: Group ID: {messageData.GroupId} Sender ID: {userId} Message ID: {addedMessageDTO.Id}");

            //call notification service 
            var notificationData = _mapper.Map<FullMessageResponse>(addedMessageDTO);
            await _notificationService.GroupMessageAddedAsync(notificationData);

            return Ok();
        }

        private async Task<bool> IsGroupAdminAsync(int groupId)
        {
            GroupDTO group = new GroupDTO() { Id = groupId };
            var resultIsAdmin = await _authorizationService.AuthorizeAsync(User, group, new GroupRoleRequirement(GroupClaimTypeConstants.Admin));

            return resultIsAdmin.Succeeded;
        }

        private async Task<bool> IsGroupMemberAsync(int groupId)
        {
            GroupDTO group = new GroupDTO() { Id = groupId };
            var resultIsMember = await _authorizationService.AuthorizeAsync(User, group, new GroupRoleRequirement(GroupClaimTypeConstants.Member));

            return resultIsMember.Succeeded;
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim == null ? null : int.Parse(claim.Value);
        }
    }
}
