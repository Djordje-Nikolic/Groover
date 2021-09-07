﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Groover.AvaloniaUI.Services.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace Groover.AvaloniaUI.Services
{
    public class GroupChatService : IGroupChatService, IDisposable, IAsyncDisposable
    {
        public HashSet<int> ConnectedGroups { get; private set; }
        public HubConnection Connection { get; private set; }

        private IApiService _apiService;
        private bool disposedValue;

        public GroupChatService(IApiService apiService)
        {
            _apiService = apiService;
            ConnectedGroups = new HashSet<int>();
        }

        public async Task<HubConnection> InitializeConnection()
        {
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

        private async Task Connection_Reconnected(string arg)
        {
            foreach (var groupId in ConnectedGroups)
            {
                await Connection.InvokeAsync("OpenGroupConnection", groupId.ToString());
            }
        }

        public async Task StartConnection() => await Connection.StartAsync();

        public async Task JoinGroup(int groupId)
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

        public async Task LeaveGroup(int groupId)
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

        public async Task NotifyConnection(int groupId, int userToNotifyId, int retryOnUnauthorized = 1)
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
        
        public async Task Reset()
        {
            ConnectedGroups.Clear();
            await Connection.StopAsync();
            await Connection.DisposeAsync();
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