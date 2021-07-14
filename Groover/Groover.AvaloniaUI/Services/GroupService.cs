using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services
{
    public class GroupService : GrooverService, IGroupService
    {
        private readonly Controller _controller;

        public GroupService(IApiService apiService) : base(apiService)
        {
            _controller = Controller.Group;
        }

        public async Task<BaseResponse> DeleteGroupAsync(int groupId)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("id", groupId.ToString());
            return await this.SendRequestAsync<BaseResponse>(queryParams, HttpMethod.Delete, _controller, "delete");
        }

        public async Task<byte[]> GetImageAsync(int groupId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse> InviteUserAsync(int groupId, int userId)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("groupId", groupId.ToString());
            queryParams.Add("userId", userId.ToString());
            return await this.SendRequestAsync<BaseResponse>(queryParams, HttpMethod.Patch, _controller, "inviteUser");
        }

        public async Task<BaseResponse> RemoveUserAsync(int groupId, int userId)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("groupId", groupId.ToString());
            queryParams.Add("userId", userId.ToString());
            return await this.SendRequestAsync<BaseResponse>(queryParams, HttpMethod.Patch, _controller, "removeUser");
        }

        public async Task<BaseResponse> SetImageAsync(/* some image and group id*/)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse> UpdateUserRoleAsync(int groupId, int userId, GrooverGroupRole newRole)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("groupId", groupId.ToString());
            queryParams.Add("userId", userId.ToString());
            queryParams.Add("newRole", newRole.ToString());
            return await this.SendRequestAsync<BaseResponse>(queryParams, HttpMethod.Patch, _controller, "updateRole");
        }


    }
}
