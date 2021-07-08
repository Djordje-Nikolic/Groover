using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.ViewModels
{
    public class LoginViewModel : ReactiveValidationObject
    {
        private IUserService _userService;
        private GrooverConstants _userParameters;

        [ObservableAsProperty]
        public bool LoggedInSuccessfully { get; }

        [Reactive]
        public string Username { get; set; } = string.Empty;
        [Reactive]
        public string Password { get; set; } = string.Empty;

        [ObservableAsProperty]
        public LoginResponse Response { get; }

        [ObservableAsProperty]
        public List<string> ErrorMessages { get;  }

        [ObservableAsProperty]
        public string SuccessMessage { get; }

        public ReactiveCommand<Unit, LoginResponse> Login { get; }

        private ReactiveCommand<LoginResponse, List<string>> GenerateErrorMessages { get; }
        private ReactiveCommand<LoginResponse, string> GenerateSuccessMessage { get; }

        public LoginViewModel(IUserService userService, GrooverConstants userParameters)
        {
            _userService = userService;
            _userParameters = userParameters;

            this.ValidationRule(x => x.Username, name => !string.IsNullOrWhiteSpace(name), "Username cannot be empty.");
            this.ValidationRule(x => x.Password, pass => !string.IsNullOrWhiteSpace(pass), "Password cannot be empty.");
            this.ValidationRule(x => x.LoggedInSuccessfully, loginSuccess => loginSuccess == false, "Already logged in!");

            Login = ReactiveCommand.CreateFromTask(LoginAsync, canExecute: this.IsValid());
            Login.ToPropertyEx(this, x => x.Response);
            this.WhenAnyValue(v => v.Response)
                .Where(r => r != null)
                .Select(r => r.IsSuccessful)
                .ToPropertyEx(this, x => x.LoggedInSuccessfully);

            GenerateErrorMessages = ReactiveCommand.Create<LoginResponse, List<string>>(ProcessErrors);
            GenerateErrorMessages.ToPropertyEx(this, x => x.ErrorMessages);

            GenerateSuccessMessage = ReactiveCommand.Create<LoginResponse, string>(ProcessSuccess);
            GenerateSuccessMessage.ToPropertyEx(this, x => x.SuccessMessage);
        }

        private async Task<LoginResponse> LoginAsync()
        {
            //await Task.Delay(2000);

            //var response = new LoginResponse();

            //await GenerateErrorMessages.Execute(response);

            //await GenerateSuccessMessage.Execute(response);

            //return response;

            LoginRequest request = new LoginRequest()
            {
                Username = this.Username,
                Password = this.Password
            };

            var response = await _userService.LoginAsync(request);

            //process response and display errors if any/display success message
            await GenerateErrorMessages.Execute(response);

            await GenerateSuccessMessage.Execute(response);

            return response;
        }

        private List<string> ProcessErrors(LoginResponse response)
        {
            List<string> messages = new List<string>();

            if (response.ErrorCodes == null)
                return messages;

            foreach (var code in response.ErrorCodes.Distinct())
            {
                string matchMessage;
                switch (code)
                {
                    case "bad_username":
                        matchMessage = "Invalid username format.";
                        break;
                    case "bad_password":
                        matchMessage = "Invalid password format.";
                        break;
                    case "ip_not_found":
                        matchMessage = "Couldn't determine client IP address.";
                        break;
                    case "not_found":
                        matchMessage = "User by this username doesn't exist.";
                        break;
                    case "UserNotAllowed":
                        matchMessage = "User not allowed to login.";
                        break;
                    case "email_not_confirmed":
                        matchMessage = "You need to confirm your email before logging in.";
                        break;
                    case "UserLockedOut":
                        matchMessage = $"User has been locked out for {_userParameters.LockoutLength.TotalMinutes} minutes.";
                        break;
                    case "invalid_credentials":
                        matchMessage = $"Wrong username or password. You have {_userParameters.MaxLoginAttempts - int.Parse(response.ErrorResponse.ErrorValue)} attempts left.";
                        break;
                    default:
                        matchMessage = "Error logging in.";
                        break;
                }
                messages.Add(matchMessage);
            }

            return messages;
        }

        private string ProcessSuccess(LoginResponse response)
        {
            if (response.IsSuccessful)
                return $"Welcome, {response.User.Username}!";
            else
                return string.Empty;
        }

    }
}
