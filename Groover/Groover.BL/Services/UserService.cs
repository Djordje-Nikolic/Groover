using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Groover.DB.MySqlDb;
using Groover.DB.MySqlDb.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Groover.BL.Models;
using Groover.BL.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using Groover.BL.Models.DTOs;
using Groover.BL.Services.Interfaces;
using Groover.BL.Handlers.Requirements;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Groover.BL.Helpers;

namespace Groover.BL.Services
{
    public class UserService : IUserService
    {
        private readonly GrooverDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IImageProcessor _imageProcessor;

        public UserService(GrooverDbContext context,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IConfiguration config,
            IMapper mapper,
            IEmailSender emailSender,
            IImageProcessor imageProcessor,
            ILogger<UserService> logger)
        {
            this._context = context;
            this._signInManager = signInManager;
            this._userManager = userManager;
            this._configuration = config;
            this._logger = logger;
            this._mapper = mapper;
            this._emailSender = emailSender;
            this._imageProcessor = imageProcessor;
        }

        public async Task<ICollection<UserDTO>> GetUsersAsync()
        {
            var users = await _context.Users.ToListAsync();

            _logger.LogInformation("Successfully fetched all the users in the database.");

            var userDTOs = _mapper.Map<ICollection<UserDTO>>(users);
            return userDTOs;
        }

        public async Task<UserDTO> GetUserAsync(int userId)
        {
            if (userId <= 0)
                throw new BadRequestException("Invalid user id. Cannot be 0 or lower.", "bad_id");

            var user = await _context.Users
                .Where(user => user.Id == userId)
                .Include(user => user.UserGroups)
                    .ThenInclude(ug => ug.Group)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new NotFoundException($"User with id {userId} not found.", "not_found");
            }

            _logger.LogInformation($"Successfully found the user with id {userId}.");
            var userDTO = _mapper.Map<UserDTO>(user);
            return userDTO;
        }

        public async Task<UserDTO> GetUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new BadRequestException("Invalid username. Cannot be null or empty.", "bad_username");

