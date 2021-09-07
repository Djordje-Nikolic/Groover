using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;

namespace Groover.AvaloniaUI.Services
{
    public class UserService : GrooverService, IUserService
    {
        private readonly Controller _controller;

        public UserService(IApiService apiService) : base(apiService)
        {
            _controller = Controller.User;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var response = await this.SendRequestAsync<LoginRequest, LoginResponse>(request, HttpMethod.Post, _controller, "login");
            if (response.IsSuccessful)
            {
                _apiService.SetAccessToken(response.Token);
            }
            return response;
        }

        public void Logout()
        {
            this._apiService.RemoveAccessToken();
            this._apiService.CleanRefreshTokens();
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            return await this.SendRequestAsync<RegisterRequest, RegisterResponse>(request, HttpMethod.Post, _controller, "register", 0);
        }

        public async Task<UserResponse> GetByUsernameAsync(string username)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("username", username);
            return await this.SendRequestAsync<UserResponse>(queryParams, HttpMethod.Get, _controller, "getByUsername");
        }

        public async Task<UserResponse> GetByIdAsync(int id)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add("id", id.ToString());
            return await this.SendRequestAsync<UserResponse>(queryParams, HttpMethod.Get, _controller, "getById");
        }

        public async Task<byte[]> GetAvatarAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse> SetAvatarAsync(/* Some image*/)
        {
            throw new NotImplementedException();
        }

        public async Task<UserResponse> UpdateUserAsync(UserRequest request)
        {
            return await this.SendRequestAsync<UserRequest, UserResponse>(request, HttpMethod.Put, _controller, "update");
        }
    }
}
