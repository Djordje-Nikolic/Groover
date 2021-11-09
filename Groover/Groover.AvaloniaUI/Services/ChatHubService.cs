using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using Groover.AvaloniaUI.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace Groover.AvaloniaUI.Services
{
    public class ChatHubService : IChatHubService, IDisposable, IAsyncDisposable
    {
        public const int DefaultRetryOnUnauthorizedAttempts = 1;
        public HashSet<int> ConnectedGroups { get; private set; }

        private HubConnection _connection;
        public HubConnection Connection 
        {
            get 
            {
                return _connection;
            }
            private set
            {
                _connection = value;
                HandlersWrapper.SetConnection(value);
            }
        }
        public ConnectionHandlersWrapper HandlersWrapper { get; }

        private IApiService _apiService;
        private bool disposedValue;

        public ChatHubService(IApiService apiService)
        {
            _apiService = apiService;
            ConnectedGroups = new HashSet<int>();
            HandlersWrapper = new ConnectionHandlersWrapper();
        }

        public async Task<HubConnection> InitializeConnection()
        {
            if (Connection != null)
                await Reset();

            Connection = new HubConnectionBuilder()
                .WithAutomaticReconnect()
                .WithUrl(_apiService.ApiConfig.GroupChatHubAddress, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_apiService.GetAccessToken());
                })
                .Build();

            Connection.On<string>("ForceTokenRefresh",  
                async (userId) => await this._apiService.RefreshTokenAsync());

            Connection.Reconnected += Connection_Reconnected;

            return Connection;
        }

        public async Task StartConnection() => await Connection.StartAsync();

        public async Task ConnectToGroup(int groupId)
        {
            ConnectedGroups.Add(groupId);

            try
            {
                await Connection.InvokeAsync("OpenGroupConnection", groupId.ToString());
            }
            catch (Exception e)
            {
                //Modify this 
                if (e.Message.Contains("Unauthorized"))
                {
                    await ReconnectOnTokenFail();
                }
                else
                    throw;
            }
        }
        public async Task DisconnectFromGroup(int groupId)
        {
            if (ConnectedGroups.Remove(groupId) == true)
            {
                try
                {
                    await Connection.InvokeAsync("CloseGroupConnection", groupId.ToString());
                }
                catch (Exception e)
                {
                    //Modify this 
                    if (e.Message.Contains("Unauthorized"))
                    {
                        await ReconnectOnTokenFail();
                    }
                    else
                        throw;
                }
            }
        }

        public async Task NotifyConnection(int groupId, int userToNotifyId, int retryOnUnauthorized = DefaultRetryOnUnauthorizedAttempts)
        {
            try
            {
                await Connection.InvokeAsync("NotifyConnection", groupId.ToString(), userToNotifyId.ToString());
            }
            catch (Exception e)
            {
                //Modify this 
                if (e.Message.Contains("Unauthorized"))
                {
                    await ReconnectOnTokenFail();
                    await NotifyConnection(groupId, userToNotifyId, retryOnUnauthorized - 1);
                }
                else
                    throw;
            }
        }
        
        public async Task<BaseResponse> SendTextMessage(TextMessageRequest textMessageRequest, int retryOnUnauthorized = DefaultRetryOnUnauthorizedAttempts)
        {
            try
            {
                await Connection.InvokeAsync("SendTextMessage", textMessageRequest);

                return new BaseResponse()
                {
                    IsSuccessful = true,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception e)
            {
                //Modify this 
                if (e.Message.Contains("Unauthorized"))
                {
                    await ReconnectOnTokenFail();
                    return await SendTextMessage(textMessageRequest, retryOnUnauthorized - 1);
                }
                else
                {
                    var errors = e.Message.Split(':').Select(error => error.Trim()).ToArray();
                    return new BaseResponse()
                    {
                        IsSuccessful = false,
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ErrorResponse = new ErrorResponse()
                        {
                            Error = errors[1],
                            ErrorCode = errors[0]
                        }
                    };
                }
            }
        }

        public async Task<BaseResponse> SendImageMessage(ImageMessageRequest imageMessageRequest, int retryOnUnauthorized = DefaultRetryOnUnauthorizedAttempts)
        {
            try
            {
                await Connection.InvokeAsync("SendImageMessage", imageMessageRequest);

                return new BaseResponse()
                {
                    IsSuccessful = true,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception e)
            {
                //Modify this 
                if (e.Message.Contains("Unauthorized"))
                {
                    await ReconnectOnTokenFail();
                    return await SendImageMessage(imageMessageRequest, retryOnUnauthorized - 1);
                }
                else
                {
                    var errors = e.Message.Split(':').Select(error => error.Trim()).ToArray();
                    return new BaseResponse()
                    {
                        IsSuccessful = false,
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ErrorResponse = new ErrorResponse()
                        {
                            Error = errors[1],
                            ErrorCode = errors[0]
                        }
                    };
                }
            }
        }

        public async Task Reset()
        {
            if (Connection != null)
            {
                if (Connection.State == HubConnectionState.Connected)
                {
                    foreach (var groupId in ConnectedGroups)
                    {
                        await DisconnectFromGroup(groupId);
                    }

                    await Connection.StopAsync();
                }

                HandlersWrapper.CleanUpHandlers();

                await Connection.DisposeAsync();
            }

            ConnectedGroups.Clear();
            Connection = null;
        }

        private async Task Connection_Reconnected(string arg)
        {
            foreach (var groupId in ConnectedGroups)
            {
                await Connection.InvokeAsync("OpenGroupConnection", groupId.ToString());
            }
        }
        private async Task ReconnectOnTokenFail()
        {
            await Connection.StopAsync();
            await _apiService.RefreshTokenAsync();
            await Connection.StartAsync();

            foreach (var groupId in ConnectedGroups)
            {
                await Connection.InvokeAsync("OpenGroupConnection", groupId.ToString());
            }
        }

        //Add wrappers for all calls to hub and if they fail with unathorized, attempt to refresh token

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    ConnectedGroups.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~GroupChatService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await Reset();
            Dispose(disposing: true);
        }
        #endregion Dispose

    }
}
