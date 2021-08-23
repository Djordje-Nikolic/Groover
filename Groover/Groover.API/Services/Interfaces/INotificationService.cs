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
        Task UserInvitedAsync(string groupId, string userId);
        Task UserJoinedGroupAsync(string groupId, UserDataResponse user);
        Task UserLeftGroupAsync(string groupId, string userId);
        Task UserRoleUpdatedAsync(string groupId, string userId, string newRole);
        Task UserUpdatedAsync(UserDataResponse user, List<string> groupIds);
    }
}