            var user = await _context.Users
                .Where(user => user.UserName == username)
                .Include(user => user.UserGroups)
                    .ThenInclude(ug => ug.Group)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new NotFoundException($"User with username {username} not found.", "not_found");
            }

            _logger.LogInformation($"Successfully found the user with username {user.UserName}.");
            var userDTO = _mapper.Map<UserDTO>(user);
            return userDTO;
        }

        public async Task<UserDTO> UpdateUserAsync(UserDTO model)
        {
            if (model == null)
                throw new BadRequestException("User data undefined.", "undefined");

            DTOValidator<UserDTO> validator = new DTOValidator<UserDTO>();
            validator.Validate(model);
            if (validator.IsValid == false)
                ErrorProcessor.Process(validator.ValidationResults, _logger);

            Preprocess(model);
            var user = await _context.Users
                .Where(user => user.Id == model.Id)
                .FirstOrDefaultAsync();
            if (user == null)
                throw new NotFoundException($"User with id {model.Id} not found.", "not_found");

            if (user.UserName != model.Username)
            {
                var duplicateUser = await _userManager.FindByNameAsync(model.Username);
                if (duplicateUser != null)
                    throw new BadRequestException($"Username '{model.Username}' is already taken.", "DuplicateUserName");
            }

            //Update things that can be updated
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!result.Succeeded)
                    ErrorProcessor.Process(result.Errors, _logger);
            }

            user = await _userManager.FindByIdAsync(model.Id.ToString());
            var userNew = _mapper.Map<User>(model);
            user.UserName = userNew.UserName;

            //Process and save image if needed
            var imageBytes = await this._imageProcessor.CheckAsync(model.AvatarImage);
            if (imageBytes != user.AvatarImage)
            {
                if (!string.IsNullOrWhiteSpace(user.AvatarImagePath) &&
                    user.AvatarImagePath != _imageProcessor.GetDefaultUserImage())
                {
                    this._imageProcessor.DeleteImage(user.AvatarImagePath);
                }

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    var imagePath = await this._imageProcessor.SaveImageAsync(imageBytes);
                    user.AvatarImagePath = imagePath;
                }
                else
                {
                    user.AvatarImagePath = this._imageProcessor.GetDefaultGroupImage();
                }
            }

            await _userManager.UpdateAsync(user);

            var userDTO = _mapper.Map<UserDTO>(user);
            return userDTO;
        }

        public async Task<LoggedInDTO> LogInAsync(LogInDTO model, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(model.Username))
                throw new BadRequestException("Invalid LogIn credentials. Username must not be null or empty.", "bad_username");
            if (string.IsNullOrWhiteSpace(model.Password))
                throw new BadRequestException("Invalid LogIn credentials. Password must not be null or empty.", "bad_password");
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new BadRequestException("Couldn't determine ip address.", "ip_not_found");

            var user = await _context.Users
                .Where(user => user.UserName == model.Username)
                .FirstOrDefaultAsync();
            if (user == null)
                throw new NotFoundException($"User with username {model.Username} not found.", "not_found");

            if (user.EmailConfirmed == false)
                throw new UnauthorizedException($"User with username {model.Username} tried to login with an inactivated account.", "email_not_confirmed");

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: true, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    throw new BadRequestException("User is locked out.", "UserLockedOut");
                if (result.IsNotAllowed)
                    throw new BadRequestException("User is not allowed to sign in.", "UserNotAllowed");

                throw new BadRequestException($"Wrong credentials for user {model.Username}.", "Wrong credentials.", "invalid_credentials", user.AccessFailedCount.ToString());
            }

            user = await _context.Users
                .Where(user => user.UserName == model.Username)
                .Include(user => user.UserGroups)
                    .ThenInclude(ug => ug.Group)
                    .ThenInclude(g => g.GroupUsers)
                    .ThenInclude(gu => gu.User)
                .FirstOrDefaultAsync();

            _logger.LogInformation($"User credentials approved. username: {model.Username}");
            string tokenString = await GenerateJwtToken(user);
            RefreshToken refreshToken = GenerateRefreshToken(ipAddress);
            refreshToken.User = user;

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Tokens for user {model.Username} issued successfully.");

            LoggedInDTO loginResp = new LoggedInDTO();
            loginResp.User = _mapper.Map<UserDTO>(user);
            loginResp.Token = tokenString;
            loginResp.RefreshToken = refreshToken.Token;

            return loginResp;
        }

        public async Task<UserDTO> SetAvatar(int userId, IFormFile imageFile)
        {
            if (userId <= 0)
                throw new BadRequestException("Invalid user id.", "bad_id");

            var user = await _context.Users
                .Where(user => user.Id == userId)
                .Include(user => user.UserGroups)
                    .ThenInclude(ug => ug.Group)
                    .ThenInclude(g => g.GroupUsers)
                    .ThenInclude(gu => gu.User)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new NotFoundException($"User with id {userId} not found.", "not_found");
            }

            var imageBytes = await this._imageProcessor.ProcessAsync(imageFile);
            if (imageBytes != user.AvatarImage)
            {
                if (!string.IsNullOrWhiteSpace(user.AvatarImagePath) &&
                    user.AvatarImagePath != _imageProcessor.GetDefaultUserImage())
                {
                    this._imageProcessor.DeleteImage(user.AvatarImagePath);
                }

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    var imagePath = await this._imageProcessor.SaveImageAsync(imageBytes);
                    user.AvatarImagePath = imagePath;
                }
                else
                {
                    user.AvatarImagePath = this._imageProcessor.GetDefaultGroupImage();
                }
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var userDto = this._mapper.Map<UserDTO>(user);
            return userDto;
        }

        public async Task RevokeRefreshTokenAsync(string token, string ipAddress)
        {
            DateTime dtNow = DateTime.UtcNow;

            var refreshToken = _context.RefreshTokens.SingleOrDefault(t => t.Token == token &&
                                                                           t.Revoked == null &&
                                                                           t.Expires > dtNow);

            if (refreshToken == null)
                throw new NotFoundException("Token not found.", "not_found");

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _context.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<LoggedInDTO> RefreshTokenAsync(string token, string ipAddress)
        {
            DateTime dtNow = DateTime.UtcNow;

            var user = await _context.Users
                .Where(u => u.RefreshTokens.Any(t => t.Token == token &&
                                                     t.Revoked == null &&
                                                     t.Expires > dtNow))
                .Include(user => user.RefreshTokens)
                .Include(user => user.UserGroups)
                .SingleOrDefaultAsync();

            if (user == null)
                throw new UnauthorizedException("No user owns this token.", "not_found");

            var refreshToken = user.RefreshTokens.Single(t => t.Token == token &&
                                                              t.Revoked == null &&
                                                              t.Expires > dtNow);

            if (refreshToken == null)
                throw new UnauthorizedException("Token expired or revoked.", "inactive_token");

            // replace old refresh token with a new one and save
            var newRefreshToken = GenerateRefreshToken(ipAddress);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);
            _context.Update(user);
            await _context.SaveChangesAsync();

            LoggedInDTO loginResp = new LoggedInDTO();
            loginResp.User = _mapper.Map<UserDTO>(user);
            loginResp.Token = await GenerateJwtToken(user);
            loginResp.RefreshToken = newRefreshToken.Token;

            return loginResp;
        }

        public async Task<ICollection<RefreshTokenDTO>> GetRefreshTokensAsync(int userId)
        {
            if (userId <= 0)
                throw new BadRequestException("Invalid user id. Cannot be 0 or lower.", "bad_id");

            var user = await _context.Users
                .Where(user => user.Id == userId)
                .Include(user => user.RefreshTokens)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new NotFoundException($"User with id {userId} not found.", "not_found");
            }

            _logger.LogInformation($"Successfully found the user with id {userId}.");

            var tokenDTOs = _mapper.Map<ICollection<RefreshTokenDTO>>(user.RefreshTokens);
            return tokenDTOs;
        }

        public async Task<int> DeleteInactiveRefreshTokensAsync(DateTime? beforeDate)
        {
            DateTime dtNow = DateTime.UtcNow;

            if (beforeDate == null)
                throw new BadRequestException("Before date is missing or invalid.", "bad_datetime");

            var refreshTokens = await _context.RefreshTokens
                .Where(t => (t.Expires <= dtNow || t.Revoked != null) && 
                             t.Created < beforeDate.Value)
                .ToListAsync();

            int count = refreshTokens.Count;

            _context.RefreshTokens.RemoveRange(refreshTokens);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Successfully deleted {count} inactive refresh tokens.");

            return count;
        }

        public async Task<int> RevokeRefreshTokensAsync(DateTime? beforeDate, string ip)
        {
            DateTime dtNow = DateTime.UtcNow;

            if (beforeDate == null)
                throw new BadRequestException("Before date is missing or invalid.", "bad_datetime");

            var refreshTokens = await _context.RefreshTokens
                .Where(t => t.Revoked == null &&
                            t.Expires > dtNow && t.Created < beforeDate.Value)
                .ToListAsync();

            int count = refreshTokens.Count;
            DateTime revokeTime = DateTime.Now;

            refreshTokens.ForEach(token => 
            {
                token.Revoked = revokeTime;
                token.RevokedByIp = ip;
            });

            _context.RefreshTokens.UpdateRange(refreshTokens);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Successfully revoked {count} refresh tokens.");

            return count;
        }

        public async Task<int> RevokeRefreshTokensAsync(int userId, string ip)
        {
            DateTime dtNow = DateTime.UtcNow;

            if (userId <= 0)
                throw new BadRequestException("Invalid user id.", "bad_id");

            var refreshTokens = await _context.RefreshTokens
                .Where(t => t.Revoked == null &&
                            t.Expires > dtNow && t.UserId == userId)
                .ToListAsync();

            int count = refreshTokens.Count;
            DateTime revokeTime = DateTime.Now;

            refreshTokens.ForEach(token =>
            {
                token.Revoked = revokeTime;
                token.RevokedByIp = ip;
            });

            _context.RefreshTokens.UpdateRange(refreshTokens);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Successfully revoked {count} refresh tokens.");

            return count;
        }

        public async Task<RegisteredDTO> RegisterAsync(RegisterDTO model)
        {
            if (model == null)
                throw new BadRequestException("User data undefined.", "undefined");

            DTOValidator<RegisterDTO> validator = new DTOValidator<RegisterDTO>();
            validator.Validate(model);
            if (validator.IsValid == false)
                ErrorProcessor.Process(validator.ValidationResults, _logger);

            Preprocess(model);
            if (!EmailValidator.IsValid(model.Email))
                throw new BadRequestException($"Email invalid: {model.Email}.", "InvalidEmail");

            User user = _mapper.Map<User>(model);
            user.AvatarImagePath = _imageProcessor.GetDefaultUserImage();

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                ErrorProcessor.Process(result.Errors, _logger);
            }

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);            

            _logger.LogInformation($"Registration of user {model.Username} succeeded. Confirmation token generated. ");
            var userDTO = _mapper.Map<UserDTO>(user);
            var dtoResult = new RegisteredDTO();
            dtoResult.User = userDTO;
            dtoResult.ConfirmationToken = confirmationToken;

            return dtoResult;
        }

        public async Task SendConfirmationEmailAsync(string confirmationUrl, UserDTO user)
        {
            var content = GenerateConfirmationEmailContent(confirmationUrl, user.Username, user.Email);
            var message = new EmailMessage(new string[] { user.Email }, "Groover Registration Confirmation Link", content, null);
            await _emailSender.SendEmailAsync(message);
        }

        public async Task ConfirmEmailAsync(ConfirmEmailDTO model)
        {
            DTOValidator<ConfirmEmailDTO> validator = new DTOValidator<ConfirmEmailDTO>();
            validator.Validate(model);
            if (validator.IsValid == false)
                ErrorProcessor.Process(validator.ValidationResults, _logger);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                throw new NotFoundException("Invalid email.", "not_found");

            var result = await _userManager.ConfirmEmailAsync(user, model.Token);
            if (!result.Succeeded)
                throw new BadRequestException("Confirmation failed.", "failed_confirmation");
        }

        private async Task<Claim[]> GetClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            foreach (var userGroup in user.UserGroups)
            {
                if (userGroup.GroupRole == GroupRole.Member)
                {
                    claims.Add(new Claim(GroupClaimTypeConstants.GetConstant(GroupRole.Member), userGroup.GroupId.ToString()));
                    continue;
                }

                if (userGroup.GroupRole == GroupRole.Admin)
                {
                    claims.Add(new Claim(GroupClaimTypeConstants.GetConstant(GroupRole.Member), userGroup.GroupId.ToString()));
                    claims.Add(new Claim(GroupClaimTypeConstants.GetConstant(GroupRole.Admin), userGroup.GroupId.ToString()));
                    continue;
                }
            }

            return claims.ToArray();
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = await GetClaims(user);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:SecretKey").Value)),
                                                     SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Jwt:Issuer").Value,
                audience: _configuration.GetSection("Jwt:Audience").Value,
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials);

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            return jwtTokenHandler.WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }

        private string GenerateConfirmationEmailContent(string confirmationUrl, string username, string email)
        {//upgrade this to something better looking
            return string.Format("<h2 style='color:red;'>{0}</h2>", confirmationUrl);
        }

        private void Preprocess(RegisterDTO model)
        {
            model.Email = model.Email.Trim();
            model.Username = model.Username.Trim();
            model.Password = model.Password.Trim();
        }

        private void Preprocess(UserDTO model)
        {
            model.Email = model.Email?.Trim();
            model.Username = model.Username.Trim();
        }
    }
}
