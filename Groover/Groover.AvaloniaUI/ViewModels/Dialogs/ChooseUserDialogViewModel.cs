using Groover.AvaloniaUI.Models.DTOs;
using Groover.AvaloniaUI.Services.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using ReactiveUI.Validation.Extensions;

namespace Groover.AvaloniaUI.ViewModels.Dialogs
{
    public class ChooseUserDialogViewModel : ReactiveValidationObject
    {
        private IUserService _userService;
        private List<UserViewModel> _currentUsers;

        [Reactive]
        public string TitleText { get; set; }
        [Reactive]
        public string YesButtonText { get; set; }
        [Reactive]
        public string NoButtonText { get; set; }
        [Reactive]
        public string DisplayError { get; set; }

        /// <summary>
        /// Holds the ID and denotes whether the username has been checked. If it hasn't, it's is null.
        /// </summary>
        [Reactive]
        public int? UsernameId { get; set; }

        private string _currentUsername;
        public string CurrentUsername 
        { 
            get => _currentUsername; 
            set
            {
                UsernameId = null;
                this.RaiseAndSetIfChanged(ref _currentUsername, value);
            }
        }

        public ReactiveCommand<Unit, int?> YesCommand { get; }
        public ReactiveCommand<Unit, int?> NoCommand { get; }
        public ReactiveCommand<string, int?> CheckCommand { get; }

        public ChooseUserDialogViewModel(List<UserViewModel> currentUsers, IUserService userService)
        {
            TitleText = "Invite user";
            YesButtonText = "SEND INVITE";
            NoButtonText = "CANCEL";
            UsernameId = null;

            _userService = userService;
            _currentUsers = currentUsers ?? new List<UserViewModel>();

            this.ValidationRule(vm => vm.CurrentUsername, username => !string.IsNullOrWhiteSpace(username), "Username can't be empty.");
            this.ValidationRule(vm => vm.UsernameId, userId => userId != null, "Invalid user.");

            YesCommand = ReactiveCommand.Create<Unit, int?>(x => UsernameId, this.IsValid());
            NoCommand = ReactiveCommand.Create<Unit, int?>(x => null);
            CheckCommand = ReactiveCommand.CreateFromTask<string, int?>(x => CheckUsername(x), this.WhenAnyValue(vm => vm.UsernameId)
                                                                                     .Select(val => val == null));
            
            CheckCommand.BindTo(this, x => x.UsernameId);
        }

        public async Task<int?> CheckUsername(string username)
        {
            DisplayError = null;

            var alreadyInGroup = this._currentUsers.Any(u => u.Username == username);
            if (alreadyInGroup)
            {
                DisplayError = "User already in group.";
                return null;
            }

            var userResponse = await _userService.GetByUsernameAsync(username);
            if (!userResponse.IsSuccessful)
            {
                switch (userResponse.StatusCode)
                {
                    case System.Net.HttpStatusCode.NotFound:
                        DisplayError = "User not found.";
                        break;
                    case System.Net.HttpStatusCode.BadRequest:
                        DisplayError = "Bad username format.";
                        break;
                    case System.Net.HttpStatusCode.Unauthorized:
                        DisplayError = "Unathorized access.";
                        break;
                    default:
                        DisplayError = "Unknown error.";
                        break;
                }
                return null;
            }
            else
            {
                return userResponse.Id;
            }    
        }
    }
}
