using Groover.BL.Hubs;
using Groover.BL.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<GroupChatHub> _hubContext;

        public NotificationService(IHubContext<GroupChatHub> hubContext)
        {
            this._hubContext = hubContext;
        }

        public async Task UserLeftGroupAsync(int groupId, int userId)
        {

        }

        public async Task UserJoinedGroupAsync(int groupId, int userId)
        {

        }

        public async Task UserInvitedAsync(int groupId, int userId)
        {

        }

        public async Task UserRoleUpdatedAsync(int groupId, int userId)
        {

        }

        public async Task UserUpdatedAsync(int userId, List<int> groupIds)
        {

        }

        public async Task GroupUpdatedAsync(int groupId)
        {

        }

        public async Task GroupDeletedAsync(int groupId)
        {

        }
    }
}
