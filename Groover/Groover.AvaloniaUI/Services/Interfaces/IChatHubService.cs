using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services.Interfaces
{
    public interface IChatHubService
    {
        HashSet<int> ConnectedGroups { get; }
        HubConnection Connection { get; }
        ConnectionHandlersWrapper HandlersWrapper { get; }
        Task<HubConnection> InitializeConnection();
        Task StartConnection();
        Task ConnectToGroup(int groupId);
        Task DisconnectFromGroup(int groupId);
        Task NotifyConnection(int groupId, int userToNotifyId, int retryOnUnauthorized = 1);
        Task<BaseResponse> SendTextMessage(TextMessageRequest messageRequest, int retryOnUnauthorized = 1);
        Task<BaseResponse> SendImageMessage(ImageMessageRequest messageRequest, int retryOnUnauthorized = 1);
        Task Reset();
    }
}
