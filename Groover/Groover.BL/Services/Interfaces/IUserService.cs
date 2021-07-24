using Groover.BL.Models.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groover.BL.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO> GetUserAsync(int userId);
        Task<UserDTO> GetUserAsync(string username);
        Task<ICollection<UserDTO>> GetUsersAsync();
        Task<ICollection<RefreshTokenDTO>> GetRefreshTokensAsync(int userId);
        Task<LoggedInDTO> LogInAsync(LogInDTO model, string ipAddress);
        Task ConfirmEmailAsync(ConfirmEmailDTO model);
        Task<RegisteredDTO> RegisterAsync(RegisterDTO model);
        Task<UserDTO> UpdateUserAsync(UserDTO model);
        Task SendConfirmationEmailAsync(string confirmationUrl, UserDTO user);
        Task RevokeRefreshTokenAsync(string token, string ipAddress);
        Task<int> RevokeRefreshTokensAsync(DateTime? beforeDate, string ipAddress);
        Task<int> RevokeRefreshTokensAsync(int userId, string ipAddress);
        Task<LoggedInDTO> RefreshTokenAsync(string token, string ipAddress);
        Task<int> DeleteInactiveRefreshTokensAsync(DateTime? beforeDate);
        Task<UserDTO> SetAvatar(int userId, IFormFile imageFile);
    }
}