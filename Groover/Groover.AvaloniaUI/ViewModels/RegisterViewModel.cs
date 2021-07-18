using Groover.AvaloniaUI.Models;
using Groover.AvaloniaUI.Models.Requests;
using Groover.AvaloniaUI.Models.Responses;
using Groover.AvaloniaUI.Services.Interfaces;
using Groover.AvaloniaUI.Utils;
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
    public class RegisterViewModel : ReactiveValidationObject
    {
        private IUserService _userService;
        private UserConstants _userParameters;

        [ObservableAsProperty]
        public bool RegisteredSuccessfully { get; }

        [Reactive]
        public string Username { get; set; } = string.Empty;
        [Reactive]
        public string Password { get; set; } = string.Empty;
        [Reactive]
        public string ConfirmPassword { get; set; } = string.Empty;
        [Reactive]
        public string Email { get; set; } = string.Empty;

        [ObservableAsProperty]
        public RegisterResponse Response { get; }

        [ObservableAsProperty]
        public List<string> ErrorMessages { get; }

        [ObservableAsProperty]
        public string SuccessMessage { get; }

        public ReactiveCommand<Unit, RegisterResponse> Register { get; }

        private ReactiveCommand<RegisterResponse, List<string>> GenerateErrorMessages { get; }
        private ReactiveCommand<RegisterResponse, string> GenerateSuccessMessage { get; }

        public RegisterViewModel(IUserService userService, UserConstants userParameters)
        {
            _userService = userService;
            _userParameters = userParameters;

            this.ValidationRule(x => x.Username, name => !string.IsNullOrWhiteSpace(name), "Username cannot be empty.");
            this.ValidationRule(x => x.Password, pass => !string.IsNullOrWhiteSpace(pass), "Password cannot be empty.");
            this.ValidationRule(x => x.ConfirmPassword, pass => !string.IsNullOrWhiteSpace(pass), "Confirm password cannot be empty.");
            this.ValidationRule(x => x.Email, email => EmailValidator.IsValid(email), "Email is not valid.");
            this.ValidationRule(x => x.RegisteredSuccessfully, loginSuccess => loginSuccess == false, "Already logged in!");

            IObservable<bool> passwordsObservable = this.WhenAnyValue(
                                x => x.Password,
                                x => x.ConfirmPassword,
                                (password, confirmation) => password == confirmation);
            this.ValidationRule(vm => vm.ConfirmPassword, passwordsObservable, "Passwords do not match!");

            Register = ReactiveCommand.CreateFromTask(RegisterAsync, canExecute: this.IsValid());
            Register.ToPropertyEx(this, x => x.Response);
            this.WhenAnyValue(v => v.Response)
                .Where(r => r != null)
                .Select(r => r.IsSuccessful)
                .ToPropertyEx(this, x => x.RegisteredSuccessfully);

            GenerateErrorMessages = ReactiveCommand.Create<RegisterResponse, List<string>>(ProcessErrors);
            GenerateErrorMessages.ToPropertyEx(this, x => x.ErrorMessages);

            GenerateSuccessMessage = ReactiveCommand.Create<RegisterResponse, string>(ProcessSuccess);
            GenerateSuccessMessage.ToPropertyEx(this, x => x.SuccessMessage);
        }

        private async Task<RegisterResponse> RegisterAsync()
        {
            //await Task.Delay(2000);

            //var response = new RegisterResponse();

            //await GenerateErrorMessages.Execute(response);

            //await GenerateSuccessMessage.Execute(response);

            //return response;

            RegisterRequest request = new RegisterRequest()
            {
                Username = this.Username,
                Password = this.Password,
                Email = this.Email
            };

            var response = await _userService.RegisterAsync(request);

            //process response and display errors if any/display success message
            await GenerateErrorMessages.Execute(response);

            await GenerateSuccessMessage.Execute(response);

            return response;
        }

        private List<string> ProcessErrors(RegisterResponse response)
        {
            List<string> messages = new List<string>();

            if (response.ErrorCodes == null)
                return messages;

            foreach (var code in response.ErrorCodes.Distinct())
            {
                string matchMessage;
                switch (code)
                {
                    case "InvalidUserName":
                        matchMessage = "Invalid username format.";
                        break;
                    case "InvalidEmail":
                        matchMessage = "Invalid email format.";
                        break;
                    case "DuplicateUserName":
                        matchMessage = "User name is already taken.";
                        break;
                    case "DuplicateEmail":
                        matchMessage = "Email address is already taken.";
                        break;
                    case "PasswordTooShort":
                        matchMessage = $"Passwords must be at least {_userParameters.PasswordMinLength} characters.";
                        break;
                    case "PasswordRequiresUniqueChars":
                        matchMessage = $"Passwords must use at least {_userParameters.PasswordMinUnique} different characters.";
                        break;
                    case "PasswordRequiresNonAlphanumeric":
                        matchMessage = "Passwords must have at least one non alphanumeric character.";
                        break;
                    case "PasswordRequiresLower":
                        matchMessage = "Passwords must have at least one lowercase ('a'-'z').";
                        break;
                    case "PasswordRequiresUpper":
                        matchMessage = "Passwords must have at least one uppercase ('A'-'Z').";
                        break;
                    case "PasswordRequiredDigit":
                        matchMessage = "Passwords must have at least one digit ('0'-'9').";
                        break;
                    case "DefaultError":
                        matchMessage = "An error has occured.";
                        break;
                    default:
                        matchMessage = "Error logging in.";
                        break;
                }
                messages.Add(matchMessage);
            }

            return messages;
        }

        private string ProcessSuccess(RegisterResponse response)
        {
            if (response.IsSuccessful)
                return $"Welcome, {response.Username}! Before you login, you will need to confirm your email address.";
            else
                return string.Empty;
        }
    }
}
