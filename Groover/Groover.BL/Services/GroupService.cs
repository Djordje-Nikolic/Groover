using AutoMapper;
using Groover.BL.Helpers;
using Groover.BL.Models;
using Groover.BL.Models.DTOs;
using Groover.BL.Models.Exceptions;
using Groover.BL.Services.Interfaces;
using Groover.BL.Utils;
using Groover.IdentityDB.MySqlDb;
using Groover.IdentityDB.MySqlDb.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Services
{
    public class GroupService : IGroupService
    {//add logging

        private const string InviteTokenPurposeFormat = "group_invite{0}";

        private readonly GrooverDbContext _context;
        private readonly ILogger<GroupService> _logger;
        private readonly IMapper _mapper;
        private readonly ITokenProviderAccessor<User> _tokenProviderAccessor;
        private readonly IUserTwoFactorTokenProvider<User> _inviteTokenProvider;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<User> _userManager;
        private readonly IAvatarImageProcessor _imageProcessor;

        public GroupService(GrooverDbContext context,
            IMapper mapper,
            ITokenProviderAccessor<User> tokenProviderAccessor,
            ILogger<GroupService> logger,
            IEmailSender emailSender,
            IAvatarImageProcessor imageProcessor,
            UserManager<User> userManager)
        {
            this._tokenProviderAccessor = tokenProviderAccessor;
            this._context = context;
            this._logger = logger;
            this._mapper = mapper;
            this._userManager = userManager;
            this._emailSender = emailSender;
            this._imageProcessor = imageProcessor;
            this._inviteTokenProvider = _tokenProviderAccessor.GetTokenProvider(Constants.GroupInviteTokenProvider);
        }

        public async Task<GroupDTO> CreateGroupAsync(GroupDTO groupDTO, int userId)
        {
            if (groupDTO == null)
                throw new BadRequestException("Group data undefined.","undefined");
            if (userId <= 0)
                throw new BadRequestException("Invalid group id.", "bad_id");

            DTOValidator<GroupDTO> validator = new DTOValidator<GroupDTO>();
            validator.Validate(groupDTO);
            if (validator.IsValid == false)
                ErrorProcessor.Process(validator.ValidationResults, _logger);

            //Preprocess
            User user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new NotFoundException($"No user by id {userId}.", "not_found");

            //Removed unique name constraint
            //if (await _context.Groups.AnyAsync(g => g.Name == groupDTO.Name) == true)
            //    throw new BadRequestException("Group with that name already exists.", "duplicate_name");

            Group group = _mapper.Map<Group>(groupDTO);
            

            if (groupDTO.Image != null && groupDTO.Image.Length > 0)
            {
                var imageBytes = await this._imageProcessor.CheckAsync(groupDTO.Image);
                var imagePath = await this._imageProcessor.SaveImageAsync(imageBytes);
                group.ImagePath = imagePath;
            }
            else
            {
                group.ImagePath = this._imageProcessor.GetDefaultGroupImage();
            }

            GroupUser groupUser = new GroupUser();
            groupUser.Group = group;
            groupUser.User = user;
            groupUser.GroupRole = GroupRole.Admin;

            _context.Groups.Add(group);
            _context.GroupUsers.Add(groupUser);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error creating a group in the database: {e.Message}");
                throw new BadRequestException("Error creating a group.", "update_error");
            }

            group = await _context.Groups.Where(g => g.Id == group.Id)
                .Include(g => g.GroupUsers)
                .ThenInclude(gu => gu.User)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var createdDTO = _mapper.Map<GroupDTO>(group);
            return createdDTO;
        }

        public async Task<GroupDTO> SetImage(int groupId, IFormFile imageFile)
        {
            if (groupId <= 0)
                throw new BadRequestException("Invalid group id.", "bad_id");

            var group = await _context.Groups
                .Where(group => group.Id == groupId)
                .Include(group => group.GroupUsers)
                .ThenInclude(gu => gu.User)
                .FirstOrDefaultAsync();

            if (group == null)
            {
                throw new NotFoundException($"Group with id {groupId} not found.", "not_found");
            }

            var imageBytes = await this._imageProcessor.ProcessAsync(imageFile);
            if (imageBytes != group.Image)
            {
                if (!string.IsNullOrWhiteSpace(group.ImagePath) &&
                    group.ImagePath != _imageProcessor.GetDefaultGroupImage())
                {
                    this._imageProcessor.DeleteImage(group.ImagePath);
                }

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    var imagePath = await this._imageProcessor.SaveImageAsync(imageBytes);
                    group.ImagePath = imagePath;
                }
                else
                {
                    group.ImagePath = this._imageProcessor.GetDefaultGroupImage();
                }
            }

            _context.Groups.Update(group);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error updating a Group image in the database. Group id {group.Id}: {e.Message}");
                throw new BadRequestException("Error updating a Group image.", "update_error");
            }

            var groupDto = this._mapper.Map<GroupDTO>(group);
            return groupDto;
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new BadRequestException("Invalid group id.", "bad_id.");

            var group = await _context.Groups.FindAsync(id);
            if (group == null)
                throw new NotFoundException($"No group by id {id}.", "not_found");

            if (!string.IsNullOrWhiteSpace(group.ImagePath) &&
                group.ImagePath != _imageProcessor.GetDefaultGroupImage())
            {
                this._imageProcessor.DeleteImage(group.ImagePath);
            }

            var groupUsers = await _context.GroupUsers.Where(gu => gu.GroupId == id).ToListAsync();
            _context.GroupUsers.RemoveRange(groupUsers);
            _context.Groups.Remove(group);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error deleting a Group from the database. Group id {group.Id}: {e.Message}");
                throw new BadRequestException("Error deletin a Group.", "update_error");
            }
        }

        public async Task<GroupDTO> GetGroupAsync(int groupId)
        {
            if (groupId <= 0)
                throw new BadRequestException("Invalid group id.", "bad_id");

            var group = await _context.Groups
                .Where(gr => gr.Id == groupId)
                .Include(gr => gr.GroupUsers)
                .ThenInclude(gu => gu.User)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (group == null)
                throw new NotFoundException($"No group by id {groupId}.", "not_found");

            var groupDTO = _mapper.Map<GroupDTO>(group);
            return groupDTO;
        }

        public async Task<ICollection<GroupDTO>> GetGroupsAsync(ICollection<int> groupIds)
        {
            if (groupIds.Any(id => id <= 0))
                throw new BadRequestException("One of the ids is invalid.", "bad_id");

            var groups = await _context.Groups
                .Where(gr => groupIds.Contains(gr.Id))
                .Include(gr => gr.GroupUsers)
                .ThenInclude(gu => gu.User)
                .AsNoTracking()
                .ToListAsync();

            var groupDTOs = _mapper.Map<ICollection<GroupDTO>>(groups);
            return groupDTOs;
        }

        public async Task<InvitationDTO> InviteUserAsync(int groupId, int userId)
        {
            if (groupId <= 0)
                throw new BadRequestException("Group id is invalid.", "bad_id");
            if (userId <= 0)
                throw new BadRequestException("User id is invalid.", "bad_id");

            GroupUser groupUser = await _context.GroupUsers.FindAsync(groupId, userId);
            if (groupUser != null)
                throw new BadRequestException("User is already a member of the group.", "already_member");

            Group group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                throw new NotFoundException($"No group by id {groupId}.", "not_found_group");

            User user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new NotFoundException($"No user by id {userId}.", "not_found");

            var token = await GenerateInviteTokenAsync(groupId, user);

            _logger.LogInformation("Invitation successful. Token generated, awaiting confirmation. ");
            InvitationDTO invitationDTO = new InvitationDTO();
            invitationDTO.User = _mapper.Map<UserDTO>(user);
            invitationDTO.Group = _mapper.Map<GroupDTO>(group);
            invitationDTO.InvitationToken = token;

            return invitationDTO;
        }

        public async Task SendInvitationEmailAsync(string acceptUrl, GroupDTO group, UserDTO receiver, int senderId)
        {
            User sender = await _userManager.FindByIdAsync(senderId.ToString());
            if (sender == null)
                throw new BadRequestException("Sender doesn't exist.", "not_found");

            var content = GenerateInviteContent(acceptUrl, group, receiver, sender);
            var message = new EmailMessage(new string[] { receiver.Email }, "Groover Group Invitation", content, null);
            await _emailSender.SendEmailAsync(message);
        }

        public async Task<GroupUserDTO> AcceptInviteAsync(string token, int groupId, int userId)
        {
            if (groupId <= 0)
                throw new BadRequestException("Group id is invalid.", "bad_id");
            if (userId <= 0)
                throw new BadRequestException("User id is invalid.", "bad_id");

            Group group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                throw new NotFoundException($"No group by id {groupId}.", "not_found");

            User user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new NotFoundException($"No user by id {userId}.", "not_found");

            var isTokenValid = await ValidateInviteTokenAsync(token, groupId, user);
            if (!isTokenValid)
                throw new UnauthorizedException("Invitation token is invalid.", "token_invalid");

            GroupUser groupUser = await _context.GroupUsers.FindAsync(groupId, userId);
            if (groupUser != null)
                throw new BadRequestException("User is already a member of the group.", "already_member");

            groupUser = new GroupUser();
            groupUser.User = user;
            groupUser.Group = group;
            groupUser.GroupRole = GroupRole.Member;
            await _context.GroupUsers.AddAsync(groupUser);
            await _context.SaveChangesAsync();

            //Might need to cut down on some of these includes
            groupUser = await _context.GroupUsers
                .Where(gu => gu.GroupId == groupId && gu.UserId == userId)
                .Include(gu => gu.User)
                .Include(gu => gu.Group)
                .ThenInclude(g => g.GroupUsers)
                .ThenInclude(gu => gu.User)
                .FirstOrDefaultAsync();

            var groupUserDTO = this._mapper.Map<GroupUserDTO>(groupUser);
            return groupUserDTO;
        }

        public async Task<bool> RemoveUserAsync(int groupId, int userId)
        {
            if (groupId <= 0)
                throw new BadRequestException("Group id is invalid.", "bad_id");
            if (userId <= 0)
                throw new BadRequestException("User id is invalid.", "bad_id");

            Group group = await _context.Groups
                .Where(gr => gr.Id == groupId)
                .Include(gr => gr.GroupUsers)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (group == null)
                throw new NotFoundException("Group doesn't exist.", "not_found_group");

            List<GroupUser> groupUsers = group.GroupUsers.ToList(); 
            GroupUser userToBeRemoved = groupUsers.FirstOrDefault(gu => gu.UserId == userId);
            if (userToBeRemoved == null)
                throw new NotFoundException("User is not a member of the group.", "not_found");

            if (userToBeRemoved.GroupRole == GroupRole.Admin &&
                groupUsers.Count > 1)
            {
                var adminCount = groupUsers.Count(gu => gu.GroupRole == GroupRole.Admin);
                if (adminCount == 1)
                    throw new BadRequestException("User is last admin, cannot leave.", "last_admin");
            }

            _context.GroupUsers.Remove(userToBeRemoved);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error removing a User from the group in the database. Group id {group.Id} User id {userToBeRemoved.User.Id}: {e.Message}");
                throw new BadRequestException("Error removing a User from a group.", "update_error");
            }

            //Return whether the user was last in group
            groupUsers.Remove(userToBeRemoved);
            if (groupUsers.Count == 0)
                return true;
            else
                return false;
        }

        public async Task<GroupDTO> UpdateGroupAsync(GroupDTO groupDTO)
        {
            if (groupDTO == null)
                throw new BadRequestException("Group data undefined.", "undefined");

            DTOValidator<GroupDTO> validator = new DTOValidator<GroupDTO>();
            validator.Validate(groupDTO);
            if (validator.IsValid == false)
                ErrorProcessor.Process(validator.ValidationResults, _logger);

            //Preprocess
            Group group = await _context.Groups.FindAsync(groupDTO.Id);
            if (group == null)
                throw new NotFoundException($"No group by id {groupDTO.Id}.", "not_found");

            Group groupNew = _mapper.Map<Group>(groupDTO);
            group.Name = groupNew.Name;
            group.Description = groupNew.Description;

            var imageBytes = await this._imageProcessor.CheckAsync(groupDTO.Image);
            if (imageBytes != group.Image)  //This is probably always gonna be different, as this doesnt do a deep comparison
            {
                if (!string.IsNullOrWhiteSpace(group.ImagePath) &&
                    group.ImagePath != _imageProcessor.GetDefaultGroupImage())
                {
                    this._imageProcessor.DeleteImage(group.ImagePath);
                }

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    var imagePath = await this._imageProcessor.SaveImageAsync(imageBytes);
                    group.ImagePath = imagePath;
                }
                else
                {
                    group.ImagePath = this._imageProcessor.GetDefaultGroupImage();
                }
            }

            _context.Groups.Update(group);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error updating a group in the database. Group id {group.Id}: {e.Message}");
                throw new BadRequestException("Error updating a group.", "update_error");
            }

            group = await _context.Groups.Where(g => g.Id == group.Id)
                .Include(g => g.GroupUsers)
                .ThenInclude(gu => gu.User)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var updatedDTO = _mapper.Map<GroupDTO>(group);
            return updatedDTO;
        }

        public async Task<string> UpdateUserRoleAsync(int groupId, int userId, string newRole)
        {
            if (groupId <= 0)
                throw new BadRequestException("Group id is invalid.", "bad_id");
            if (userId <= 0)
                throw new BadRequestException("User id is invalid.", "bad_id");
            if (string.IsNullOrWhiteSpace(newRole))
                throw new BadRequestException("New role is undefined.", "bad_role");

            if (!Enum.TryParse(typeof(GroupRole), newRole, out object roleParseResult))
                throw new BadRequestException("New role is invalid.", "bad_role");

            GroupRole groupRoleNew = (GroupRole)roleParseResult;

            Group group = await _context.Groups
                                .Where(g => g.Id == groupId)
                                .Include(g => g.GroupUsers)
                                .FirstOrDefaultAsync();
            if (group == null)
                throw new NotFoundException("Couldn't find the group.", "not_found_group");

            ICollection<GroupUser> groupUsers = group.GroupUsers;
            GroupUser groupUser = groupUsers.FirstOrDefault(gu => gu.UserId == userId);
            if (groupUser == null)
                throw new NotFoundException("User is not a member of the group.", "not_found");

            //Check if user is last admin, and is trying to demote himself
            if (groupUser.GroupRole == GroupRole.Admin &&
                groupRoleNew != GroupRole.Admin)
            {
                var numberOfAdmins = groupUsers.Count(gu => gu.GroupRole == GroupRole.Admin);
                if (numberOfAdmins == 1)
                    throw new BadRequestException("User is last admin, cannot demote.", "last_admin");
            }

            groupUser.GroupRole = groupRoleNew;
            _context.GroupUsers.Update(groupUser);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error updating the role of a Group User in the database. Group id {group.Id} User id {groupUser.User.Id}: {e.Message}");
                throw new BadRequestException("Error updating the role of a group user.", "update_error");
            }

            return groupRoleNew.ToString();
        }

        private async Task<string> GenerateInviteTokenAsync(int groupId, User user)
        {
            string purpose = GenerateInviteTokenPurpose(groupId);
            return await _inviteTokenProvider.GenerateAsync(purpose, _userManager, user);
        }

        private async Task<bool> ValidateInviteTokenAsync(string token, int groupId, User user)
        {
            string purpose = GenerateInviteTokenPurpose(groupId);
            return await _inviteTokenProvider.ValidateAsync(purpose, token, _userManager, user);
        }

        private string GenerateInviteTokenPurpose(int groupId)
        {
            return string.Format(InviteTokenPurposeFormat, groupId);
        }

        private string GenerateInviteContent(string acceptUrl, GroupDTO group, UserDTO receiver, User sender)
        {//upgrade this to something better looking
            return string.Format("<a href='{0}' style='color:red;'>Click here to accept an invitation to the group '{1}'</a>", acceptUrl, group.Name);
        }
    }
}
