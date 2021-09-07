using Groover.BL.Models.DTOs;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groover.BL.Services.Interfaces
{
    public interface IGroupService
    {
        Task<GroupDTO> GetGroupAsync(int groupId);
        Task<ICollection<GroupDTO>> GetGroupsAsync(ICollection<int> groupIds);
        Task<bool> RemoveUserAsync(int groupId, int userId);
        Task<string> UpdateUserRoleAsync(int groupId, int userId, string newRole);
        Task<InvitationDTO> InviteUserAsync(int groupId, int userId);
        Task<GroupUserDTO> AcceptInviteAsync(string token, int groupId, int userId);
        Task<GroupDTO> UpdateGroupAsync(GroupDTO groupDTO);
        Task DeleteAsync(int id);
        Task<GroupDTO> CreateGroupAsync(GroupDTO groupDTO, int userId);
        Task SendInvitationEmailAsync(string acceptUrl, GroupDTO group, UserDTO receiver, int senderId);
        Task<GroupDTO> SetImage(int groupId, IFormFile imageFile);
    }
}