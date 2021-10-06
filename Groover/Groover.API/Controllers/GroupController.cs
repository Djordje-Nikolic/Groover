using AutoMapper;
using Groover.API.Models.Requests;
using Groover.API.Models.Responses;
using Groover.API.Services.Interfaces;
using Groover.BL.Handlers.Requirements;
using Groover.BL.Models.DTOs;
using Groover.BL.Models.Exceptions;
using Groover.BL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Groover.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class GroupController : ControllerBase
    {//add logging

        private readonly IGroupService _groupService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMapper _autoMapper;
        private readonly ILogger<GroupController> _logger;

        public GroupController(IGroupService groupService, 
                               IUserService userService,
                               INotificationService notificationService,
                               IAuthorizationService authorizationService,
                               IMapper autoMapper, 
                               ILogger<GroupController> logger)
        {
            _groupService = groupService;
            _userService = userService;
            _notificationService = notificationService;
            _authorizationService = authorizationService;
            _autoMapper = autoMapper;
            _logger = logger;
        }

        //Member
        [HttpGet("getById")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation($"Attempting to get the group by id {id}.");

            if (!await IsGroupMemberAsync(id))
            {
                throw new UnauthorizedException($"User is not group {id} member.", "not_member");
            }

            GroupDTO groupDTO = await _groupService.GetGroupAsync(id);
            _logger.LogInformation($"Successfully found the group by id {id}");

            var response = _autoMapper.Map<GroupResponse>(groupDTO);
            return Ok(response);
        }

        //Anyone
        [HttpGet("getImage")]
        public async Task<IActionResult> GetImage(int groupId)
        {
            _logger.LogInformation($"Attempting to retrieve the group image: {groupId}");

            var groupDTO = await _groupService.GetGroupAsync(groupId);

            _logger.LogInformation($"Successfully retrieved the group image: {groupId}");
            return File(groupDTO.Image, "image/*");
        }

        //Anyone
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateGroupRequest createGroupRequest)
        {
            _logger.LogInformation($"Attempting to create a group by name {createGroupRequest.Name}.");

            var userId = GetUserId();
            if (userId == null)
                throw new UnauthorizedException("User id undefined.", "bad_id");

            GroupDTO groupDTO = _autoMapper.Map<GroupDTO>(createGroupRequest);
            GroupDTO createdDTO = await _groupService.CreateGroupAsync(groupDTO, userId.Value);
            _logger.LogInformation($"Successfully created a group by name {createGroupRequest.Name}.");

            var response = _autoMapper.Map<GroupResponse>(createdDTO);
            GroupUserDTO ugData = (await _userService.GetUserAsync(userId.Value))
                                   .UserGroups.Where(ug => ug.GroupId == createdDTO.Id)
                                   .First();
            var notificationData = _autoMapper.Map<UserGroupResponse>(ugData);

            //Send notification and invalidated tokens
            await _notificationService.ForceTokenRefreshAsync(userId.Value.ToString());
            await _notificationService.GroupCreatedAsync(notificationData, userId.ToString());

            return Ok(response);
        }

        //Admin
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation($"Attempting to delete the group by id {id}.");

            if (!await IsGroupAdminAsync(id))
            {
                throw new UnauthorizedException($"User is not group {id} admin.", "not_admin");
            }

            await _groupService.DeleteAsync(id);

            _logger.LogInformation($"Successfully deleted the group by id {id}.");

            //Send notifications
            await _notificationService.GroupDeletedAsync(id.ToString());

            return Ok(new { message = $"Successfully deleted the group by id {id}." });
        }

        //Admin
        [HttpPatch("setImage")]
        public async Task<IActionResult> SetImage([FromForm] SetGroupImageRequest request)
        {
            _logger.LogInformation($"Attempting to set an image for group: {request.GroupId}");

            if (!await IsGroupAdminAsync(request.GroupId))
            {
                throw new UnauthorizedException($"User is not group {request.GroupId} admin.", "not_admin");
            }

            var updatedGroup = await _groupService.SetImage(request.GroupId, request.ImageFile);
            _logger.LogInformation($"Successfully set an image for group: {request.GroupId}");

            var response = this._autoMapper.Map<GroupResponse>(updatedGroup);
            var notificationData = this._autoMapper.Map<GroupDataResponse>(response);

            //Send notification
            await this._notificationService.GroupUpdatedAsync(notificationData);

            return Ok(response);
        }

        //Admin
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateGroupRequest updateGroupRequest)
        {
            _logger.LogInformation($"Attempting to update the group by id {updateGroupRequest.Id}.");

            if (!await IsGroupAdminAsync(updateGroupRequest.Id))
            {
                throw new UnauthorizedException($"User is not group {updateGroupRequest.Id} admin.", "not_admin");
            }

            GroupDTO groupDTO = _autoMapper.Map<GroupDTO>(updateGroupRequest);
            GroupDTO updatedDTO = await _groupService.UpdateGroupAsync(groupDTO); 
            _logger.LogInformation($"Successfully updated the group by id {updateGroupRequest.Id}.");

            var response = _autoMapper.Map<GroupResponse>(updatedDTO);
            var notificationData = this._autoMapper.Map<GroupDataResponse>(response);

            //Send notification
            await this._notificationService.GroupUpdatedAsync(notificationData);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("acceptInvite")]
        public async Task<IActionResult> AcceptInvite(string token, int groupId, int userId)
        {
            AcceptInvitationRequest request = new AcceptInvitationRequest()
            {
                Token = token,
                UserId = userId,
                GroupId = groupId
            };

            _logger.LogInformation($"Attempting to accept an invitation for user {request.UserId} to group {request.GroupId}.");

            var groupUserDTO = await _groupService.AcceptInviteAsync(request.Token, request.GroupId, request.UserId);
            _logger.LogInformation($"Successfully accepted an invitation for user {request.UserId} to group {request.GroupId}.");

            //Send notification and invalidate token
            var groupUserLiteResp = _autoMapper.Map<GroupUserLiteResponse>(groupUserDTO);
            var userGroupResp = _autoMapper.Map<UserGroupResponse>(groupUserDTO);

            await _notificationService.ForceTokenRefreshAsync(request.UserId.ToString());
            await _notificationService.UserJoinedGroupAsync(request.GroupId.ToString(), groupUserLiteResp, userGroupResp);

            return Ok(new { message = $"Successfully accepted an invitation for user { request.UserId} to group { request.GroupId}."});
        }


        //Admin
        [HttpPatch("inviteUser")]
        public async Task<IActionResult> InviteUser(int groupId, int userId)
        {
            _logger.LogInformation($"Attempting to invite user {userId} to group {groupId}.");

            if (!await IsGroupAdminAsync(groupId))
            {
                throw new UnauthorizedException($"User is not group {groupId} admin.", "not_admin");
            }

            var invitationDTO = await _groupService.InviteUserAsync(groupId, userId);
            var senderId = GetUserId();
            if (senderId == null)
                throw new UnauthorizedException($"User id undefined.", "bad_id");


            var acceptUrl = GenerateInvitationUrl(invitationDTO.InvitationToken, invitationDTO.Group.Id, invitationDTO.User.Id);
            await _groupService.SendInvitationEmailAsync(acceptUrl, invitationDTO.Group, invitationDTO.User, senderId.Value);
            _logger.LogInformation($"Successfully invited user {userId} to group {groupId}.");

            //Send notification for invite
            GroupLiteResponse group = _autoMapper.Map<GroupLiteResponse>(invitationDTO.Group);
            await _notificationService.UserInvitedAsync(invitationDTO.InvitationToken, group, userId.ToString());

            return Ok(new { message = $"Successfully invited user {userId} to group {groupId}." });
        }

        //Admin OR removing yourself
        [HttpPatch("removeUser")]
        public async Task<IActionResult> RemoveUser(int groupId, int userId)
        {
            _logger.LogInformation($"Attempting to remove user {userId} from group {groupId}.");

            UserDTO userToRemove = new() { Id = userId };
            var resultIsUser = await _authorizationService.AuthorizeAsync(User, userToRemove, new IsUserRequirement());

            if (!await IsGroupAdminAsync(groupId) && !resultIsUser.Succeeded)
            {
                throw new UnauthorizedException($"User is not group {groupId} admin.", "not_admin");
            }

            var lastMember = await _groupService.RemoveUserAsync(groupId, userId);
            _logger.LogInformation($"Successfully removed user {userId} from group {groupId}.");

            //Send notification and invalidate token
            if (lastMember)
            {
                await _groupService.DeleteAsync(groupId);
                await _notificationService.GroupDeletedAsync(groupId.ToString());
            }
            else
            {
                await _notificationService.UserLeftGroupAsync(groupId.ToString(), userId.ToString());
            }
            await _notificationService.ForceTokenRefreshAsync(userId.ToString());

            return Ok(new { message = $"Successfully removed user {userId} from group {groupId}." });
        }

        //Admin
        [HttpPatch("updateRole")]
        public async Task<IActionResult> UpdateUserRole(int groupId, int userId, string newRole)
        {
            _logger.LogInformation($"Attempting to change role of user {userId} in group {groupId}.");

            if (!await IsGroupAdminAsync(groupId))
            {
                throw new UnauthorizedException($"User is not group {groupId} admin.", "not_admin");
            }

            newRole = await _groupService.UpdateUserRoleAsync(groupId, userId, newRole);
            _logger.LogInformation($"Successfully updated role of user {userId} in group {groupId}.");

            //Send notification and invalidate token
            await _notificationService.ForceTokenRefreshAsync(userId.ToString());
            await _notificationService.UserRoleUpdatedAsync(groupId.ToString(), userId.ToString(), newRole);

            return Ok(new { message = $"Successfully updated role of user {userId} in group {groupId}." });
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

        private string GenerateInvitationUrl(string token, int groupId, int userId)
        {
            //Test this
            return Url.Action(nameof(AcceptInvite), "group", new { token, groupId, userId }, Request.Scheme);
        }
    }
}
