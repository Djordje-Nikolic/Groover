using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services.Interfaces
{
    public interface IGroupService
    {
        Task<BaseResponse> DeleteGroupAsync(int groupId);
        Task<BaseResponse> InviteUserAsync(int groupId, int userId);
        Task<BaseResponse> AcceptInviteAsync(string token, int groupId, int userId);
        Task<BaseResponse> RemoveUserAsync(int groupId, int userId);
        Task<BaseResponse> UpdateUserRoleAsync(int groupId, int userId, GrooverGroupRole newRole);
        Task<GroupResponse> UpdateGroupAsync(GroupRequest groupRequest);
        Task<GroupResponse> CreateGroupAsync(GroupRequest groupRequest);
        Task<byte[]> GetImageAsync(int groupId);
        Task<BaseResponse> SetImageAsync(/* Some image and group id*/);
    }
}
