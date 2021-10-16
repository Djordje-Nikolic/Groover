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
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const string ExpectedDateTimeFormat = "d/MM/yyyy HH:mm:ss";
        private const long MaxTrackRequestLength = 105057600;
        private readonly IGroupChatService _groupChatService;
        private readonly INotificationService _notificationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMapper _mapper;
        private readonly ILogger<GroupChatController> _logger;
        private readonly LinkGenerator _linkGenerator;

        public GroupChatController(IGroupChatService groupChatService,
                               INotificationService notificationService,
                               IAuthorizationService authorizationService,
                               IMapper autoMapper,
                               ILogger<GroupChatController> logger,
                               LinkGenerator linkGenerator)
        {
            _groupChatService = groupChatService;
            _notificationService = notificationService;
            _authorizationService = authorizationService;
            _mapper = autoMapper;
            _logger = logger;
            _linkGenerator = linkGenerator;
        }

        //Member
        [HttpGet("getTrackBytes")]
        public async Task<IActionResult> GetTrackBytes(int groupId, string trackId)
        {
            if (!await IsGroupMemberAsync(groupId))
            {
                throw new UnauthorizedException($"User is not a member of the group {groupId}.", "not_member");
            }

            _logger.LogInformation($"Attempting to fetch track bytes: Group ID: {groupId} Track ID: {trackId}");

            TrackDTO trackDTO = await _groupChatService.GetLoadedTrackAsync(groupId, trackId);

            _logger.LogInformation($"Successfully fetched track bytes: Group ID: {groupId} Track ID: {trackId}");

            return File(trackDTO.TrackBytes, trackDTO.ContentType, $"{trackDTO.Name}.{trackDTO.Extension}");
        }

        //Member
        [HttpGet("getTrack")]
        public async Task<IActionResult> GetTrack(int groupId, string trackId)
        {
            if (!await IsGroupMemberAsync(groupId))
            {
                throw new UnauthorizedException($"User is not a member of the group {groupId}.", "not_member");
            }

            _logger.LogInformation($"Attempting to fetch track metadata: Group ID: {groupId} Track ID: {trackId}");

            TrackDTO trackDTO = await _groupChatService.GetTrackMetadataAsync(groupId, trackId);
            TrackResponse trackResponse = _mapper.Map<TrackResponse>(trackDTO);
            trackResponse.TrackFileLink = new Link()
            {
                Href = _linkGenerator.GetUriByAction(HttpContext, nameof(GetTrackBytes), values: new { groupId, trackId }),
                Rel = "trackBytes",
                Method = "GET"
            };

            _logger.LogInformation($"Successfully fetched track metadata: Group ID: {groupId} Track ID: {trackId}");

            return Ok(trackResponse);
        }

        //Member
        [HttpGet("getAllMessages")]
        public async Task<IActionResult> GetMessages(int groupId)
        {
            if (!await IsGroupMemberAsync(groupId))
            {
                throw new UnauthorizedException($"User is not a member of the group {groupId}.", "not_member");
            }

            _logger.LogInformation($"Attempting to fetch all messages. Group ID: {groupId}");

            ICollection<FullMessageDTO> messages = await _groupChatService.GetAllMessagesAsync(groupId);
            var responseData = _mapper.Map<ICollection<FullMessageResponse>>(messages);

            _logger.LogInformation($"Successfully fetched all messages. Group ID: {groupId}");

            var response = new CollectionResponse<FullMessageResponse>();
            response.Items = responseData;
            return Ok(response);
        }

        //Member
        [HttpGet("getMessages")]
        public async Task<IActionResult> GetMessages(int groupId, int pageSize, string? pagingState)
        {
            if (!await IsGroupMemberAsync(groupId))
            {
                throw new UnauthorizedException($"User is not a member of the group {groupId}.", "not_member");
            }

            _logger.LogInformation($"Attempting to fetch messages. Group ID: {groupId} Page Size: {pageSize} Paging State: {pagingState ?? string.Empty}");

            PageParamsDTO pageParamsDTO = new PageParamsDTO()
            {
                PageSize = pageSize,
                PagingState = pagingState
            };

            PagedDataDTO<ICollection<FullMessageDTO>> pagedData = await _groupChatService.GetAllMessagesAsync(groupId, pageParamsDTO);
            var responseData = _mapper.Map<PagedResponse<ICollection<FullMessageResponse>>>(pagedData);

            _logger.LogInformation($"Successfully fetched messages. Group ID: {groupId} Page Size: {pageSize} Paging State: {pagingState} Next Paging State: {pagedData.PageParams.NextPagingState}");

            return Ok(responseData);
        }

        //Member
        [HttpGet("getAllLatestMessages")]
        public async Task<IActionResult> GetMessages(int groupId, string createdAfter)
        {
            if (!await IsGroupMemberAsync(groupId))
            {
                throw new UnauthorizedException($"User is not a member of the group {groupId}.", "not_member");
            }

            _logger.LogInformation($"Attempting to fetch messages after a certain UTC time. Group ID: {groupId} After: {createdAfter}");

            if (!DateTime.TryParseExact(createdAfter, ExpectedDateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out DateTime createdAfterDT))
                throw new BadRequestException("Invalid date time value for the expected format specified.", "Invalid date time value for the expected format specified.", "bad_datetime_format", ExpectedDateTimeFormat);

            ICollection<FullMessageDTO> messages = await _groupChatService.GetMessagesAsync(groupId, createdAfterDT);
            var responseData = _mapper.Map<ICollection<FullMessageResponse>>(messages);

            _logger.LogInformation($"Successfully fetched messages after a certain UTC time. Group ID: {groupId} After: {createdAfter}");

            var response = new CollectionResponse<FullMessageResponse>();
            response.Items = responseData;
            return Ok(response);
        }

        //Member
        [HttpGet("getLatestMessages")]
        public async Task<IActionResult> GetMessages(int groupId, int pageSize, string? pagingState, string createdAfter)
        {
            if (!await IsGroupMemberAsync(groupId))
            {
                throw new UnauthorizedException($"User is not a member of the group {groupId}.", "not_member");
            }

            _logger.LogInformation($"Attempting to fetch messages after a certain UTC time. Group ID: {groupId} Page Size: {pageSize} Paging State: {pagingState ?? string.Empty} After: {createdAfter}");

            PageParamsDTO pageParamsDTO = new PageParamsDTO()
            {
                PageSize = pageSize,
                PagingState = pagingState
            };

            if (!DateTime.TryParseExact(createdAfter, ExpectedDateTimeFormat, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.AssumeUniversal, 
                out DateTime createdAfterDT))
                throw new BadRequestException("Invalid date time value for the expected format specified.", "Invalid date time value for the expected format specified.", "bad_datetime_format", ExpectedDateTimeFormat);

            PagedDataDTO<ICollection<FullMessageDTO>> pagedData = await _groupChatService.GetMessagesAsync(groupId, createdAfterDT, pageParamsDTO);
            var responseData = _mapper.Map<PagedResponse<ICollection<FullMessageResponse>>>(pagedData);

            _logger.LogInformation($"Successfully fetched messages after a certain UTC time. Group ID: {groupId} Page Size: {pageSize} Paging State: {pagingState} Next Paging State: {pagedData.PageParams.NextPagingState} After: {createdAfter}");

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

            return Ok(new { message = $"Successfully sent a text message (id: {addedMessageDTO.Id}) to group with id {addedMessageDTO.GroupId}." });
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

            return Ok(new { message = $"Successfully sent an image message (id: {addedMessageDTO.Id}) to group with id {addedMessageDTO.GroupId}." });
        }

        //Member
        [HttpPost("sendTrackMessage")]
        [RequestSizeLimit(MaxTrackRequestLength)]
        //[Consumes("multipart/form-data")]
        public async Task<IActionResult> SendTrackMessage([FromForm]IFormFile trackFile, [ModelBinder(BinderType = typeof(JsonModelBinder))] TrackMessageRequest messageData)
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

            return Ok(new { message = $"Successfully sent a track message (id: {addedMessageDTO.Id}) to group with id {addedMessageDTO.GroupId}." });
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
