using AutoMapper;
using Groover.API.Models.Requests;
using Groover.API.Models.Responses;
using Groover.BL.Handlers.Requirements;
using Groover.BL.Models.DTOs;
using Groover.BL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IAuthorizationService _authorizationService;
        private readonly IMapper _autoMapper;
        private readonly ILogger<GroupController> _logger;

        public GroupController(IGroupService groupService, 
                               IUserService userService,
                               IAuthorizationService authorizationService,
                               IMapper autoMapper, 
                               ILogger<GroupController> logger)
        {
            _groupService = groupService;
            _userService = userService;
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
                return new ForbidResult();
            }

            GroupDTO groupDTO = await _groupService.GetGroupAsync(id);
            _logger.LogInformation($"Successfully found the group by id {id}");

            var response = _autoMapper.Map<GroupResponse>(groupDTO);
            return Ok(response);
        }

        //Anyone
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateGroupRequest createGroupRequest)
        {
            _logger.LogInformation($"Attempting to create a group by name {createGroupRequest.Name}.");

            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            GroupDTO groupDTO = _autoMapper.Map<GroupDTO>(createGroupRequest);
            GroupDTO createdDTO = await _groupService.CreateGroupAsync(groupDTO, userId.Value);
            var response = _autoMapper.Map<GroupResponse>(createdDTO);

            _logger.LogInformation($"Successfully create a group by name {createGroupRequest.Name}.");

            return Ok(response);
        }

        //Admin
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation($"Attempting to delete the group by id {id}.");

            if (!await IsGroupAdminAsync(id))
            {
                return Forbid();
            }

            await _groupService.DeleteAsync(id);

            _logger.LogInformation($"Successfully deleted the group by id {id}.");

            return Ok(new { message = $"Successfully deleted the group by id {id}." });
        }

        //Admin
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateGroupRequest updateGroupRequest)
        {
            _logger.LogInformation($"Attempting to update the group by id {updateGroupRequest.Id}.");

            if (!await IsGroupAdminAsync(updateGroupRequest.Id))
            {
                return Forbid();
            }

            GroupDTO groupDTO = _autoMapper.Map<GroupDTO>(updateGroupRequest);
            GroupDTO updatedDTO = await _groupService.UpdateGroupAsync(groupDTO);
            var response = _autoMapper.Map<GroupResponse>(updatedDTO);

            _logger.LogInformation($"Successfully updated the group by id {updateGroupRequest.Id}.");

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("acceptInvite")]
        public async Task<IActionResult> AcceptInvite([FromBody] AcceptInvitationRequest request)
        {
            _logger.LogInformation($"Attempting to accept an invitation for user {request.UserId} to group {request.GroupId}.");

            await _groupService.AcceptInviteAsync(request.Token, request.GroupId, request.UserId);

            _logger.LogInformation($"Successfully accepted an invitation for user {request.UserId} to group {request.GroupId}.");

            return Ok(new { message = $"Successfully accepted an invitation for user { request.UserId} to group { request.GroupId}."});
        }


        //Admin
        [HttpPatch("inviteUser")]
        public async Task<IActionResult> InviteUser(int groupId, int userId)
        {
            _logger.LogInformation($"Attempting to invite user {userId} to group {groupId}.");

            if (!await IsGroupAdminAsync(groupId))
            {
                return Forbid();
            }

            var invitationDTO = await _groupService.InviteUserAsync(groupId, userId);
            var senderId = GetUserId();
            if (senderId == null)
                return Unauthorized();

            var acceptUrl = GenerateInvitationUrl(invitationDTO.InvitationToken, invitationDTO.Group.Id, invitationDTO.User.Id);
            await _groupService.SendInvitationEmailAsync(acceptUrl, invitationDTO.Group, invitationDTO.User, senderId.Value);

            _logger.LogInformation($"Successfully invited user {userId} to group {groupId}.");

            return Ok(new { message = $"Successfully invited user {userId} to group {groupId}." });
        }

        //Admin OR removing yourself
        [HttpPatch("removeUser")]
        public async Task<IActionResult> RemoveUser(int groupId, int userId)
        {
            _logger.LogInformation($"Attempting to remove user {userId} from group {groupId}.");

            UserDTO userToRemove = await _userService.GetUserAsync(userId);
            var resultIsUser = await _authorizationService.AuthorizeAsync(User, userToRemove, new IsUserRequirement());

            if (!await IsGroupAdminAsync(groupId) && !resultIsUser.Succeeded)
            {
                return Forbid();
            }

            await _groupService.RemoveUserAsync(groupId, userId);

            _logger.LogInformation($"Successfully removed user {userId} from group {groupId}.");

            return Ok(new { message = $"Successfully removed user {userId} from group {groupId}." });
        }

        //Admin
        [HttpPatch("updateRole")]
        public async Task<IActionResult> UpdateUserRole(int groupId, int userId, string newRole)
        {
            _logger.LogInformation($"Attempting to change role of user {userId} in group {groupId}.");

            if (!await IsGroupAdminAsync(groupId))
            {
                return Forbid();
            }

            await _groupService.UpdateUserRoleAsync(groupId, userId, newRole);

            _logger.LogInformation($"Successfully updated role of user {userId} in group {groupId}.");

            return Ok(new { message = $"Successfully updated role of user {userId} in group {groupId}." });
        }

        private async Task<bool> IsGroupAdminAsync(int groupId)
        {
            GroupDTO group = await _groupService.GetGroupAsync(groupId);
            var resultIsAdmin = await _authorizationService.AuthorizeAsync(User, group, new GroupRoleRequirement(GroupClaimTypeConstants.Admin));

            return resultIsAdmin.Succeeded;
        }

        private async Task<bool> IsGroupMemberAsync(int groupId)
        {
            GroupDTO group = await _groupService.GetGroupAsync(groupId);
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
