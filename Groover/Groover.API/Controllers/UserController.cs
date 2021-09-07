using AutoMapper;
using Groover.API.Models.Requests;
using Groover.API.Models.Responses;
using Groover.API.Services.Interfaces;
using Groover.BL.Handlers.Requirements;
using Groover.BL.Models.DTOs;
using Groover.BL.Models.Exceptions;
using Groover.BL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Groover.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _autoMapper;
        private readonly ILogger<UserController> _logger;
        private readonly INotificationService _notificationService;

        public UserController(IUserService userService,
            INotificationService notificationService,
            IMapper autoMapper, 
            ILogger<UserController> logger)
        {
            _notificationService = notificationService;
            _userService = userService;
            _autoMapper = autoMapper;
            _logger = logger;
        }

        [HttpGet("getById")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation($"Attempting to retrieve the user by id: {id}");

            if (id != GetUserId())
                return Unauthorized();

            var userDTO = await _userService.GetUserAsync(id);
            var response = _autoMapper.Map<UserResponse>(userDTO);

            _logger.LogInformation($"Successfully retrieved the user by id: {id}");

            return Ok(response);
        }

        [HttpGet("getByUsername")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            _logger.LogInformation($"Attempting to retrieve the user: {username}");

            var userDTO = await _userService.GetUserAsync(username);
            var response = _autoMapper.Map<UserLiteResponse>(userDTO);

            _logger.LogInformation($"Successfully retrieved the user: {username}");
            return Ok(response);
        }

        [HttpGet("getAvatar")]
        public async Task<IActionResult> GetAvatar(int userId)
        {
            _logger.LogInformation($"Attempting to retrieve the avatar for the user: {userId}");

            var userDTO = await _userService.GetUserAsync(userId);

            _logger.LogInformation($"Successfully retrieved the avatar for the user: {userId}");
            return File(userDTO.AvatarImage, "image/*");
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateUserRequest updateUserRequest)
        {
            _logger.LogInformation($"Attempting to update user with id {updateUserRequest.Id}");

            var updateModel = this._autoMapper.Map<UserDTO>(updateUserRequest);
            var updatedUser = await _userService.UpdateUserAsync(updateModel);
            _logger.LogInformation($"Update succeeded for user with id {updateUserRequest.Id}.");

            var response = _autoMapper.Map<UserResponse>(updatedUser);
            var notificationData = _autoMapper.Map<UserDataResponse>(response);

            //Send notifications
            var groups = GetGroupList();
            await _notificationService.UserUpdatedAsync(notificationData, groups);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LogIn([FromBody] LogInRequest logInRequest)
        {
            _logger.LogInformation($"Login attempt for user: {logInRequest.Username}");

            var loginModel = this._autoMapper.Map<LogInDTO>(logInRequest);
            var logInDTO = await _userService.LogInAsync(loginModel, IpAddress());
            _logger.LogInformation($"Login succeeded for user {logInRequest.Username}.");

            SetRefreshTokenCookie(logInDTO.RefreshToken);
            var response = _autoMapper.Map<LogInResponse>(logInDTO);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            _logger.LogInformation($"Registration attempt for user: {registerRequest.Username}");

            var registerModel = this._autoMapper.Map<RegisterDTO>(registerRequest);
            var dtoRes = await _userService.RegisterAsync(registerModel);

            var confirmationLink = GenerateConfirmationUrl(dtoRes.ConfirmationToken, dtoRes.User.Email);
            await _userService.SendConfirmationEmailAsync(confirmationLink, dtoRes.User);

            var response = _autoMapper.Map<UserResponse>(dtoRes.User);
            _logger.LogInformation($"Registration succeeded for user {response.Username}. Awaiting confirmation. ");

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            ConfirmEmailRequest confirmEmailRequest = new ConfirmEmailRequest()
            {
                Token = token,
                Email = email
            };

            _logger.LogInformation($"Email confirmation attempt for email: {confirmEmailRequest.Email}");

            var confirmModel = this._autoMapper.Map<ConfirmEmailDTO>(confirmEmailRequest);
            await _userService.ConfirmEmailAsync(confirmModel);

            return Ok(new { message = "Confirmation successful. " });
        }

        [HttpPatch("setAvatar")]
        public async Task<IActionResult> SetAvatar(IFormFile imageFile)
        {
            int? userId = GetUserId();
            if (userId == null)
                throw new UnauthorizedException("User id not set in claims");

            _logger.LogInformation($"Attempting to set an avatar for user: {userId.Value}");

            var updatedUser = await _userService.SetAvatar(userId.Value, imageFile);
            _logger.LogInformation($"Successfully set an avator for user: {userId.Value}");

            var response = this._autoMapper.Map<UserResponse>(updatedUser);
            var notificationData = _autoMapper.Map<UserDataResponse>(response);

            //Send notifications
            var groups = GetGroupList();
            await _notificationService.UserUpdatedAsync(notificationData, groups);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            _logger.LogInformation($"Attempting to refresh the JWT token using the refresh token: {refreshToken}.");

            var response = await _userService.RefreshTokenAsync(refreshToken, IpAddress());

            SetRefreshTokenCookie(response.RefreshToken);

            _logger.LogInformation($"Successfully refreshed the JWT token. New refresh token: {response.RefreshToken}");

            return Ok(new { token = response.Token });
        }

        //Only global admin?
        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] string token)
        {
            // accept token from request body or cookie
            var refreshToken = token ?? Request.Cookies["refreshToken"];

            _logger.LogInformation($"Attempting to revoke a refresh token: {refreshToken}.");

            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Token is required" });

            await _userService.RevokeRefreshTokenAsync(refreshToken, IpAddress());

            _logger.LogInformation($"Successfully revoked the refresh token.");

            return Ok(new { message = "Token revoked" });
        }

        #region Global Admin Endpoints

        [Authorize(Roles = "Admin")]
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Attempting to fetch all the users from the database.");

            var userDTOs = await _userService.GetUsersAsync();
            var response = _autoMapper.Map<ICollection<UserResponse>>(userDTOs);

            _logger.LogInformation("Successfully fetched all the users from the database.");

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/refreshTokens")]
        public async Task<IActionResult> GetRefreshTokens(int id)
        {
            _logger.LogInformation($"Attempting to fetch all the refresh tokens for the user by id: {id}");

            var refreshTokenDTOs = await _userService.GetRefreshTokensAsync(id);
            var response = _autoMapper.Map<ICollection<RefreshTokenResponse>>(refreshTokenDTOs);

            _logger.LogInformation($"Successfully fetched all the refresh tokens for the user by id: {id}");

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("deleteInactiveRefreshTokens")]
        public async Task<IActionResult> DeleteInactiveRefreshTokens([FromBody] DateTime? beforeDate)
        {
            _logger.LogInformation($"Attempting to delete all revoked refresh tokens before time: {beforeDate?.ToString()}");

            var count = await _userService.DeleteInactiveRefreshTokensAsync(beforeDate);

            _logger.LogInformation($"Successfully deleted all revoked refresh tokens before time: {beforeDate?.ToString()}. Count: {count}");

            return Ok(new { deletedCount = count });
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("revokeAllTokens")]
        public async Task<IActionResult> RevokeAllTokens([FromBody] DateTime? beforeDate)
        {
            _logger.LogInformation($"Attempting to revoke all refresh tokens before time: {beforeDate?.ToString()}");

            var count = await _userService.RevokeRefreshTokensAsync(beforeDate, IpAddress());

            _logger.LogInformation($"Successfully revoked all refresh tokens before time: {beforeDate?.ToString()}. Count: {count}");

            return Ok(new { revokedCount = count });
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("revokeUserTokens")]
        public async Task<IActionResult> RevokeUserTokens(int userId)
        {
            _logger.LogInformation($"Attempting to revoke all refresh tokens for user: {userId}");

            var count = await _userService.RevokeRefreshTokensAsync(userId, IpAddress());

            _logger.LogInformation($"Successfully revoked all refresh tokens for user: {userId}. Count: {count}");

            return Ok(new { revokedCount = count });
        }
        #endregion

        private void SetRefreshTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string IpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

        private string GenerateConfirmationUrl(string token, string email)
        {
            //Test this
            return Url.Action(nameof(ConfirmEmail), "user", new { token, email} , Request.Scheme);
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim == null ? null : int.Parse(claim.Value);
        }

        private List<string> GetGroupList()
        {
            return User.Claims
                .Where(claim => claim.Type == GroupClaimTypeConstants.Member)
                .Select(claim => claim.Value)
                .ToList();
        }
    }
}
