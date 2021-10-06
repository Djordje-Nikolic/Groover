using AutoMapper;
using Groover.BL.Helpers;
using Groover.BL.Models;
using Groover.BL.Models.Chat.DTOs;
using Groover.BL.Models.DTOs;
using Groover.BL.Models.Exceptions;
using Groover.BL.Services.Interfaces;
using Groover.ChatDB;
using Groover.ChatDB.Interfaces;
using Groover.ChatDB.Models;
using Groover.IdentityDB.MySqlDb.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using Groover.BL.Models.Chat;

namespace Groover.BL.Services
{
    public class GroupChatService : IGroupChatService
    {
        private readonly IMapper _mapper;
        private readonly IGroupChatRepository _groupChatRepository;
        private readonly ILogger<GroupChatService> _logger;
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;
        private readonly IChatImageProcessor _imageProcessor;
        private readonly ITrackProcessor _trackProcessor;
        private ITrackRepository _trackRepository { get => _groupChatRepository.TrackRepository; }
        private IMessageRepository _messageRepository { get => _groupChatRepository.MessageRepository; }

        public GroupChatService(IMapper mapper,
            IGroupChatRepository groupChatRepository,
            ILogger<GroupChatService> logger,
            IGroupService groupService,
            IUserService userService,
            IChatImageProcessor imageProcessor,
            ITrackProcessor trackProcessor)
        {
            _mapper = mapper;
            _groupChatRepository = groupChatRepository;
            _logger = logger;
            _groupService = groupService;
            _userService = userService;
            _imageProcessor = imageProcessor;
            _trackProcessor = trackProcessor;
        }

        public async Task<FullMessageDTO> AddTextMessageAsync(TextMessageDTO textMessageDTO)
        {
            GroupDTO group = await _groupService.GetGroupAsync(textMessageDTO.GroupId);
            if (group == null)
                throw new NotFoundException($"Group doesn't exist. Group ID: {textMessageDTO.GroupId}", "not_found_group");

            UserDTO user = await _userService.GetUserAsync(textMessageDTO.SenderId);
            if (user == null)
                throw new NotFoundException($"User doesn't exist. User ID: {textMessageDTO.SenderId}", "not_found_user");

            DTOValidator<TextMessageDTO> textMessageValidator = new DTOValidator<TextMessageDTO>();
            textMessageValidator.Validate(textMessageDTO);
            if (textMessageValidator.IsValid == false)
                ErrorProcessor.Process(textMessageValidator.ValidationResults, _logger);

            try
            {
                _logger.LogInformation($"Attempting to add a text message: Group ID: {textMessageDTO.GroupId}");

                Message message = _mapper.Map<Message>(textMessageDTO);
                Message addedMessage = await _groupChatRepository.AddTextMessageAsync(message);

                _logger.LogInformation($"Successfully added a text message: Group ID: {textMessageDTO.GroupId} Message UUID: {addedMessage.Id}");

                FullMessageDTO messageDTO = _mapper.Map<FullMessageDTO>(addedMessage);
                return messageDTO;
            }
            catch (ArgumentException e)
            {
                throw new BadRequestException($"Text message is invalid: {e.Message}", "bad_message_format", e);
            }
            catch (Exception e)
            {
                throw new GrooverException($"Couldn't add the text message: {e.Message}.", "internal", e);
            }

        }

