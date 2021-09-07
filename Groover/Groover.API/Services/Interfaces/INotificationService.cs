using Groover.API.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.API.Services.Interfaces
{
    public interface INotificationService
    {
        Task ForceTokenRefreshAsync(string userId);
        Task GroupDeletedAsync(string groupId);
        Task GroupUpdatedAsync(GroupDataResponse group);
        Task GroupCreatedAsync(UserGroupResponse newGroup, string userId);
        Task UserInvitedAsync(string token, GroupLiteResponse group, string userId);
        Task UserJoinedGroupAsync(string groupId, GroupUserLiteResponse groupUserData, UserGroupResponse userGroupData);
        Task UserLeftGroupAsync(string groupId, string userId);
        Task UserRoleUpdatedAsync(string groupId, string userId, string newRole);
        Task UserUpdatedAsync(UserDataResponse user, List<string> groupIds);
    }
}
