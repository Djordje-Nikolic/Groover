using Groover.AvaloniaUI.Models;
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
        public Task<BaseResponse> DeleteGroupAsync(int groupId);
        public Task<BaseResponse> InviteUserAsync(int groupId, int userId);
        public Task<BaseResponse> RemoveUserAsync(int groupId, int userId);
        public Task<BaseResponse> UpdateUserRoleAsync(int groupId, int userId, GrooverGroupRole newRole);
    }
}