        public async Task<FullMessageDTO> AddImageMessageAsync(ImageMessageDTO imageMessageDTO)
        {
            GroupDTO group = await _groupService.GetGroupAsync(imageMessageDTO.GroupId);
            if (group == null)
                throw new NotFoundException($"Group doesn't exist. Group ID: {imageMessageDTO.GroupId}", "not_found_group");

            UserDTO user = await _userService.GetUserAsync(imageMessageDTO.SenderId);
            if (user == null)
                throw new NotFoundException($"User doesn't exist. User ID: {imageMessageDTO.SenderId}", "not_found_user");

            DTOValidator<ImageMessageDTO> imageMessageValidator = new DTOValidator<ImageMessageDTO>();
            imageMessageValidator.Validate(imageMessageDTO);
            if (imageMessageValidator.IsValid == false)
                ErrorProcessor.Process(imageMessageValidator.ValidationResults, _logger);

            await _imageProcessor.CheckAsync(imageMessageDTO.Image);

            try
            {
                _logger.LogInformation($"Attempting to add an image message: Group ID: {imageMessageDTO.GroupId}");

                Message message = _mapper.Map<Message>(imageMessageDTO);
                Message addedMessage = await _groupChatRepository.AddImageMessageAsync(message);

                _logger.LogInformation($"Successfully added an image message: Group ID: {imageMessageDTO.GroupId} Message UUID: {addedMessage.Id}");

                FullMessageDTO messageDTO = _mapper.Map<FullMessageDTO>(addedMessage);
                return messageDTO;
            }
            catch (ArgumentException e)
            {
                throw new BadRequestException($"Image message is invalid: {e.Message}", "bad_message_format", e);
            }
            catch (Exception e)
            {
                throw new GrooverException($"Couldn't add the image message: {e.Message}.", "internal", e);
            }
        }

        public async Task<FullMessageDTO> AddTrackMessageAsync(TrackMessageDTO trackMessageDTO, IFormFile trackFile)
        {
            GroupDTO group = await _groupService.GetGroupAsync(trackMessageDTO.GroupId);
            if (group == null)
                throw new NotFoundException($"Group doesn't exist. Group ID: {trackMessageDTO.GroupId}", "not_found_group");

            UserDTO user = await _userService.GetUserAsync(trackMessageDTO.SenderId);
            if (user == null)
                throw new NotFoundException($"User doesn't exist. User ID: {trackMessageDTO.SenderId}", "not_found_user");

            DTOValidator<TrackMessageDTO> trackMessageValidator = new DTOValidator<TrackMessageDTO>();
            trackMessageValidator.Validate(trackMessageDTO);
            if (trackMessageValidator.IsValid == false)
                ErrorProcessor.Process(trackMessageValidator.ValidationResults, _logger);

            Track track = await _trackProcessor.ProcessTrackAsync(trackFile);
            track.Name = trackMessageDTO.TrackName.Normalize();

            try
            {
                _logger.LogInformation($"Attempting to add a track message: Group ID: {trackMessageDTO.GroupId}");

                Message message = _mapper.Map<Message>(trackMessageDTO);
                Message addedMessage = await _groupChatRepository.AddTrackMessageAsync(message, track);

                _logger.LogInformation($"Successfully added a track message: Group ID: {trackMessageDTO.GroupId} Message UUID: {addedMessage.Id}");

                FullMessageDTO messageDTO = _mapper.Map<FullMessageDTO>(addedMessage);
                return messageDTO;
            }
            catch (ArgumentException e)
            {
                throw new BadRequestException($"Track message is invalid: {e.Message}", "bad_message_format", e);
            }
            catch (Exception e)
            {
                throw new GrooverException($"Couldn't add the track message: {e.Message}.", "internal", e);
            }
        }

        public async Task<TrackDTO> GetTrackAsync(int groupId, string trackUuId)
        {
            if (groupId <= 0)
                throw new BadRequestException($"Invalid group id: {groupId}.", "bad_id");
            if (string.IsNullOrWhiteSpace(trackUuId))
                throw new BadRequestException("Track UUID can't be null or empty.", "bad_uuid");

            GroupDTO group = await _groupService.GetGroupAsync(groupId);
            if (group == null)
                throw new NotFoundException($"Group doesn't exist. Group ID: {groupId}", "not_found_group");

            try
            {
                _logger.LogInformation($"Attempting to fetch a track from a group. Track UUID: {trackUuId} Group ID: {groupId}");

                Track track = await _trackRepository.GetAsync(trackUuId);
                if (track == null)
                    throw new NotFoundException("Track with that id does not exist.", "not_found");

                await _trackRepository.LoadAsync(track, false);

                TrackDTO trackDTO = _mapper.Map<TrackDTO>(track);

                _logger.LogInformation($"Successfully fetched a track from a group. Track UUID: {trackDTO.Id} Group ID: {groupId}");

                return trackDTO;
            }
            catch (ArgumentException e)
            {
                throw new BadRequestException($"Track UUID is invalid: {e.Message}", "bad_uuid", e);
            }
            catch (Exception e)
            {
                throw new GrooverException($"Unknown error occured: {e.Message}.", "internal", e);
            }
        }

