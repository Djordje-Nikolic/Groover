using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Services.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<UserResponse> GetByUsernameAsync(string username);
        Task<UserResponse> GetByIdAsync(int id);
        Task<byte[]> GetAvatarAsync(int userId);
        Task<BaseResponse> SetAvatarAsync(/* Some image*/);
        Task<UserResponse> UpdateUserAsync(UserRequest request);
        void Logout();
    }
}
