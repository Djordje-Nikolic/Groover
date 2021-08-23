using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Services.Interfaces
{
    public interface INotificationService
    {
        public Task UserLeftGroupAsync(int groupId, int userId);
        public Task UserJoinedGroupAsync(int groupId, int userId);
        public Task UserInvitedAsync(int groupId, int userId);
        public Task UserRoleUpdatedAsync(int groupId, int userId);
        public Task UserUpdatedAsync(int userId, List<int> groupIds);
        public Task GroupUpdatedAsync(int groupId);
        public Task GroupDeletedAsync(int groupId);
    }
}
