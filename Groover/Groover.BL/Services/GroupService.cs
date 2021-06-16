using AutoMapper;
using Groover.BL.Helpers;
using Groover.BL.Models;
using Groover.BL.Models.DTOs;
using Groover.BL.Models.Exceptions;
using Groover.BL.Services.Interfaces;
using Groover.BL.Utils;
using Groover.DB.MySqlDb;
using Groover.DB.MySqlDb.Entities;
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

        public GroupService(GrooverDbContext context,
            IMapper mapper,
            ITokenProviderAccessor<User> tokenProviderAccessor,
            ILogger<GroupService> logger,
            IEmailSender emailSender,
            UserManager<User> userManager)
        {
            this._tokenProviderAccessor = tokenProviderAccessor;
            this._context = context;
            this._logger = logger;
            this._mapper = mapper;
            this._userManager = userManager;
            this._emailSender = emailSender;
            this._inviteTokenProvider = _tokenProviderAccessor.GetTokenProvider(Constants.GroupInviteTokenProvider);
        }

        public async Task<GroupDTO> CreateGroupAsync(GroupDTO groupDTO, int userId)
        {
            if (groupDTO == null)
                throw new BadRequestException("Group data undefined.","undefined");
            if (userId <= 0)
                throw new BadRequestException("Invalid user id.", "bad_id");

            DTOValidator<GroupDTO> validator = new DTOValidator<GroupDTO>();
            validator.Validate(groupDTO);
            if (validator.IsValid == false)
                ErrorProcessor.Process(validator.ValidationResults, _logger);

            //Preprocess
            User user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new NotFoundException($"No user by id {userId}.", "not_found");

            if (await _context.Groups.AnyAsync(g => g.Name == groupDTO.Name) == true)
                throw new BadRequestException("Group with that name already exists.", "duplicate_name");

            Group group = _mapper.Map<Group>(groupDTO);
            GroupUser groupUser = new GroupUser();
            groupUser.Group = group;
            groupUser.User = user;
            groupUser.GroupRole = GroupRole.Admin;

            _context.Groups.Add(group);
            _context.GroupUsers.Add(groupUser);
            await _context.SaveChangesAsync();

            group = await _context.Groups.Where(g => g.Id == group.Id)
                .Include(g => g.GroupUsers)
                .ThenInclude(gu => gu.User)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var createdDTO = _mapper.Map<GroupDTO>(group);
            return createdDTO;
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new BadRequestException("Invalid group id.", "bad_id.");

            var group = await _context.Groups.FindAsync(id);
            if (group == null)
                throw new NotFoundException($"No group by id {id}.", "not_found");

            var groupUsers = await _context.GroupUsers.Where(gu => gu.GroupId == id).ToListAsync();
            _context.GroupUsers.RemoveRange(groupUsers);
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
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
                throw new NotFoundException($"No group by id {groupId}.", "not_found");

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

        public async Task AcceptInviteAsync(string token, int groupId, int userId)
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
        }

        public async Task RemoveUserAsync(int groupId, int userId)
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

            if (groupUsers.Count == 2)
            {
                groupUsers.Remove(userToBeRemoved);

                var lastUser = groupUsers.Last();
                lastUser.GroupRole = GroupRole.Admin;
                _context.GroupUsers.Update(lastUser);
            }
            else if (groupUsers.Count == 1)
            {
                _context.Groups.Remove(group);
            }

            _context.GroupUsers.Remove(userToBeRemoved);
            await _context.SaveChangesAsync();
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

            if (await _context.Groups.AnyAsync(g => g.Name == groupDTO.Name && 
                                                    g.Id != groupDTO.Id) == true)
                throw new BadRequestException("Group with that name already exists.", "duplicate_name");

            Group groupNew = _mapper.Map<Group>(groupDTO);
            group.Name = groupNew.Name;
            group.Description = groupNew.Description;

            _context.Groups.Update(group);
            await _context.SaveChangesAsync();

            group = await _context.Groups.Where(g => g.Id == group.Id)
                .Include(g => g.GroupUsers)
                .ThenInclude(gu => gu.User)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var updatedDTO = _mapper.Map<GroupDTO>(group);
            return updatedDTO;
        }

        public async Task UpdateUserRoleAsync(int groupId, int userId, string newRole)
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

            GroupUser groupUser = await _context.GroupUsers.FindAsync(groupId, userId);
            if (groupUser == null)
                throw new NotFoundException("User is not a member of the group.", "not_found");

            groupUser.GroupRole = groupRoleNew;
            _context.GroupUsers.Update(groupUser);
            await _context.SaveChangesAsync();
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
            return string.Format("<h2 style='color:red;'>{0}</h2>", acceptUrl);
        }
    }
}
