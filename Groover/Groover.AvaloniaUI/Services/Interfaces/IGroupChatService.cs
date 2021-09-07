using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services.Interfaces
{
    public interface IGroupChatService
    {
        HashSet<int> ConnectedGroups { get; }
        HubConnection Connection { get; }
        Task<HubConnection> InitializeConnection();
        Task StartConnection();
        Task JoinGroup(int groupId);
        Task LeaveGroup(int groupId);
        Task NotifyConnection(int groupId, int userToNotifyId, int retryOnUnauthorized = 1);
        Task Reset();
    }
}
