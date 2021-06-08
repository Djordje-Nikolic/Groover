using Groover.BL.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groover.BL.Services.Interfaces
{
    public interface IGroupService
    {
        Task<GroupDTO> GetGroupAsync(int groupId);
        Task<ICollection<GroupDTO>> GetGroupsAsync(ICollection<int> groupIds);
        Task RemoveUserAsync(int groupId, int userId);
        Task UpdateUserRoleAsync(int groupId, int userId, string newRole);
        Task<InvitationDTO> InviteUserAsync(int groupId, int userId);
        Task AcceptInviteAsync(string token, int groupId, int userId);
        Task<GroupDTO> UpdateGroupAsync(GroupDTO groupDTO);
        Task DeleteAsync(int id);
        Task<GroupDTO> CreateGroupAsync(GroupDTO groupDTO, int userId);
        Task SendInvitationEmailAsync(string acceptUrl, GroupDTO group, UserDTO receiver, int senderId);
    }
}