        public async Task<PagedDataDTO<ICollection<FullMessageDTO>>> GetAllMessagesAsync(int groupId, PageParamsDTO pageParamsDTO)
        {
            if (groupId <= 0)
                throw new BadRequestException($"Invalid group id: {groupId}.", "bad_id");
            if (pageParamsDTO == null)
                throw new BadRequestException("Undefined page params.", "undefined");

            GroupDTO group = await _groupService.GetGroupAsync(groupId);
            if (group == null)
                throw new NotFoundException($"Group doesn't exist. Group ID: {groupId}", "not_found_group");

            try
            {
                PageParams pageParams = _mapper.Map<PageParams>(pageParamsDTO);

                _logger.LogInformation($"Attempting to fetch all messages from a group: " +
                    $"Group ID: {groupId} " +
                    $"Page Size: {pageParams.PageSize} " +
                    $"Paging State: {pageParams.PagingState ?? "First Page"}");

                ICollection<Message> messages = await _messageRepository.GetByGroupAsync(groupId, pageParams);

                ICollection<FullMessageDTO> messageDTOs = _mapper.Map<ICollection<FullMessageDTO>>(messages);
                PageParamsDTO returnPageParamsDTO = _mapper.Map<PageParamsDTO>(pageParams);

                _logger.LogInformation($"Successfully fetched messages from a group:" +
                    $"Group ID: {groupId} " +
                    $"Number of returned messages: {messageDTOs.Count}" +
                    $"Page Size: {pageParams.PageSize} " +
                    $"Paging State: {pageParams.PagingState ?? "First Page"}" +
                    $"Next Paging State: {pageParams.NextPagingState}");

                return new PagedDataDTO<ICollection<FullMessageDTO>>()
                {
                    Data = messageDTOs,
                    PageParams = returnPageParamsDTO
                };
            }
            catch (Exception e)
            {
                throw new GrooverException($"Unknown error occured: {e.Message}.", "internal", e);
            }
        }

        public async Task<PagedDataDTO<ICollection<FullMessageDTO>>> GetMessagesAsync(int groupId, DateTime dateTimeAfter, PageParamsDTO pageParamsDTO)
        {
            if (groupId <= 0)
                throw new BadRequestException($"Invalid group id: {groupId}.", "bad_id");
            if (pageParamsDTO == null)
                throw new BadRequestException("Undefined page params.", "undefined");

            GroupDTO group = await _groupService.GetGroupAsync(groupId);
            if (group == null)
                throw new NotFoundException($"Group doesn't exist. Group ID: {groupId}", "not_found_group");

            try
            {
                PageParams pageParams = _mapper.Map<PageParams>(pageParamsDTO);

                _logger.LogInformation($"Attempting to fetch all messages from a group, after a certain point in time: " +
                    $"Group ID: {groupId} " +
                    $"After DateTime: {dateTimeAfter}" +
                    $"Page Size: {pageParams.PageSize} " +
                    $"Paging State: {pageParams.PagingState ?? "First Page"}");

                ICollection<Message> messages = await _messageRepository.GetAfterAsync(groupId, dateTimeAfter, pageParams);

                ICollection<FullMessageDTO> messageDTOs = _mapper.Map<ICollection<FullMessageDTO>>(messages);
                PageParamsDTO returnPageParamsDTO = _mapper.Map<PageParamsDTO>(pageParams);

                _logger.LogInformation($"Successfully fetched messages from a group:" +
                    $"Group ID: {groupId} " +
                    $"After DateTime: {dateTimeAfter}" +
                    $"Number of returned messages: {messageDTOs.Count}" +
                    $"Page Size: {pageParams.PageSize} " +
                    $"Paging State: {pageParams.PagingState ?? "First Page"}" +
                    $"Next Paging State: {pageParams.NextPagingState}");

                return new PagedDataDTO<ICollection<FullMessageDTO>>()
                {
                    Data = messageDTOs,
                    PageParams = returnPageParamsDTO
                };
            }
            catch (Exception e)
            {
                throw new GrooverException($"Unknown error occured: {e.Message}.", "internal", e);
            }
        }

    }
}
