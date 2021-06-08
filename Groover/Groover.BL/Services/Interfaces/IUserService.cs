using Groover.BL.Models.DTOs;
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
        Task SendConfirmationEmailAsync(string confirmationUrl, UserDTO user);
        Task RevokeToken(string token, string ipAddress);
        Task<LoggedInDTO> RefreshToken(string token, string ipAddress);
        Task<int> DeleteInactiveRefreshTokensAsync(DateTime? beforeDate);
    }
}